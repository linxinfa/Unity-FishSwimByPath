using UnityEngine;

/// <summary>
/// This class provides functions for generating curved meshes around a Spline.
/// </summary>
/// <remarks>
/// This class enables you to dynamically generate curved meshes like streets, rivers, tubes, ropes, tunnels, etc.
/// </remarks>
[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[AddComponentMenu("SuperSplines/Spline Mesh")]
public class SplineMesh : MonoBehaviour
{
	public Spline spline;												///< Reference to the spline that defines the path.
	
	public UpdateMode updateMode = UpdateMode.DontUpdate; 				///< Specifies when the spline mesh will be updated.
	public int deltaFrames = 1;											///< The number of frames that need to pass before the mesh will be updated again. (for UpdateMode.EveryXFrames)
	public float deltaTime = 0.1f;										///< The amount of time that needs to pass before the mesh will be updated again. (for UpdateMode.EveryXSeconds)
	private int updateFrame = 0;
	private float updateTime = 0f;
	
	public Mesh startBaseMesh; 											///< Reference to the base mesh that will be created around the spline.
	public Mesh baseMesh; 												///< Reference to the main base mesh that will be created around the spline
	public Mesh endBaseMesh; 											///< Reference to the base mesh that will be created around the spline.
	
	public int segmentCount = 50; 										///< Number of segments (base meshes) stringed together per generated mesh.
	
	public UVMode uvMode = UVMode.InterpolateV; 						///< Defines how UV coordinates will be calculated.
	public Vector2 uvScale = Vector2.one; 								///< Affects the scale of texture coordinates on the streched mesh
	public Vector2 xyScale = Vector2.one; 								///< Mesh scale in the local directions around the spline.
	
	public bool highAccuracy = false;									///< If set to true, the component will sample the spline for every vertex seperately
	
	public SplitMode splitMode = SplitMode.DontSplit;					///< Defines if and specifies which individual parts of the spline will exclusively be used for mesh generation.
	public float segmentStart = 0; 										///< Index of the spline segment that will be used as control path.
	public float segmentEnd = 1; 										///< Index of the spline segment that will be used as control path.
	public int splineSegment = 0; 										///< Index of the spline segment that will be used as control path.
	
	private MeshData meshDataStart;// = new MeshData( null );
	private MeshData meshDataBase;// = new MeshData( null );
	private MeshData meshDataEnd;//= new MeshData( null );
	
	private MeshData meshDataNew;
	
	private Mesh bentMesh = null;
	
	public Mesh BentMesh{ get{ return ReturnMeshReference( ); } } 		///< Returns a reference to the generated mesh.
	public bool IsSubSegment{ get{ return (splineSegment != -1); } }	/// Returns true if the component only computes a part of the whole spline mesh.
	
	void Start( )
	{
		if( spline != null )
			spline.UpdateSpline( );
		
		UpdateMesh( );
	}
	
	void OnEnable( )
	{
		if( spline != null )
			spline.UpdateSpline( );
		
		UpdateMesh( );
	}
	
	void LateUpdate( )
	{
		switch( updateMode )
		{
		case UpdateMode.DontUpdate:
			break;
			
		case UpdateMode.EveryXFrames:
			if( Time.frameCount % deltaFrames == 0 )
				goto case UpdateMode.EveryFrame;
				
			break;
			
		case UpdateMode.EveryXSeconds:
			if( deltaTime < Time.realtimeSinceStartup - updateTime )
			{
				updateTime = Time.realtimeSinceStartup;
				goto case UpdateMode.EveryFrame;
			}
			
			break;
			
		case UpdateMode.WhenSplineChanged:
			if( updateFrame != spline.UpdateFrame )
			{
				updateFrame = spline.UpdateFrame;
				goto case UpdateMode.EveryFrame;
			}
			
			break;
			
		case UpdateMode.EveryFrame:
			UpdateMesh( );
			
			break;
		}
	}
	
	/// <summary>
	/// This function updates the spline mesh. It is called automatically once in a while, if updateMode isn't set to DontUpdate.
	/// </summary>
	public void UpdateMesh( )
	{
		SetupMesh( );
		
		bentMesh.Clear( );
		
		if( baseMesh == null || spline == null || segmentCount <= 0 )
			return;
		
		SetupMeshBuffers( );
		
		float startParam;
		float endParam;
		
		float deltaParam;
		
		switch( splitMode )
		{
		case SplitMode.BySplineSegment:
			SplineSegment[] splineSegments = spline.SplineSegments;
			
			splineSegment = Mathf.Clamp( splineSegment, 0, splineSegments.Length-1 );
			
			SplineSegment segment = splineSegments[splineSegment];
			
			startParam = (float)segment.StartNode.Parameters[spline].position;
			endParam = startParam + (float)segment.NormalizedLength;
			
			break;
			
		case SplitMode.BySplineParameter:
			startParam = segmentStart;
			endParam = segmentEnd;
			
			break;
			
		case SplitMode.DontSplit:
		default:
			startParam = 0;
			endParam = 1;
			
			break;
		}
		
		deltaParam = endParam - startParam;
		
		float param0 = 0f;
		float param1 = 0f;
		
		SplineMeshModifier[] splineMeshModifiers = GetComponentsInChildren<SplineMeshModifier>( );
		
		for( int segmentIdx = 0; segmentIdx < segmentCount; segmentIdx++ )
		{
			MeshData currentBaseMesh;
			
			if( segmentIdx == 0 && startBaseMesh != null )
				currentBaseMesh = meshDataStart;
			else if( segmentIdx == segmentCount-1 && endBaseMesh != null )
				currentBaseMesh = meshDataEnd;
			else
				currentBaseMesh = meshDataBase;
			
			param0 = startParam + deltaParam * (float) (segmentIdx) / segmentCount;
			param1 = startParam + deltaParam * (float) (segmentIdx+1) / segmentCount;
			
			BendMesh( param0, param1, currentBaseMesh, meshDataNew, splineMeshModifiers );
		}
		
		meshDataNew.AssignToMesh( bentMesh );
	}

	private void BendMesh( float param0, float param1, MeshData meshDataBase, MeshData meshDataNew, SplineMeshModifier[] meshModiefiers )
	{
		float paramOffset = param1 - param0;
		
		Vector3 targetPos;
		Quaternion targetRot;
		
		Vector3 pos0 = Vector3.zero;
		Vector3 pos1 = Vector3.zero;
		
		Quaternion rot0 = Quaternion.identity;
		Quaternion rot1 = Quaternion.identity;
		
		Quaternion inverseRotation = Quaternion.Inverse( spline.transform.rotation );
		
		int firstVertexIndex = meshDataNew.currentVertexIndex;
		
		if( !highAccuracy )
		{
			pos0 = spline.transform.InverseTransformPoint(spline.GetPositionOnSpline( param0 ));
			pos1 = spline.transform.InverseTransformPoint(spline.GetPositionOnSpline( param1 ));
			
			rot0 = spline.GetOrientationOnSpline( param0 ) * inverseRotation;
			rot1 = spline.GetOrientationOnSpline( param1 ) * inverseRotation;
		}
		
		for( int i = 0; i < meshDataBase.VertexCount; i++, meshDataNew.currentVertexIndex++ )
		{
			Vector3 vertex = meshDataBase.vertices[i];
			Vector2 uvCoord = meshDataBase.uvCoord[i];
			
			float normalizedZPos = vertex.z + 0.5f;
			
			float splineParam = param0 + paramOffset * normalizedZPos;
			
			switch( uvMode )
			{
			case UVMode.InterpolateU:
				uvCoord.x = splineParam;
				break;
				
			case UVMode.InterpolateV:
				uvCoord.y = splineParam;
				break;
			}
			
			uvCoord.x *= uvScale.x;
			uvCoord.y *= uvScale.y;
			
			if( highAccuracy )
			{
				targetRot = spline.GetOrientationOnSpline( splineParam ) * inverseRotation;
				targetPos = spline.transform.InverseTransformPoint( spline.GetPositionOnSpline( splineParam ) );
			}
			else
			{
				targetRot = Quaternion.Lerp( rot0, rot1, normalizedZPos );
				targetPos = new Vector3( 
					pos0.x + (pos1.x-pos0.x) * normalizedZPos, 
					pos0.y + (pos1.y-pos0.y) * normalizedZPos, 
					pos0.z + (pos1.z-pos0.z) * normalizedZPos );
			}
			
			vertex.x *= xyScale.x;
			vertex.y *= xyScale.y;
			vertex.z = 0;
			
			foreach( SplineMeshModifier meshModifier in meshModiefiers )
				vertex = meshModifier.ModifyVertex( this, vertex, splineParam );
			
			meshDataNew.vertices[meshDataNew.currentVertexIndex] = FastRotation( targetRot, vertex ) + targetPos;
			
			if( meshDataBase.HasNormals )
			{
				Vector3 normal = meshDataBase.normals[i];
				
				foreach( SplineMeshModifier meshModifier in meshModiefiers )
					normal = meshModifier.ModifyNormal( this, normal, splineParam );
				
				meshDataNew.normals[meshDataNew.currentVertexIndex] = targetRot * normal;
			}
			
			if( meshDataBase.HasTangents )
			{
				Vector4 tangent = meshDataBase.tangents[i];
				
				foreach( SplineMeshModifier meshModifier in meshModiefiers )
					tangent = meshModifier.ModifyTangent( this, tangent, splineParam );
				
				meshDataNew.tangents[meshDataNew.currentVertexIndex] = targetRot * tangent;
			}
			
			foreach( SplineMeshModifier meshModifier in meshModiefiers )
				uvCoord = meshModifier.ModifyUV( this, uvCoord, splineParam );
			
			meshDataNew.uvCoord[meshDataNew.currentVertexIndex] = uvCoord;
		}
		
		for( int i = 0; i < meshDataBase.TriangleCount; i++, meshDataNew.currentTriangleIndex++ )
			meshDataNew.triangles[meshDataNew.currentTriangleIndex] = meshDataBase.triangles[i] + firstVertexIndex;
	}
	
	private Vector3 FastRotation( Quaternion rotation, Vector3 point )
	{
		float num = rotation.x * 2f;
		float num2 = rotation.y * 2f;
		float num3 = rotation.z * 2f;
		float num4 = rotation.x * num;
		float num5 = rotation.y * num2;
		float num6 = rotation.z * num3;	
		float num7 = rotation.x * num2;
		float num8 = rotation.x * num3;
		float num9 = rotation.y * num3;
		float num10 = rotation.w * num;
		float num11 = rotation.w * num2;
		float num12 = rotation.w * num3;
		
		Vector3 result;
		
		result.x = (1f - (num5 + num6)) * point.x + (num7 - num12) * point.y;
		result.y = (num7 + num12) * point.x + (1f - (num4 + num6)) * point.y;
		result.z = (num8 - num11) * point.x + (num9 + num10) * point.y;
		
		return result;
	}
		
	private void SetupMesh( )
	{
		if( bentMesh == null )
		{
			bentMesh = new Mesh( );
		
			bentMesh.name = "BentMesh";
			bentMesh.hideFlags = HideFlags.HideAndDontSave;
		}
		
		MeshFilter meshFilter = GetComponent<MeshFilter>( );
		
		if( meshFilter.sharedMesh != bentMesh )
			meshFilter.sharedMesh = bentMesh;
		
		
		MeshCollider meshCollider = GetComponent<MeshCollider>( );
		
		if( meshCollider != null )
		{
			meshCollider.sharedMesh = null;
			meshCollider.sharedMesh = bentMesh;
		}
		
	}
	
	private void SetupMeshBuffers( )
	{
		if( meshDataStart == null ) meshDataStart = new MeshData( null );
		if( meshDataBase == null ) meshDataBase = new MeshData( null );
		if( meshDataEnd == null ) meshDataEnd = new MeshData( null );
		if( meshDataNew == null ) meshDataNew = new MeshData( null );
		
		if( !meshDataStart.ReferencesMesh( startBaseMesh ) )
			meshDataStart = new MeshData( startBaseMesh );
		
		if( !meshDataBase.ReferencesMesh( baseMesh ) )
			meshDataBase = new MeshData( baseMesh );
		
		if( !meshDataEnd.ReferencesMesh( endBaseMesh ) )
			meshDataEnd = new MeshData( endBaseMesh );
		
		MeshData[] capMeshes = new MeshData[] {meshDataStart, meshDataEnd};
		
		int middleSegmentCount = segmentCount;
		
//		if( IsSubSegment )
//		{
//			if( splineSegment != 0 )
//			{
//				capMeshes[0] = null;
//				++middleSegmentCount;
//			}
//			
//			if( splineSegment != spline.SegmentCount - 1 )
//			{
//				capMeshes[1] = null;
//				++middleSegmentCount;
//			}
//		}
		
		if( startBaseMesh != null )
			--middleSegmentCount;
		
		if( endBaseMesh != null )
			--middleSegmentCount;
		
		if( !meshDataNew.Suits( meshDataBase, middleSegmentCount, capMeshes ) )
			meshDataNew = new MeshData( meshDataBase, middleSegmentCount, capMeshes );
		else
			meshDataNew.Reset( );
	}
	
	private Mesh ReturnMeshReference( )
	{
		return bentMesh;
	}
	
	private class MeshData
	{
		public Vector3[] vertices;
		public Vector2[] uvCoord;
		public Vector3[] normals;
		public Vector4[] tangents;
		
		public int[] triangles;
		
		public Bounds bounds;
		
		public int currentTriangleIndex;
		public int currentVertexIndex;
		
		public bool HasNormals;
		public bool HasTangents;
		
		public int VertexCount{ get{ return vertices.Length; } }
		public int TriangleCount{ get{ return triangles.Length; } }
		
		public Mesh referencedMesh = null;
		
		public MeshData( Mesh mesh )
		{
			referencedMesh = mesh;
			
			currentTriangleIndex = 0;
			currentVertexIndex = 0;
				
			if( mesh == null )
			{
				vertices = new Vector3[0];
				normals = new Vector3[0];
				tangents = new Vector4[0];
				uvCoord = new Vector2[0];
				triangles = new int[0];
				bounds = new Bounds( Vector3.zero, Vector3.zero );
				
				HasNormals = normals.Length > 0;
				HasTangents = tangents.Length > 0;
				
				return;
			}
			
			vertices = mesh.vertices;
			normals = mesh.normals;
			tangents = mesh.tangents;
			uvCoord = mesh.uv;
			
			triangles = mesh.triangles;
			
			bounds = mesh.bounds;
			
			HasNormals = normals.Length > 0;
			HasTangents = tangents.Length > 0;
		}
		
		public MeshData( MeshData mData, int segmentCount, MeshData[] additionalMeshes )
		{
			int totalVertexCount = mData.vertices.Length * segmentCount;
			int totalUVCount = mData.uvCoord.Length * segmentCount;
			int totalNormalsCount = mData.normals.Length * segmentCount;
			int totalTangentsCount = mData.tangents.Length * segmentCount;
			int totalTrianglesCount = mData.triangles.Length * segmentCount;
			
			foreach( MeshData meshData in additionalMeshes )
			{
				if( meshData == null )
					continue;
				
				totalVertexCount += meshData.vertices.Length;
				totalUVCount += meshData.uvCoord.Length;
				totalNormalsCount += meshData.normals.Length;
				totalTangentsCount += meshData.tangents.Length;
				
				totalTrianglesCount += meshData.triangles.Length;
			}
			
			vertices = new Vector3[totalVertexCount];
			uvCoord = new Vector2[totalUVCount];
			normals = new Vector3[totalNormalsCount];
			tangents = new Vector4[totalTangentsCount];
			triangles = new int[totalTrianglesCount];
			
			HasNormals = normals.Length > 0;
			HasTangents = tangents.Length > 0;
		}
		
		public bool Suits( MeshData mData, int segmentCount, MeshData[] additionalMeshes )
		{
			int totalVertexCount = mData.vertices.Length * segmentCount;
			int totalUVCount = mData.uvCoord.Length * segmentCount;
			int totalNormalsCount = mData.normals.Length * segmentCount;
			int totalTangentsCount = mData.tangents.Length * segmentCount;
			int totalTrianglesCount = mData.triangles.Length * segmentCount;
			
			foreach( MeshData meshData in additionalMeshes )
			{
				if( meshData == null )
					continue;
				
				totalVertexCount += meshData.vertices.Length;
				totalUVCount += meshData.uvCoord.Length;
				totalNormalsCount += meshData.normals.Length;
				totalTangentsCount += meshData.tangents.Length;
				
				totalTrianglesCount += meshData.triangles.Length;
			}
			
			if( totalVertexCount != vertices.Length )
				return false;
			if( totalUVCount != uvCoord.Length )
				return false;
			if( totalNormalsCount != normals.Length )
				return false;
			if( totalTangentsCount != tangents.Length )
				return false;
			if( totalTrianglesCount != triangles.Length )
				return false;
			
			return true;
		}
		
		public bool ReferencesMesh( Mesh mesh )
		{
			return referencedMesh == mesh;
		}
		
		public void Reset( )
		{
			currentTriangleIndex = 0;
			currentVertexIndex = 0;
		}
		
		public void AssignToMesh( Mesh mesh )
		{
			mesh.vertices = vertices;
			mesh.uv = uvCoord;
			
			if( HasNormals )
				mesh.normals = normals;
			
			if( HasTangents )
				mesh.tangents = tangents;
			
			mesh.triangles = triangles;
		}
	}
	
	/// <summary>
	/// Defines how the SplineMesh class will calculate UV-coordinates.
	/// </summary>
	public enum UVMode
	{
		InterpolateV, ///< UV coordinates will be interpolated on the V-axis
		InterpolateU, ///< UV coordinates will be interpolated on the U-axis
		DontInterpolate ///< UV coordinates will simply be copied from the base mesh and won't be altered.
	}
	
	/// <summary>
	/// Defines if and specifies which individual parts of the spline will exclusively be used for mesh generation.
	/// </summary>
	public enum SplitMode
	{
		DontSplit,
		BySplineSegment,
		BySplineParameter
	}
	
	/// <summary>
	/// Specifies when to update and recalculate an instance of SplineMesh.
	/// </summary>
	public enum UpdateMode
	{
		DontUpdate, 	///< Keeps the spline static. It will only be updated when the component becomes enabled (OnEnable( )).
		EveryFrame, 	///< Updates the spline every frame.
		EveryXFrames, 	///< Updates the spline every x frames.
		EveryXSeconds, 	///< Updates the spline every x seconds.
		WhenSplineChanged ///< Updates the spline mesh whenever its reference spline has been updated.
	}
}

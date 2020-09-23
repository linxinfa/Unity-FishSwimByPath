using UnityEngine;

using System;
using System.Collections.Generic;

/// <summary>
/// The Spline class represents three-dimensional curves.
/// </summary>
/// <remarks>
/// It provides the most important functions that are necessary to create, calculate and render Splines.
/// The class derives from MonoBehaviour so it can be attached to gameObjects and used like any other self-written script.
/// </remarks>
[AddComponentMenu("SuperSplines/Spline")]
public partial class Spline : MonoBehaviour
{
	public List<SplineNode> splineNodesArray = new List<SplineNode>( ); 									///< A collection of SplineNodes that are used as control nodes.
	private List<SplineNode> splineNodesInternal = new List<SplineNode>( );
	
	public InterpolationMode interpolationMode = InterpolationMode.Hermite; 								///< Specifies what kind of curve interpolation will be used.
	public RotationMode rotationMode = RotationMode.Tangent; 												///< Specifies how to calculate rotations on the spline.
	public TangentMode tangentMode = TangentMode.UseTangents; 												///< Specifies how tangents are calculated in hermite mode.
	public NormalMode normalMode = NormalMode.UseGlobalSplineNormal;										///< Specifies how the spline's normal is defined. (mostly needed for RotationMode.Tangent)
	
	public UpdateMode updateMode = UpdateMode.DontUpdate; 													///< Specifies when the spline will be updated.
	public int deltaFrames = 1;																				///< The number of frames that need to pass before the spline will be updated again. (for UpdateMode.EveryXFrames)
	public float deltaTime = 0.1f;																			///< The amount of time that needs to pass before the spline will be updated again. (for UpdateMode.EveryXSeconds)
	private int updateFrame = 0;
	private float updateTime = 0f;
	
	public bool perNodeTension = false;																		///< If true, the curve's tension can be defined per node 
	public float tension = 0.5f; 																			///< Curve Tension (only has an effect on Hermite splines).
	public Vector3 normal = Vector3.up; 																	///< Spline's Normal / Up-Vector used to calculate rotations (only needed for RotationMode.Tangent)
	
	public bool autoClose = false; 																			///< If set to true the spline start and end points of the spline will be connected. (Note that Bézier-Curves can't be auto-closed!)
	public int interpolationAccuracy = 5; 																	///< Defines how accurately numeric calculations will be done.
	
	private LengthData lengthData = new LengthData( );
	
	public float Length			{ get{ return (float) lengthData.length; } } 								///< Returns the length of the spline in game units.
	public bool AutoClose		{ get{ return autoClose && interpolationMode!=InterpolationMode.Bezier; } } ///< Returns true if spline is auto-closed. If the spline is a Bézier-Curve, false will always be returned.
	public int NodesPerSegment	{ get{ return IsBezier ? 3 : 1; } }											///< Returns the number of spline nodes that are needed to describe a spline segment.
	public int SegmentCount		{ get{ return Mathf.Max((ControlNodeCount-1)/NodesPerSegment,0); } } 		///< Returns the number of spline segments. (Note that a spline segment of a Bézier-Curve is defined by 4 control nodes!)
	
	public bool HasBeenUpdated	{ get{ return updateFrame >= Time.frameCount-1; } }							///< Returns true if the spline has been updated in the current or previous frame.
	public int UpdateFrame	{ get{ return updateFrame; } }													///< Returns the frame in which the spline has lastly been updated.
	
	private int ControlNodeCount{ get{ return AutoClose ? splineNodesInternal.Count + 1 : splineNodesInternal.Count; } }
	private double InvertedAccuracy{ get{ return 1.0 / interpolationAccuracy; } }
	private bool IsBezier{ get{ return interpolationMode == InterpolationMode.Bezier; } }
	private bool HasNodes{ get{ return splineNodesInternal.Count > 0; } }
	
	
	/// <summary>
	/// Returns an array containing all relevant control nodes that are used internally. 
	/// </summary>
	/// <remarks>
	/// Because references to not existing spline nodes and spline nodes that can't be used (more or less than x*3+1 nodes in bézier mode) are removed from 
	/// the internal node array this array might differ from the values in the splineNodesArray.
	/// </remarks>
	public SplineNode[] SplineNodes
	{
		get{ 
			if( splineNodesInternal == null ) 
				splineNodesInternal = new List<SplineNode>( );
			
			return splineNodesInternal.ToArray( ); 
		} 
	} ///< 
	
	/// <summary>
	/// Returns an array containing the start and end nodes of the spline's segments. 
	/// </summary>
	/// <remarks>
	/// If the used interpolation method isn't bézier-interpolation, it is identical to the returned array of SplineNodes.
	/// </remarks>
	public SplineNode[] SegmentNodes
	{
		get{ 
			if( !IsBezier )
				return SplineNodes;
			
			List<SplineNode> nodes = new List<SplineNode>( );
			
			for( int i = 0; i < splineNodesInternal.Count; i+=NodesPerSegment )
				nodes.Add( splineNodesInternal[i] );
			
			return nodes.ToArray( );
		} 
	}
	
	/// <summary>
	/// Returns an array containing the spline's segments. 
	/// </summary>
	public SplineSegment[] SplineSegments
	{
		get {
			SplineSegment[] sSegments = new SplineSegment[SegmentCount];
			
			for( int i = 0; i < sSegments.Length; i++ )
				sSegments[i] = new SplineSegment( this, GetNode( i*NodesPerSegment, 0 ), GetNode( i*NodesPerSegment, NodesPerSegment ) );
			
			return sSegments;
		}
	} 
	
	void OnEnable( ) 
	{
		UpdateSpline( );
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
			
//		case UpdateMode.WhenNodeMoved:
//			bool transformChanged = false;
//			
//			foreach( SplineNode node in splineNodesInternal )
//			{
//				if( node != null )
//				{
//					if( node.transform.hasChanged )
//					{
//						node.transform.hasChanged = false;
//						transformChanged = true;
//					}
//				}
//			}
//			
//			if( transformChanged )
//				goto case UpdateMode.EveryFrame;
//			
//			break;
			
		case UpdateMode.EveryFrame:
			UpdateSpline( );
			break;
		}
	}
	
	/// <summary>
	/// This function updates the spline. It is called automatically once in a while, if updateMode isn't set to DontUpdate.
	/// </summary>
	public void UpdateSpline( )
	{
		switch( interpolationMode )
		{
		case InterpolationMode.Linear:
			if( !(splineInterpolator is LinearInterpolator) )
				splineInterpolator = new LinearInterpolator( );
			break;
			
		case InterpolationMode.Bezier:
			if( !(splineInterpolator is BezierInterpolator) )
				splineInterpolator = new BezierInterpolator( );
			break;
			
		case InterpolationMode.Hermite:
			if( !(splineInterpolator is HermiteInterpolator) )
				splineInterpolator = new HermiteInterpolator( );
			break;
			
		case InterpolationMode.BSpline:
			if( !(splineInterpolator is BSplineInterpolator) )
				splineInterpolator = new BSplineInterpolator( );
			break;
			
		}
		
		//Count valid spline nodes
		int validNodes = 0;
		
		foreach( SplineNode sNode in splineNodesArray )
			if( sNode != null )
				++validNodes;
		
		//Get relevant count
		int relevantNodeCount = GetRelevantNodeCount( validNodes );
		
		//Initialize the internal node array
		if( splineNodesInternal == null )
			splineNodesInternal = new List<SplineNode>( );
		
		splineNodesInternal.Clear( );
		
		if( !EnoughNodes( relevantNodeCount ) )
			return;
		
		splineNodesInternal.AddRange( splineNodesArray.GetRange( 0, relevantNodeCount ) );
		splineNodesInternal.Remove( null );
		
		ReparameterizeCurve( );
		
		updateFrame = Time.frameCount;
	}
	
	/// <summary>
	/// This function returns a point on the spline for a parameter between 0 and 1
	/// </summary>
	/// <returns>
	/// A point on the spline.
	/// </returns>
	/// <param name='param'>
	/// A normalized spline parameter ([0..1]).
	/// </param>
	public Vector3 GetPositionOnSpline( float param )
	{
		if( !HasNodes )
			return Vector3.zero;
		
		return GetPositionInternal( RecalculateParameter( param ) );
	}
	
	/// <summary>
	/// This function returns a tangent to the spline for a parameter between 0 and 1
	/// </summary>
	/// <returns>
	/// A tangent to the spline.
	/// </returns>
	/// <param name='param'>
	/// A normalized spline parameter ([0..1]).
	/// </param>
	public Vector3 GetTangentToSpline( float param )
	{
		if( !HasNodes )
			return Vector3.zero;
		
		return GetTangentInternal( RecalculateParameter( param ) );
	}
	
	/// <summary>
	/// This function returns a normal to the spline for a parameter between 0 and 1.
	/// </summary>
	/// <remarks>
	/// If per-node normals are enabled, it will interpolate the spline's normals. Otherwise it will use the spline's default normal.
	/// </remarks>
	/// <returns>
	/// A normal to the spline.
	/// </returns>
	/// <param name='param'>
	/// A normalized spline parameter ([0..1]).
	/// </param>
	public Vector3 GetNormalToSpline( float param )
	{
		if( !HasNodes )
			return Vector3.zero;
		
		if( normalMode != NormalMode.UseGlobalSplineNormal )
			return GetNormalInternal( RecalculateParameter( param ) );
		else
			return normal.normalized;
	}
	
	/// <summary>
	/// This function returns the curvature of the spline for a parameter between 0 and 1
	/// </summary>
	/// <returns>
	/// The local curvature of the spline at a specific location.
	/// </returns>
	/// <param name='param'>
	/// A normalized spline parameter ([0..1]).
	/// </param>
	public Vector3 GetCurvatureOfSpline( float param )
	{
		if( !HasNodes )
			return Vector3.zero;
		
		return GetCurvatureInternal( RecalculateParameter( param ) );
	}
	
	/// <summary>
	/// This function returns a rotation on the spline for a parameter between 0 and 1
	/// </summary>
	/// <returns>
	/// A rotation on the spline..
	/// </returns>
	/// <param name='param'>
	/// A normalized spline parameter ([0..1]).
	/// </param>
	public Quaternion GetOrientationOnSpline( float param )
	{
		if( !HasNodes )
			return Quaternion.identity;
		
		switch( rotationMode )
		{
		case RotationMode.Tangent:
			SegmentParameter sParam = RecalculateParameter( param );
			
			Vector3 tangent = GetTangentInternal( sParam );
			Vector3 normal = GetNormalInternal( sParam );
			
			if( tangent.sqrMagnitude == 0f || normal.sqrMagnitude == 0f )
				return Quaternion.identity;
			
			return Quaternion.LookRotation( tangent, normal );
			
		case RotationMode.Node:
			return GetRotationInternal( RecalculateParameter( param ) );
			
		default:
			return Quaternion.identity;
		}
		
	}
	
	/// <summary>
	/// This function returns an interpolated custom value on the spline for a parameter between 0 and 1.
	/// </summary>
	/// <remarks>
	/// The control values can be set in the SplineNode inspector or in the SplineNode script. These control values will be interpolated just like 
	/// the SplineNodes' control positions are. Depending on the used interpolation mode, the actual control values won't be elements of the set of the interpolated values. 
	/// Such a behaviour applies to B-splines for example. Just like the B-spline doesn't necessarily contain all control positions, its interpolated
	/// custom values don't necessarily contain all custom control values.
	/// </remarks>
	/// <returns>
	/// An interpolated custom value on the spline.
	/// </returns>
	/// <param name='param'>
	/// A normalized spline parameter ([0..1]).
	/// </param>
	public float GetCustomValueOnSpline( float param )
	{
		if( !HasNodes )
			return 0f;
		
		return GetValueInternal( RecalculateParameter( param ) );
	}
	
	private Vector3 GetPositionInternal( SegmentParameter sParam )
	{
		return splineInterpolator.InterpolateVector( this, sParam.normalizedParam, sParam.normalizedIndex, AutoClose, splineNodesInternal, 0 );
	}
	
	private Vector3 GetTangentInternal( SegmentParameter sParam )
	{
		return splineInterpolator.InterpolateVector( this, sParam.normalizedParam, sParam.normalizedIndex, AutoClose, splineNodesInternal, 1 );
	}
	
	private Vector3 GetNormalInternal( SegmentParameter sParam )
	{
		SplineNode n0; SplineNode n1;
		SplineNode n2; SplineNode n3;
		
		splineInterpolator.GetNodeData( splineNodesInternal, sParam.normalizedIndex, AutoClose, out n0, out n1, out n2, out n3 ); 
		
		Vector3 normal0;
		Vector3 normal1;
		Vector3 normal2;
		Vector3 normal3;
		
		if( normalMode == NormalMode.UseNodeNormal )
		{
			normal0 = n0.transform.TransformDirection( n0.normal ).normalized;
			normal1 = n1.transform.TransformDirection( n1.normal ).normalized;
			normal2 = n2.transform.TransformDirection( n2.normal ).normalized;
			normal3 = n3.transform.TransformDirection( n3.normal ).normalized;
		}
		else
		{
			normal0 = n0.transform.up;
			normal1 = n1.transform.up;
			normal2 = n2.transform.up;
			normal3 = n3.transform.up;
		}
		
		if( splineInterpolator is HermiteInterpolator )
		{
			HermiteInterpolator hermiteInterpolator = splineInterpolator as HermiteInterpolator;
			hermiteInterpolator.RecalcVectors( this, n0, n1, ref normal2, ref normal3 );
		}
		
		return splineInterpolator.InterpolateVector( sParam.normalizedParam, normal0, normal1, normal2.normalized, normal3.normalized, 0 ).normalized;
	}
	
	private Vector3 GetCurvatureInternal( SegmentParameter sParam )
	{
		return splineInterpolator.InterpolateVector( this, sParam.normalizedParam, sParam.normalizedIndex, AutoClose, splineNodesInternal, 2 );
	}
	
	private float GetValueInternal( SegmentParameter sParam )
	{
		return splineInterpolator.InterpolateValue( this, sParam.normalizedParam, sParam.normalizedIndex, AutoClose, splineNodesInternal, 0 );
	}
	
	private Quaternion GetRotationInternal( SegmentParameter sParam )
	{
		return splineInterpolator.InterpolateRotation( this, sParam.normalizedParam, sParam.normalizedIndex, AutoClose, splineNodesInternal, 0 );
	}
	
	/// <summary>
	/// This function returns a spline segment that contains the point on the spline that is defined by a normalized parameter.
	/// </summary>
	/// <returns>
	/// A spline segment containing the point corresponding to param.
	/// </returns>
	/// <param name='param'>
	/// A normalized spline parameter ([0..1]).
	/// </param>
	public SplineSegment GetSplineSegment( float param )
	{
		param = Mathf.Clamp01( param );
		
		foreach( SplineSegment segment in SplineSegments )
			if( segment.IsParameterInRange( param ) )
				return segment;
		
		return null;
	}
	
	/// <summary>
	/// This function converts a normalized spline parameter to the actual distance to the spline's start point.
	/// </summary>
	/// <returns>
	/// The actual distance from the start point to the point defined by param.
	/// </returns>
	/// <param name='param'>
	/// A normalized spline parameter ([0..1]).
	/// </param>
	public float ConvertNormalizedParameterToDistance( float param )
	{
		return Length * param;
	}
	
	/// <summary>
	/// This function converts an actual distance from the spline's start point to normalized spline parameter.
	/// </summary>
	/// <returns>
	/// A normalized spline parameter based on the distance from the splines start point.
	/// </returns>
	/// <param name='param'>
	/// A specific distance of a point on the spline from its starting point (must be less or equal to the spline length).
	/// </param>
	public float ConvertDistanceToNormalizedParameter( float param )
	{
		return (Length <= 0f) ? 0f : param/Length;
	}
	
	/// <summary>
	/// Use this function to quickly append a new SplineNode at the spline's end.
	/// </summary>
	/// <returns>
	/// A new GameObject that has a SplineNode-Component attached to it.
	/// </returns>
	public GameObject AddSplineNode( )
	{
		if( splineNodesArray.Count > 0 )
			return AddSplineNode( splineNodesArray[splineNodesArray.Count-1] );
		else
			return AddSplineNode( null );
	}
	
	/// <summary>
	/// Use this function to quickly insert a new SplineNode.
	/// </summary>
	/// <returns>
	/// A new GameObject that has a SplineNode-Component attached to it.
	/// </returns>
	/// <param name='normalizedParam'>
	/// A normalized spline parameter, that defines where the new SplineNode will be inserted. 
	/// </param>
	public GameObject AddSplineNode( float normalizedParam )
	{	
		if( SplineNodes.Length == 0 )
			return AddSplineNode( );
	
		SplineNode previousNode = null;
		
		foreach( SplineNode sNode in SplineNodes )
		{
			if( sNode.Parameters[this].position >= normalizedParam )
				return AddSplineNode( previousNode ); 
			
			previousNode = sNode;
		}
		
		return AddSplineNode( splineNodesArray[splineNodesArray.Count - 1] );
	}
	
	/// <summary>
	/// Use this function to quickly insert a new SplineNode.
	/// </summary>
	/// <returns>
	/// A new GameObject that has a SplineNode-Component attached to it.
	/// </returns>
	/// <param name='precedingNode'>
	/// A reference to a SplineNode after which the new SplineNode will be inserted.
	/// </param>
	public GameObject AddSplineNode( SplineNode precedingNode )
	{
		GameObject gObject = new GameObject( );
		
		SplineNode splineNode = gObject.AddComponent<SplineNode>( );
		
		int insertIndex;
		
		if( precedingNode == null )
			insertIndex = 0;
		else
			insertIndex = splineNodesArray.IndexOf( precedingNode ) + 1;
			
		if( insertIndex == -1 )
			throw( new ArgumentException( "The SplineNode referenced by \"percedingNode\" is not part of the spline " + gameObject.name ) );
		
		splineNodesArray.Insert( insertIndex, splineNode );
		
		UpdateSpline( );
		
		return gObject;
	}
	
	/// <summary>
	/// Use this function to quickly remove a new SplineNode.
	/// </summary>
	/// <param name='gObject'>
	/// A reference to the gameObject that the SplineNode is attached to.
	/// </param>
	public void RemoveSplineNode( GameObject gObject )
	{
		SplineNode splineNode = gObject.GetComponent<SplineNode>( );
		
		if( splineNode != null )
			RemoveSplineNode( splineNode );
	}
	
	/// <summary>
	/// Use this function to quickly remove a new SplineNode.
	/// </summary>
	/// <param name='splineNode'>
	/// A reference to the SplineNode that shall be removed.
	/// </param>
	public void RemoveSplineNode( SplineNode splineNode )
	{
		splineNodesArray.Remove( splineNode );
		
		UpdateSpline( );
	}
	
	//Recalculate the spline parameter for constant-velocity interpolation
	private SegmentParameter RecalculateParameter( double param )
	{
		if( param <= 0 )
			return new SegmentParameter( 0, 0 );
		if( param > 1 )
			return new SegmentParameter( MaxNodeIndex( ), 1 );
		
		double invertedAccuracy = InvertedAccuracy;
		
		if( lengthData == null )
			lengthData = new LengthData( );
			
		if( lengthData.subSegmentPosition == null )
			lengthData.Calculate( this );
		
		for( int i = lengthData.subSegmentPosition.Length - 1; i >= 0; i-- )
		{
			if( lengthData.subSegmentPosition[i] < param )
			{
				int floorIndex = (i - (i % (interpolationAccuracy)));
				
				int normalizedIndex = floorIndex * NodesPerSegment / interpolationAccuracy;
				double normalizedParam = invertedAccuracy * (i-floorIndex + (param - lengthData.subSegmentPosition[i]) / lengthData.subSegmentLength[i]);
				
				if( normalizedIndex >= ControlNodeCount - 1 )
					return new SegmentParameter( MaxNodeIndex( ), 1.0 );
				
				return new SegmentParameter( normalizedIndex, normalizedParam );
			}
		}
	
		return new SegmentParameter( MaxNodeIndex( ), 1 );
	}
	
	private SplineNode GetNode( int idxNode, int idxOffset )
	{
		idxNode += idxOffset;
		
		if( AutoClose )
			return splineNodesInternal[ (idxNode % splineNodesInternal.Count + splineNodesInternal.Count) % splineNodesInternal.Count ];
		else
			return splineNodesInternal[ Mathf.Clamp( idxNode, 0, splineNodesInternal.Count-1 ) ];
	}
	
	private void ReparameterizeCurve( )
	{
		if( lengthData == null )
			lengthData = new LengthData( );
		
		lengthData.Calculate( this );
	}
	
	private int MaxNodeIndex( )
	{
		return ControlNodeCount - NodesPerSegment - 1;
	}
	
	private int GetRelevantNodeCount( int nodeCount )
	{
		int relevantNodeCount = nodeCount;
		
		if( IsBezier )
		{
			if( nodeCount < 7 )
				relevantNodeCount -= (nodeCount) % 4;
			else
				relevantNodeCount -= (nodeCount - 4) % 3;
		}
		
		return relevantNodeCount;
	}
	
	private bool EnoughNodes( int nodeCount )
	{
		if( IsBezier )
			return !(nodeCount < 4 );
		else
			return !(nodeCount < 2);
	}
	
	private struct SegmentParameter
	{
		public double normalizedParam;
		public int normalizedIndex;
		
		public SegmentParameter( int index, double param )
		{
			normalizedParam = param;
			normalizedIndex = index;
		}
	}
	
	/// <summary>
	/// Specifies how tangents of control points should be calculated. Note that this will only affect Hermite-Splines.
	/// </summary>
	public enum TangentMode 
	{ 
		UseNormalizedTangents, 	///< Use the normalized vector that connects the two adjacent control nodes as tangent (see UseTangents).
		UseTangents, 			///< Use the vector that connects the two adjacent control nodes as tangent.
		UseNodeForwardVector 	///< Use the forward vector which depends on the control node's rotation.
	}
	
	/// <summary>
	/// Specifies how normals are defined. This is very important when the spline's rotations mode is set to tangent.
	/// </summary>
	public enum NormalMode 
	{
		UseGlobalSplineNormal,	///< Use the globally defined normal of the spline. (Spline.normal)
		UseNodeNormal, 			///< Use the nodes normal.
		UseNodeUpVector, 		///< Use the nodes local up-vector given by its transform-component.
	}
	
	/// <summary>
	/// Specifies how rotations will be interpolated over the spline.
	/// </summary>
	public enum RotationMode 
	{ 
		None, 					///< No rotation (Quaternion.identity).
		Node, 					///< Interpolate the control nodes' orientation.
		Tangent 				///< Use the tangent to calculate the rotation on the spline.
	}
	
	/// <summary>
	/// Specifies the type of spline interpolation that will be used.
	/// </summary>
	public enum InterpolationMode 
	{
		Hermite, 				///< Hermite Spline
		Bezier, 				///< Bézier Spline
		BSpline, 				///< B-Spline
		Linear,					///< Linear Interpolation
		CustomMatrix			///< Use a custom coefficient matrix for interpolation (if CustomMatrix hasn't been assigned to, the hermite matrix will be used) 
	}
	
	/// <summary>
	/// Specifies when to update and recalculate a spline.
	/// </summary>
	public enum UpdateMode
	{
		DontUpdate, 	///< Keeps the spline static. It will only be updated when the component becomes enabled (OnEnable( )).
		EveryFrame, 	///< Updates the spline every frame.
		EveryXFrames, 	///< Updates the spline every x frames.
		EveryXSeconds, 	///< Updates the spline every x seconds.
//		WhenNodeMoved 	///< Updates the spline whenever a spline node has been moved. (Will reset the nodes' transforms' hasChanged-Property to false in LateUpdate)
	}
}

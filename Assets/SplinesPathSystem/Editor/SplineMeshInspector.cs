using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SplineMesh))]
public class SplineMeshInspector : InstantInspector
{
	private SerializedProperty splineProp;
	
	private SerializedProperty updateModeProp;
	private SerializedProperty deltaFramesProp;
	private SerializedProperty deltaTimeProp;
	
	private SerializedProperty startMeshProp;
	private SerializedProperty baseMeshProp;
	private SerializedProperty endMeshProp;
	
	private SerializedProperty xyScaleProp;
	private SerializedProperty uvScaleProp;
	private SerializedProperty uvModeProp;
	private SerializedProperty highAccuracyProp;
	
	private SerializedProperty segmentCountProp;
	private SerializedProperty splineSegmentProp;
	private SerializedProperty segmentStartProp;
	private SerializedProperty segmentEndProp;
	
	private SerializedProperty splitModeProp;
	
	private GUIStyle buttonGUIStyleLeft;
	private GUIStyle buttonGUIStyleRight;
	
	private const string noSplineHint = "Please select the spline that shall be used for mesh generation.";
	private const string noMeshHint = "Please specify a base mesh. A middle mesh is required.";
	
	private const string highAccuracyHint = "If \"High Accuracy\" is enabled the mesh generation will be slower and shouldn't be performed every frame! " +
		"\nConsider exporting the generated mesh or changing the \"Update Mode\" to \"DontUpdate\", \"EveryXSeconds\" or \"EveryXFrames\"!";
	
	public void OnEnable( )
	{
		splineProp 		= serializedObject.FindProperty( "spline" );
		startMeshProp 	= serializedObject.FindProperty( "startBaseMesh" );
		baseMeshProp 	= serializedObject.FindProperty( "baseMesh" );
		endMeshProp 	= serializedObject.FindProperty( "endBaseMesh" );
		uvScaleProp 	= serializedObject.FindProperty( "uvScale" );
		xyScaleProp 	= serializedObject.FindProperty( "xyScale" );
		uvModeProp 		= serializedObject.FindProperty( "uvMode" );
		splitModeProp 	= serializedObject.FindProperty( "splitMode" );
		
		highAccuracyProp = serializedObject.FindProperty( "highAccuracy" );
		
		updateModeProp = serializedObject.FindProperty( "updateMode" );
		deltaFramesProp = serializedObject.FindProperty( "deltaFrames" );
		deltaTimeProp = serializedObject.FindProperty( "deltaTime" );
		
		segmentCountProp	= serializedObject.FindProperty( "segmentCount" );
		splineSegmentProp 	= serializedObject.FindProperty( "splineSegment" );
		segmentStartProp 	= serializedObject.FindProperty( "segmentStart" );
		segmentEndProp	 	= serializedObject.FindProperty( "segmentEnd" );
	}
		
	public override void OnInspectorGUIInner( )
	{
		DrawSplineField( );
		
		DrawUpdateOptions( );
		
		DrawBaseMeshes( );
		
		DrawMeshOptions( );
		
		DrawSegmentOptions( );
		
		DrawMeshTools( );
	}
	
	private void DrawUpdateOptions( )
	{
		EditorGUILayout.PrefixLabel( "Update Options", EditorStyles.label, EditorStyles.boldLabel );
		
		++EditorGUI.indentLevel;
		EditorGUILayout.PropertyField( updateModeProp, new GUIContent( "Update Mode" ), true );
		
		switch( (Spline.UpdateMode) updateModeProp.enumValueIndex )
		{
		case Spline.UpdateMode.EveryXFrames:
			EditorGUILayout.PropertyField( deltaFramesProp, new GUIContent( "Delta Frames" ) );
			deltaFramesProp.intValue = Mathf.Max( deltaFramesProp.intValue, 2 );
			break;
		case Spline.UpdateMode.EveryXSeconds:	
			EditorGUILayout.PropertyField( deltaTimeProp, new GUIContent( "Delta Seconds" ) );
			deltaTimeProp.floatValue = Mathf.Max( deltaTimeProp.floatValue, 0.01f );
			break;
		}
		
		--EditorGUI.indentLevel;
	}
	
	private void DrawSegmentOptions( )
	{
		if( targets.Length != 1 )
		{
			EditorGUILayout.HelpBox( "Multi-editing is not supported for some options. Please select only one SplineMesh!", MessageType.Warning );
			return;
		}
		
		EditorGUILayout.PrefixLabel( "Segmentation Options", EditorStyles.label, EditorStyles.boldLabel );
		
		++EditorGUI.indentLevel;
		EditorGUILayout.IntSlider( segmentCountProp, 2, MaxSegmentCount( ), new GUIContent( "Segment Count" ) ); 
		
		SmallSpace( );
		
		EditorGUILayout.PropertyField( splitModeProp, new GUIContent( "Split Mode" ) );
		
		++EditorGUI.indentLevel;
		switch( (SplineMesh.SplitMode) splitModeProp.enumValueIndex )
		{
		case SplineMesh.SplitMode.BySplineParameter:
			
			EditorGUI.BeginChangeCheck( );
			float start = segmentStartProp.floatValue;
			float end = segmentEndProp.floatValue;
			EditorGUILayout.MinMaxSlider( new GUIContent( "Spline Parameter" ), ref start, ref end, 0, 1 ); 
			segmentStartProp.floatValue = start;
			segmentEndProp.floatValue = end;
			EditorGUI.EndChangeCheck( );
		
			DrawHorizontalFields( segmentStartProp, segmentEndProp, "Values", "S", "E" );
			
			break;
			
		case SplineMesh.SplitMode.BySplineSegment:
			Spline targetSpline = splineProp.objectReferenceValue as Spline;
			int maxSegments = (targetSpline != null ) ? targetSpline.SegmentCount-1 : 1;
			
			EditorGUILayout.IntSlider( splineSegmentProp, 0, maxSegments, new GUIContent( "Segment Index" ) ); 
			
			break;
			
		case SplineMesh.SplitMode.DontSplit:
			break;
		}
		--EditorGUI.indentLevel;
		
		SmallSpace( );
		SmallSpace( );
		
		--EditorGUI.indentLevel;
	}
	
	private void DrawSplineField( )
	{
		//Spline Property
		EditorGUILayout.PrefixLabel( "Spline", EditorStyles.label, EditorStyles.boldLabel );
		
		++EditorGUI.indentLevel;
		EditorGUILayout.PropertyField( splineProp, GUIContent.none );
		--EditorGUI.indentLevel;
		
		if( splineProp.objectReferenceValue == null )
			EditorGUILayout.HelpBox( noSplineHint, MessageType.Warning, true );
		
		SmallSpace( );
	}
	
	private void DrawBaseMeshes( )
	{
		//Mesh property
		EditorGUILayout.PrefixLabel( "Base Meshes", EditorStyles.label, EditorStyles.boldLabel );
		
		++EditorGUI.indentLevel;
		EditorGUILayout.PropertyField( startMeshProp, new GUIContent( "Start Mesh" ) );
		EditorGUILayout.PropertyField( baseMeshProp, new GUIContent( "Middle Mesh" ) );
		EditorGUILayout.PropertyField( endMeshProp, new GUIContent( "End Mesh" ) );
		--EditorGUI.indentLevel;
		
		if( baseMeshProp.objectReferenceValue == null )
			EditorGUILayout.HelpBox( noMeshHint, MessageType.Warning, false );
		
		SmallSpace( );
	}
	
	private void DrawMeshOptions( )
	{
		EditorGUILayout.PrefixLabel( "Mesh Options", EditorStyles.label, EditorStyles.boldLabel );
		
		++EditorGUI.indentLevel;
		
		EditorGUILayout.PropertyField( uvModeProp, new GUIContent( "UV-Mode" ) );
		
		DrawVectorFields( uvScaleProp, "UV Scale", "U", "V" );
		DrawVectorFields( xyScaleProp, "Mesh Scale", "X", "Y" );
		
		SmallSpace( );
		
		EditorGUILayout.PropertyField( highAccuracyProp,  new GUIContent( "High Accuracy" ) );
		
		if( highAccuracyProp.boolValue && ((SplineMesh.UpdateMode)updateModeProp.enumValueIndex) == SplineMesh.UpdateMode.WhenSplineChanged )
			EditorGUILayout.HelpBox( highAccuracyHint, MessageType.Info, false );
		
		--EditorGUI.indentLevel;
		
		SmallSpace( );
	}
	
	private void DrawVectorFields( SerializedProperty property, string label, string xLabel, string yLabel )
	{
		EditorGUILayout.LabelField( label );
		++EditorGUI.indentLevel;
		DrawHorizontalFields( property.FindPropertyRelative( "x" ), property.FindPropertyRelative( "y" ), label, xLabel, yLabel );
		--EditorGUI.indentLevel;
	}
	
	private void DrawHorizontalFields( SerializedProperty property1, SerializedProperty property2, string label, string label1, string label2 )
	{
		EditorGUILayout.BeginHorizontal( );
		EditorGUIUtility.LookLikeControls(60, 40);
		EditorGUILayout.PropertyField( property1, new GUIContent( label1 ) );
		
		PushIndentLevel( );
		EditorGUI.indentLevel = 1;
		
		EditorGUIUtility.LookLikeControls(25, 40);
		EditorGUILayout.PropertyField( property2, new GUIContent( label2 ) );
		
		PopIndentLevel( );
		DefaultWidths( );
		EditorGUILayout.EndHorizontal( );
	}
	
	private void DrawMeshTools( )
	{
		EditorGUILayout.BeginHorizontal( );
		
		GUILayout.Space( 15 );
		
		SplineMesh sMesh = target as SplineMesh;
		
		if( GUILayout.Button( "Export Mesh", GetLeftButtonGUIStyle( ), GUILayout.Height( 23f ) ) )
		{
			string filePath = EditorUtility.SaveFilePanelInProject( "Export Spline Mesh", "Spline Mesh (" + target.name + ")", "asset", "" );
			
			if( filePath.Trim( ) != "" )
			{
				Mesh mesh = new Mesh( );
				
				mesh.vertices = sMesh.BentMesh.vertices;
				mesh.normals = sMesh.BentMesh.normals;
				mesh.tangents = sMesh.BentMesh.tangents;
				mesh.uv = sMesh.BentMesh.uv;
				
				mesh.triangles = sMesh.BentMesh.triangles;
				
				AssetDatabase.CreateAsset( mesh, filePath );
				AssetDatabase.SaveAssets( );
				
				mesh.hideFlags = HideFlags.HideAndDontSave;
			}
		}
		
		if( GUILayout.Button( "View Mesh", GetRightButtonGUIStyle( ), GUILayout.Height( 23f ) ) )
			Selection.activeObject = sMesh.BentMesh;
		
		EditorGUILayout.EndHorizontal( );
	}
	
	public override void OnInspectorChanged( )
	{
		foreach( Object targetObject in serializedObject.targetObjects	 )
			(targetObject as SplineMesh).UpdateMesh( );
	}
	
	private int MaxSegmentCount( )
	{
		int unusedVertices = 65000;
		
		SplineMesh splineMesh = target as SplineMesh;
		
		if( splineMesh.spline == null )
			return unusedVertices;
		else if( splineMesh.baseMesh == null )
			return unusedVertices;
		
		if( splineMesh.startBaseMesh != null && splineMesh.splineSegment <= 0 )
			unusedVertices -= splineMesh.startBaseMesh.vertexCount;
		
		if( splineMesh.endBaseMesh != null && (splineMesh.splineSegment == -1 || splineMesh.splineSegment == splineMesh.spline.SegmentCount-1) )
			unusedVertices -= splineMesh.endBaseMesh.vertexCount;
		
		return (unusedVertices - (unusedVertices % splineMesh.baseMesh.vertexCount)) / splineMesh.baseMesh.vertexCount;
	}
	
	private int MaxSplineSegment( )
	{
		Spline spline = splineProp.objectReferenceValue as Spline;
		
		if( spline != null )
			return spline.SegmentCount-1;
		else
			return 0;
	}
	
	private GUIStyle GetLeftButtonGUIStyle( )
	{
		if( buttonGUIStyleLeft == null )
		{
			buttonGUIStyleLeft = new GUIStyle( EditorStyles.miniButtonLeft );
			buttonGUIStyleLeft.alignment = TextAnchor.MiddleCenter;
			buttonGUIStyleLeft.wordWrap = true;
			buttonGUIStyleLeft.border = new RectOffset( 3, 3, 3, 3 );
			buttonGUIStyleLeft.contentOffset = - Vector2.up * 2f;
			buttonGUIStyleLeft.fontSize = 12;
		}
		
		return buttonGUIStyleLeft;
	}
	
	private GUIStyle GetRightButtonGUIStyle( )
	{
		if( buttonGUIStyleRight == null )
		{
			buttonGUIStyleRight = new GUIStyle( EditorStyles.miniButtonRight );
			buttonGUIStyleRight.alignment = TextAnchor.MiddleCenter;
			buttonGUIStyleRight.wordWrap = true;
			buttonGUIStyleRight.border = new RectOffset( 3, 3, 3, 3 );
			buttonGUIStyleRight.contentOffset = - Vector2.up * 2f;
			buttonGUIStyleRight.fontSize = 12;
		}
		
		return buttonGUIStyleRight;
	}
}

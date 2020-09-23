using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

public partial class SplineEditor : InstantInspector
{
	private SerializedProperty updateModeProp;
	private SerializedProperty interpolationModeProp;
	private SerializedProperty rotationModeProp;
	private SerializedProperty tangentModeProp;
	private SerializedProperty normalModeProp;
	private SerializedProperty accuracyProp;
	private SerializedProperty upVectorProp;
	private SerializedProperty autoCloseProp;
	private SerializedProperty tensionProp;
	private SerializedProperty perNodeTensionProp;
	
	private SerializedProperty deltaFramesProp;
	private SerializedProperty deltaTimeProp;
	
	private CustomArrayDrawer<SplineNode> customArrayDrawer;
	
	private static readonly string performanceInfo = 
		"Performance Hint: Accuracy values above 15 are only reasonable if the segment length betweeen two spline nodes exceeds 10^4 game units, " +
		"or if you need high accuracy in a small scale of less than 10^(-4) game units.";
	
	private static readonly string editingInfo = 
		"In order to insert spline nodes at particular positions on the curve, simply right-click " +
		"somewhere near the spline while pressing the " + (Application.platform == RuntimePlatform.OSXEditor ? "Command" : "Control") + " key.";
	
	private static readonly string multiEditingWarning = 
		"Multi-object editing is not supported for the node array. \nPlease select only one spline!";
	
	private static readonly string bezierWarning = 
		"Bezier Splines must contain a multiple of three plus one node! Only the first {0} nodes will be used as control nodes!";
	
	public void OnEnable( )
	{
		interpolationModeProp = serializedObject.FindProperty( "interpolationMode" );
		rotationModeProp = serializedObject.FindProperty( "rotationMode" );
		tangentModeProp = serializedObject.FindProperty( "tangentMode" );
		accuracyProp = serializedObject.FindProperty( "interpolationAccuracy" );
		tensionProp = serializedObject.FindProperty( "tension" );
		upVectorProp = serializedObject.FindProperty( "normal" );
		autoCloseProp = serializedObject.FindProperty( "autoClose" );
		
		perNodeTensionProp = serializedObject.FindProperty( "perNodeTension" );
		normalModeProp = serializedObject.FindProperty( "normalMode" );
		
		updateModeProp = serializedObject.FindProperty( "updateMode" );
		deltaFramesProp = serializedObject.FindProperty( "deltaFrames" );
		deltaTimeProp = serializedObject.FindProperty( "deltaTime" );
		
		customArrayDrawer = new CustomArrayDrawer<SplineNode>( this, OnInspectorChanged, target as Spline, (target as Spline).splineNodesArray, "Spline Nodes" ); 
	}
	
	public override void OnInspectorGUIInner( )
	{
		DrawSplineSettings( );
		DrawSplineNodeArray( target as Spline );

        DrawSplineNodeEditor();

    }

    bool m_isFoldoutSplineSettings = true;
	private void DrawSplineSettings( )
	{
		//EditorGUILayout.PrefixLabel( "Spline Settings", EditorStyles.label, EditorStyles.boldLabel );
        m_isFoldoutSplineSettings = EditorGUILayout.Foldout(m_isFoldoutSplineSettings, "Spline Settings");
        if (!m_isFoldoutSplineSettings)
            return;
        ++EditorGUI.indentLevel;
		
		EditorGUILayout.PropertyField( interpolationModeProp, new GUIContent( "Spline Type" ) );
		EditorGUILayout.PropertyField( rotationModeProp, new GUIContent( "Rotation Mode" ) );
		
		if( (Spline.InterpolationMode) interpolationModeProp.enumValueIndex == Spline.InterpolationMode.Hermite )
		{
			EditorGUILayout.PrefixLabel( new GUIContent( "Hermite Settings" ), EditorStyles.label, EditorStyles.boldLabel );
			
			++EditorGUI.indentLevel;
			
			EditorGUILayout.PropertyField( tangentModeProp, new GUIContent( "Tangent Mode" ) );
			
			EditorGUILayout.PropertyField( perNodeTensionProp, new GUIContent( "Tension Per Node" ) );
			EditorGUILayout.PropertyField( tensionProp, new GUIContent( "Curve Tension" ) );
			
			--EditorGUI.indentLevel;
			
			SmallSpace( );
		}
		
		if( (Spline.RotationMode) rotationModeProp.enumValueIndex == Spline.RotationMode.Tangent ) 
		{
			EditorGUILayout.PrefixLabel( new GUIContent( "Rotation Options" ), EditorStyles.label, EditorStyles.boldLabel );
			GUILayout.Space(-5);
			EditorGUILayout.PrefixLabel( new GUIContent( "(Tangent-Rotation Mode)" ), EditorStyles.miniLabel, EditorStyles.miniLabel );
			
			++EditorGUI.indentLevel;
			
			EditorGUILayout.PropertyField( normalModeProp, new GUIContent( "Normal Mode" ) );
			EditorGUILayout.PropertyField( upVectorProp, new GUIContent( "Up-Vector (Normal)" ), true );
			
			--EditorGUI.indentLevel;
			
			SmallSpace( );
		}
		
		EditorGUILayout.IntSlider( accuracyProp, 1, 30, new GUIContent( "Calc. Accuracy" ) );
		
		if( accuracyProp.intValue > 15 )
			EditorGUILayout.HelpBox( performanceInfo, MessageType.Info );
		
		if( (Spline.InterpolationMode) interpolationModeProp.enumValueIndex != Spline.InterpolationMode.Bezier )
			EditorGUILayout.PropertyField( autoCloseProp, new GUIContent( "Auto Close" ), true );
		
		
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
		
		--EditorGUI.indentLevel;
		
		SmallSpace();
	}
	
	private void DrawSplineNodeArray( Spline currentSpline )
	{
		if( targets.Length > 1 )
		{
			EditorGUILayout.Space( );
			EditorGUILayout.HelpBox( multiEditingWarning, MessageType.Warning );
			EditorGUILayout.Space( );
			
			return;
		}
		
		customArrayDrawer.DrawArray( );
		
		if( currentSpline.interpolationMode == Spline.InterpolationMode.Bezier )
		{
			int nodeCount = currentSpline.splineNodesArray.Count;
			int unUsedNodes = (nodeCount - 1) % 3;
			
			if( currentSpline.splineNodesArray.Count > 3 )
				if( unUsedNodes != 0 )
					EditorGUILayout.HelpBox( bezierWarning.Replace( "{0}", (nodeCount-unUsedNodes).ToString( ) ), MessageType.Warning );
		}
		
		EditorGUILayout.HelpBox( editingInfo, MessageType.Info );
	}

    private void DrawSplineNodeEditor()
    {
        if(GUILayout.Button("保存为二进制文件"))
        {
            var objs = Selection.gameObjects;
            if(null != objs && objs.Length > 1)
            {
                foreach(var obj in objs)
                {
                    var spline = obj.GetComponent<Spline>();
                    if(null != spline)
                    {
                        SplinePrefab2Bin.SavePathPrefab(spline);
                    }
                    SplinePrefab2Bin.SaveAllPrefabs2Bin();
                }
            }
            else
            {
                SplinePrefab2Bin.SavePathPrefab(target as Spline);
                SplinePrefab2Bin.SaveAllPrefabs2Bin();
            }
            AssetDatabase.Refresh();
        }
    }

    public override void OnInspectorChanged( )
	{
		foreach( Object targetObject in serializedObject.targetObjects )
			ApplyChangesToTarget( targetObject );
		
		SceneView.RepaintAll( );
	}
	
	public void ApplyChangesToTarget( Object targetObject )
	{
		Spline spline = targetObject as Spline;
			
		spline.UpdateSpline( );
		
		SplineMesh splineMesh = spline.GetComponent<SplineMesh>( );
		
		if( splineMesh != null )
			splineMesh.UpdateMesh( );
	}
}

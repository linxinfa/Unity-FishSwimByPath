using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SplineAutoAlign))]
public class SplineAutoAlignInspector : Editor 
{
	public override void OnInspectorGUI( )
	{
		DrawDefaultInspector( );
		
		EditorGUILayout.Space( );
		EditorGUILayout.Space( );
		
		EditorGUILayout.BeginHorizontal( );
			EditorGUILayout.Space( );
			if( GUILayout.Button( "   Auto Align Nodes" ) )
				((SplineAutoAlign) target).AutoAlign( );
			EditorGUILayout.Space( );
		EditorGUILayout.EndHorizontal( );
		
		EditorGUILayout.Space( );
		EditorGUILayout.Space( );
	}
}

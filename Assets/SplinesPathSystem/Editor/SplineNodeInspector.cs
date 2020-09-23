using System;
using System.Collections;

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SplineNode))]
public class SplineNodeInspector : InstantInspector
{
	private SerializedProperty customValueProp;
	private SerializedProperty tensionProp;
	private SerializedProperty normalProp;
	
	private GUIStyle buttonGUIStyle;
	
	private SplineNode targetNode;
	
	private static Spline selectedSpline = null;
	private static LengthMode lMode = LengthMode.GameUnits;
	
	private readonly string notUsedWarning = 
		"This SplineNode isn't used by any spline in the scene. Attach this node to a spline by dragging it onto a spline's Inspector window!";
	
	public void OnEnable( )
	{
		customValueProp = serializedObject.FindProperty( "customValue" );
		tensionProp = serializedObject.FindProperty( "tension" );
		normalProp = serializedObject.FindProperty( "normal" );
	}
		
	public override void OnInspectorGUIInner( )
	{
		targetNode = target as SplineNode;
		
		List<Spline> splinesToRemove = new List<Spline>( );
			
		foreach( var key in targetNode.Parameters.Keys )
		{
			if( !key )
				splinesToRemove.Add( key );
			else if( !key.splineNodesArray.Contains( targetNode ) )
				splinesToRemove.Add( key );
		}
		
		foreach( var key in splinesToRemove )
			targetNode.Parameters.Remove( key );
		
		DrawInspectorOptions( );
		
		DrawSplineSettings( );
		
		DrawCustomSettings( );
		
		DrawButtons( );
	}
	
	private void DrawInspectorOptions( )
	{
		EditorGUILayout.PrefixLabel( "Inspector Options", EditorStyles.label, EditorStyles.boldLabel );
		
		++EditorGUI.indentLevel;
		
		lMode = (LengthMode) EditorGUILayout.EnumPopup( "Length Mode", lMode );
		SmallSpace( );
		
		--EditorGUI.indentLevel;
	}
	
	private void DrawSplineSettings( )
	{
		EditorGUILayout.PrefixLabel( "Spline Data", EditorStyles.label, EditorStyles.boldLabel );
		
		++EditorGUI.indentLevel;
		
		if( targetNode.Parameters.Count <= 0 )
		{
			EditorGUILayout.HelpBox( notUsedWarning, MessageType.Info );
		}
		else
		{
			List<Spline> splineKeys = new List<Spline>( targetNode.Parameters.Keys );
			List<string> splineNames = new List<string>( );
			
			foreach( Spline spline in splineKeys )
			{
				if( !splineNames.Contains( spline.name ) )
					splineNames.Add( spline.name );
				else
				{
					string newName = spline.name;
					
					while( splineNames.Contains( newName ) )
						newName += "*";
					
					splineNames.Add( newName );
				}
			}
			
			int index = splineKeys.IndexOf( selectedSpline );
			
			if( index < 0 )
				index = 0;
			
			index = EditorGUILayout.Popup( "Spline", index, splineNames.ToArray( ) );
			
			selectedSpline = splineKeys[index];
			
			float lengthFactor = (lMode != LengthMode.GameUnits) ? 1 : selectedSpline.Length;
			
			float position = (float)targetNode.Parameters[selectedSpline].position * lengthFactor;
			float length = (float)targetNode.Parameters[selectedSpline].length * lengthFactor;
			SmallSpace( );
			
			int nodeIndex = Array.IndexOf( selectedSpline.SplineNodes, targetNode );
			
			EditorGUILayout.TextField( "Index in Spline", nodeIndex.ToString( ) );
			SmallSpace( );
			
			EditorGUILayout.TextField( "Spline Parameter", position.ToString( ) );
			GUILayout.Space(-5);
			EditorGUIUtility.LookLikeControls( 200 );
			EditorGUILayout.PrefixLabel( new GUIContent( "(Distance From Start Node)" ), EditorStyles.miniLabel, EditorStyles.miniLabel );
			EditorGUIUtility.LookLikeControls( );
			
			EditorGUILayout.TextField( "Length Parameter", length.ToString( ) );
			GUILayout.Space(-5);
			EditorGUIUtility.LookLikeControls( 200 );
			EditorGUILayout.PrefixLabel( new GUIContent( "(Distance To Next Node)" ), EditorStyles.miniLabel, EditorStyles.miniLabel );
			EditorGUIUtility.LookLikeControls( );
		}
		
		--EditorGUI.indentLevel;
	}
	
	private void DrawCustomSettings( )
	{
		EditorGUILayout.PrefixLabel( "Custom Settings", EditorStyles.label, EditorStyles.boldLabel );
		
		++EditorGUI.indentLevel;
		
		EditorGUILayout.PropertyField( tensionProp, new GUIContent( "Curve Tension" ) );
		EditorGUILayout.PropertyField( normalProp, new GUIContent( "Curve Normal" ) );
		EditorGUILayout.Space( );
		
		EditorGUILayout.PropertyField( customValueProp, new GUIContent( "Custom Data" ) );
		SmallSpace( );
		
		--EditorGUI.indentLevel;
	}
	
	private void DrawButtons( )
	{
		if( targetNode.Parameters.Count <= 0 )
			return;
		
		SplineNode[] splineNodes = selectedSpline.SplineNodes;
		
		int nodeIndex = Array.IndexOf( selectedSpline.SplineNodes, targetNode );
		
		EditorGUI.BeginDisabledGroup( selectedSpline == null );
		
		EditorGUILayout.BeginHorizontal( );
		GUILayout.Space( 15 );
		
		if( GUILayout.Button( "Previous Node", GetButtonGUIStyleLeft( ), GUILayout.Height( 21f ) ) )
			Selection.activeGameObject = splineNodes[ (nodeIndex!=0 ? nodeIndex : splineNodes.Length) - 1].gameObject; 
		
		if( GUILayout.Button( "  Next Node	", GetButtonGUIStyleRight( ), GUILayout.Height( 21f ) ) )
			Selection.activeGameObject = splineNodes[(nodeIndex+1)%splineNodes.Length].gameObject; 
		
		EditorGUILayout.EndHorizontal( );
		
		EditorGUI.EndDisabledGroup( );
	}
	
	public void OnSceneGUI( )
	{
		if( targetNode == null )
			return;
		
		Handles.color = new Color( .3f, 1f, .20f, 1 );
		Handles.ArrowCap( 0, targetNode.Position, Quaternion.LookRotation( targetNode.TransformedNormal ), HandleUtility.GetHandleSize( targetNode.Position ) * 0.5f );
		
				Handles.color = new Color( .2f, 0.4f, 1f, 1 );
		Handles.ArrowCap( 0, targetNode.Position, Quaternion.LookRotation( targetNode.transform.forward ), HandleUtility.GetHandleSize( targetNode.Position ) * 0.5f );
		
		Handles.color = new Color( 1f, 0.5f, 0f, .75f );
		Handles.SphereCap( 0, targetNode.Position, targetNode.Rotation, HandleUtility.GetHandleSize( targetNode.Position ) * 0.175f );
	}
	
	private GUIStyle GetButtonGUIStyleLeft( )
	{
		GUIStyle buttonGUIStyle = new GUIStyle( EditorStyles.miniButtonLeft );
		
		buttonGUIStyle.alignment = TextAnchor.MiddleCenter;
		buttonGUIStyle.wordWrap = true;
		buttonGUIStyle.border = new RectOffset( 3, 3, 3, 3 );
		buttonGUIStyle.contentOffset = - Vector2.up * 2f;
		buttonGUIStyle.fontSize = 12;
		
		return buttonGUIStyle;
	}
	
	private GUIStyle GetButtonGUIStyleRight( )
	{
		GUIStyle buttonGUIStyle = new GUIStyle( EditorStyles.miniButtonRight );
		
		buttonGUIStyle.alignment = TextAnchor.MiddleCenter;
		buttonGUIStyle.wordWrap = true;
		buttonGUIStyle.border = new RectOffset( 3, 3, 3, 3 );
		buttonGUIStyle.contentOffset = - Vector2.up * 2f;
		buttonGUIStyle.fontSize = 12;
		
		return buttonGUIStyle;
	}
	
	private enum LengthMode
	{
		Normalized,
		GameUnits
	}
}

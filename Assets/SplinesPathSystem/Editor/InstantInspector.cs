using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

public abstract class InstantInspector : Editor
{
	private float labelWidth = 150;
	private float fieldWidth = -1;
	
	private Stack<int> indentStack = new Stack<int>( );
	
	public override void OnInspectorGUI( )
	{
		EditorGUI.indentLevel++;
		
		SmallSpace( );
		
		serializedObject.Update( );
		
		OnInspectorGUIInner( );
		
		if( serializedObject.ApplyModifiedProperties( ) )
			OnInspectorChanged( );
		
		SmallSpace( );
		
		EditorGUI.indentLevel--;
	}
	
	public void RepaintScene( )
	{
		if( SceneView.lastActiveSceneView != null )
			SceneView.lastActiveSceneView.Repaint( );
	}
	
	public void DefaultWidths( )
	{
		if( fieldWidth < 0 )
			EditorGUIUtility.LookLikeControls( labelWidth );
		else
			EditorGUIUtility.LookLikeControls( labelWidth, fieldWidth );
	}
	
	public void SetDefaultLook( float labelWidth )
	{
		this.labelWidth = labelWidth;
	}
	
	public void SetDefaultLook( float labelWidth, float fieldWidth )
	{
		this.labelWidth = labelWidth;
		this.fieldWidth = fieldWidth;
	}
	
	public void PushIndentLevel( )
	{
		indentStack.Push( EditorGUI.indentLevel );
	}
	
	public void PopIndentLevel( )
	{
		EditorGUI.indentLevel = indentStack.Pop( );
	}
	
	public abstract void OnInspectorGUIInner( );
	
	public virtual void OnInspectorChanged( )
	{
		
	}
	
	public static void SmallSpace( )
	{
		GUILayout.Space( 5f );
	}
}

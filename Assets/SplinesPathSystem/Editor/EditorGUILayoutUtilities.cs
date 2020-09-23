using UnityEngine;
using UnityEditor;
using System.Collections;

internal class CustomPropertyDrawerUtilites
{
	public static void BoldPrefixLabel( Rect position, string label )
	{
		EditorGUIUtility.LookLikeInspector( );
		position.x-=2;
		EditorGUI.LabelField( position, new GUIContent( label ), EditorStyles.boldLabel );
		position.x+=2;
		EditorGUIUtility.LookLikeControls( );
	}
	
	public static Rect NormalizeRect( Rect position ) 
	{
		position.height = 16;
		
		return position;
	}
	
	public static Rect IndentRect( Rect position )
	{
		position.x += (EditorGUI.indentLevel-1) * 5f;
		position.width -= (EditorGUI.indentLevel) * 5f; 
		
		return position;
	}
}

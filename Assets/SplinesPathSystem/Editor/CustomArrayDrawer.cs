using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class CustomArrayDrawer<T> where T : Behaviour
{
	private MonoBehaviour undoTarget;
	
	private Editor inspector;
	
	private List<T> editedList;
	
	private string headingLabel;
	private string typeName;
	private string typeNamePlural;
	
	private GUIStyle boldFoldoutStlye;
	
	private bool foldedOut = true;
	private bool isDragging = false;
	
	private Action callback;
	
	private float dragTargetWidth = 0f;
	private const float insertBoxesWidth = 66f;
	
	public CustomArrayDrawer( Editor inspector, Action callback, MonoBehaviour undoTarget, List<T> editedList, string headingLabel )
	{
		this.undoTarget = undoTarget;
		this.editedList = editedList;
		
		this.callback = callback;
		
		this.headingLabel = headingLabel;
		
		this.inspector = inspector;
		
		this.typeName = typeof( T ).Name;
		
		if( typeName.EndsWith( "s" ) )
			typeNamePlural = typeName + "es";
		else if( typeName.EndsWith( "sh" ) )
			typeNamePlural = typeName + "es";
		else if( typeName.EndsWith( "ch" ) )
			typeNamePlural = typeName + "es";
		else if( typeName.EndsWith( "dg" ) )
			typeNamePlural = typeName + "es";
		else if( typeName.EndsWith( "o" ) )
			typeNamePlural = typeName + "es";
		else if( typeName.EndsWith( "y" ) )
			typeNamePlural = typeName.Remove( typeName.Length-1, 1 ) + "ies";
		else
			typeNamePlural = typeName + "s";
	}
	
	public void DrawArray( )
	{	
		//Draw headline
		GUIStyle boldFoldoutStlye = new GUIStyle( EditorStyles.foldout );
		boldFoldoutStlye.fontStyle = FontStyle.Bold;
		
		foldedOut = EditorGUILayout.Foldout( foldedOut, headingLabel, boldFoldoutStlye );
		
		if( !foldedOut )
			return;
		
		if( editedList.Count == 0 )
		{
			//No nodes in the array:
			InsertElements( DropBox( "Drag game objects containing the " + typeName + "-component into this box in order to add them to the array." ), 0 );
		}
		else
		{
			//Draw default array
			EditorGUILayout.BeginHorizontal( );
			GUILayout.Space( 20 );
			EditorGUILayout.HelpBox( "Insert " + typeName + " by dragging them into the Inspector. Further options will be enabled, while you're performing a drag'n'drop operation.", MessageType.None );
				
			EditorGUILayout.EndHorizontal( );
			
			int indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			
			UpdateInsertBoxes( );
			
			for( int i = 0; i < editedList.Count; i++ )
			{
				GUILayout.Space( 1 );
				
				EditorGUILayout.BeginHorizontal( );
				
				GUILayout.Space( 28 + (indentLevel-1) * 15 );
				
				bool removePressed = GUILayout.Button( "Remove", EditorStyles.miniButtonLeft, GUILayout.Width( 48f ), GUILayout.Height( 16f ) );
				
				if( dragTargetWidth > 1f )
				{
					EditorGUILayout.LabelField( "Insert  Before", EditorStyles.objectFieldThumb, GUILayout.MaxWidth( dragTargetWidth ) );
					InsertElements( AcceptDrag( ), i );
				}
				
				T newElement = EditorGUILayout.ObjectField( editedList[i], typeof( T ), true ) as T;
				
				//Change element
				if( newElement != editedList[i] )
				{
					Undo.RegisterUndo( undoTarget, "Change " + typeName + " in " + undoTarget.gameObject.name );
					editedList[i] = newElement;
					
					OnChanged( );
				}
				
				if( dragTargetWidth > 1f )
				{
					EditorGUILayout.LabelField( "  Insert After", EditorStyles.objectFieldThumb, GUILayout.MaxWidth( dragTargetWidth ) );
					InsertElements( AcceptDrag( ), i+1 );
				}
				
				EditorGUILayout.EndHorizontal( );
				
				//Remove element
				if( removePressed )
				{
					Undo.RegisterUndo( undoTarget, "Remove " + typeName + " from " + undoTarget.gameObject.name );
					editedList.RemoveAt( i );
					
					OnChanged( );
				}
			}
			
			EditorGUI.indentLevel = indentLevel;
		}
		
		GUILayout.Space( 4f );
	}
	
	private bool InsertElements( List<T> newElements, int index )
	{
		if( newElements.Count <= 0 )
			return false;
		
		Undo.RegisterUndo( undoTarget, "Add " + typeNamePlural + " to " + undoTarget.gameObject.name );
		
		editedList.InsertRange( index, newElements );
		
		OnChanged( );
		
		return true;
	}
	
	private void OnChanged( )
	{
		if( callback != null )
			callback( );
		
		EditorGUIUtility.ExitGUI( );
	}
	
	private List<T> DropBox( string caption )
	{
		List<T> nodeReferences = null;
		
		//Draw drop box
		EditorGUILayout.BeginHorizontal( );
		
		EditorGUILayout.HelpBox( caption, MessageType.None );
		
		nodeReferences = AcceptDrag( );
		
		EditorGUILayout.EndHorizontal( );
		
		return nodeReferences;
	}
	
	private List<T> AcceptDrag( )
	{	
		EventType eventType = Event.current.type;
		Rect dropBoxRect = GUILayoutUtility.GetLastRect( );
		
		//if no drag n drop operation return empty collection
		if( eventType != EventType.DragUpdated && eventType != EventType.DragPerform )
			return new List<T>( );
		
		if( !dropBoxRect.Contains( Event.current.mousePosition - new Vector2( 0, 3 ) ) )
			return new List<T>( );

		DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
		
		if( eventType != EventType.DragPerform )
			return new List<T>( );
		
		DragAndDrop.AcceptDrag( );
		
		List<T> elements = new List<T>( );
		
		foreach( UnityEngine.Object item in DragAndDrop.objectReferences )
		{
			GameObject gameObject = item as GameObject;
			
			if( gameObject == null )
				continue;
			
			T element = gameObject.GetComponent<T>( );
			
			if( element == null )
			{
				Debug.LogWarning( "The game object \"" + gameObject.name + "\" doesn't have a " + typeName + " component!", gameObject );
				
				continue;
			}
			
			elements.Add( element );
		}
		
		return elements;
	}
	
	private void UpdateInsertBoxes( )
	{
		if( Event.current.type == EventType.DragUpdated )
			isDragging = true;
		
		if( Event.current.type == EventType.DragExited )
			isDragging = false;
		
		if( Event.current.type != EventType.Layout )
			return;
		
		if( isDragging && Mathf.Abs( dragTargetWidth - insertBoxesWidth ) > 0.5f )
		{
			UnityEngine.Object[] dragNDropReferences = DragAndDrop.objectReferences;
			
			foreach( UnityEngine.Object reference in dragNDropReferences )
			{
				GameObject gObject = reference as GameObject;
				
				if( gObject == null )
					return;
				else if( gObject.GetComponent<T>( ) == null )
					return;
			}
			
			dragTargetWidth = Mathf.Clamp( Mathf.Lerp( dragTargetWidth, insertBoxesWidth + 5f, 0.1f ), 0f, insertBoxesWidth );
			inspector.Repaint( );
		}
		else if( !isDragging && dragTargetWidth > 0.5f )
		{
			dragTargetWidth = Mathf.Clamp( Mathf.Lerp( dragTargetWidth, -10f, 0.1f ), 0f, insertBoxesWidth );
			inspector.Repaint( );
		}
	}
}

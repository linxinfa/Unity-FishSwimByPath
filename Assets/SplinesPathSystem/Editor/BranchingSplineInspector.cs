using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BranchingSpline))]
public class BranchingSplineInspector : InstantInspector
{
	private CustomArrayDrawer<Spline> splineArrayDrawer;
	
	private static readonly string editingInfo = "Add all splines that will make up the branching spline to the above array. " +
		"SplineNodes that are used in multiple splines will automatically be used as junctions/branching points.";
	
	public void OnEnable( )
	{
		BranchingSpline bSpline = target as BranchingSpline;
		
		splineArrayDrawer = new CustomArrayDrawer<Spline>( this, OnInspectorChanged, bSpline, bSpline.splines, "Sub-Paths" ); 
	}
	
	public override void OnInspectorGUIInner( )
	{
		BranchingSpline bSpline = target as BranchingSpline;
		
		DrawSplineNodeArray( bSpline );
		
		EditorGUILayout.HelpBox( editingInfo, MessageType.Info );
	}
	
	private void DrawSplineNodeArray( BranchingSpline currentSpline )
	{	
		if( targets.Length > 1 )
			return;
		
		splineArrayDrawer.DrawArray( );
	}
	
	public override void OnInspectorChanged( )
	{
//		SceneView.RepaintAll( );
	}
}

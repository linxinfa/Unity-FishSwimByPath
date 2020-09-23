using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class Spline : MonoBehaviour
{
	void OnDrawGizmos( )
	{
		UpdateSpline( );
		
		if( !HasNodes )
			return;
		
		DrawSplineGizmo( new Color( 0.5f, 0.5f, 0.5f, 0.5f ) );
		
		Plane screen = new Plane( );
		Gizmos.color = new Color( 1f, 1f, 1f, 0.5f );
		
		
		screen.SetNormalAndPosition( Camera.current.transform.forward, Camera.current.transform.position );
		
		foreach( SplineNode node in splineNodesInternal )
			Gizmos.DrawSphere( node.Position, GetSizeMultiplier( node ) * 2 );
	}
	
	void OnDrawGizmosSelected( )
	{
		UpdateSpline( );
		
		if( !HasNodes )
			return;
		
		DrawSplineGizmo( new Color( 1f, 0.5f, 0f, 1f ) );
		
		Gizmos.color = new Color( 1f, 0.5f, 0f, 0.75f );
		
		int nodeIndex = -1;
		
		foreach( SplineNode node in splineNodesInternal )
		{
			++nodeIndex;
			
			if( IsBezier && (nodeIndex % 3) != 0 )
				Gizmos.color = new Color( .8f, 1f, .1f, 0.70f );
			else
				Gizmos.color = new Color( 1f, 0.5f, 0f, 0.75f );
			
			Gizmos.DrawSphere( node.Position, GetSizeMultiplier( node ) * 1.5f );
		}
	}
	
	void DrawSplineGizmo( Color curveColor )
	{	
		switch( interpolationMode )
		{
		case InterpolationMode.BSpline:
		case InterpolationMode.Bezier:
			Gizmos.color = new Color( curveColor.r, curveColor.g, curveColor.b, curveColor.a * 0.25f );
				Gizmos.color = new Color( .8f, 1f, .1f, curveColor.a * 0.25f );
			
			for( int i = 0; i < ControlNodeCount-1; i++ )
			{
				Gizmos.DrawLine( GetNode( i, 0 ).Position, GetNode( i, 1 ).Position );
			
				if( ( i % 3 == 0) && IsBezier )
					++i;
			}
			
			goto default;
			
		case InterpolationMode.Hermite:
		default:
			Gizmos.color = curveColor;
			
			for( int i = 0; i < ControlNodeCount-1; i += NodesPerSegment )
			{
				Vector3 lastPos = GetPositionInternal( new SegmentParameter( i, 0 ) );
				
				for( float f = (IsBezier ? 0.025f : 0.1f); f < 1.0005f; f += (IsBezier ? 0.025f : 0.1f) )
				{
					Vector3 curPos = GetPositionInternal( new SegmentParameter( i, f ) );
					
					Gizmos.DrawLine( lastPos, curPos );
					
					lastPos = curPos;
				}
			}
			
			break;
		}
	}
	
	float GetSizeMultiplier( SplineNode node )
	{
		if( !Camera.current.orthographic )
		{
			Plane screen = new Plane( );
			
			float sizeMultiplier = 0f;
			
			screen.SetNormalAndPosition( Camera.current.transform.forward, Camera.current.transform.position );
			screen.Raycast( new Ray( node.Position, Camera.current.transform.forward ), out sizeMultiplier );
			
			return sizeMultiplier * .0075f;
		}
	
		return Camera.current.orthographicSize * 0.01875f;
	}
}

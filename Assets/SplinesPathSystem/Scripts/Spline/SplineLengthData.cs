using UnityEngine;
using System;

public partial class Spline : MonoBehaviour
{
	//Approximate the length of a spline segment numerically
	private double GetSegmentLengthInternal( int idxFirstPoint, double startValue, double endValue, double step )
	{
		Vector3 currentPos;
		
		double pPosX; double pPosY; double pPosZ;
		double cPosX; double cPosY; double cPosZ;
		
		double length = 0;
		
		currentPos = GetPositionInternal( new SegmentParameter( idxFirstPoint, startValue ) );
		
		pPosX = currentPos.x;
		pPosY = currentPos.y;
		pPosZ = currentPos.z;
		
		for( double f = startValue + step; f < (endValue + step * 0.5); f += step )
		{
			currentPos = GetPositionInternal( new SegmentParameter( idxFirstPoint, f ) );
			
			cPosX = pPosX - currentPos.x;
			cPosY = pPosY - currentPos.y;
			cPosZ = pPosZ - currentPos.z;
			
			length += Math.Sqrt( cPosX*cPosX + cPosY*cPosY + cPosZ*cPosZ );
			
			pPosX = currentPos.x;
			pPosY = currentPos.y;
			pPosZ = currentPos.z;
		}
		
		return length;
	}
	
	private sealed class LengthData
	{
		public double[] subSegmentLength;
		public double[] subSegmentPosition;
		
		public double length;
		
		public void Calculate( Spline spline )
		{
			int subsegmentCount = spline.SegmentCount * spline.interpolationAccuracy;
			double invertedAccuracy = 1.0 / spline.interpolationAccuracy;
			
			subSegmentLength = new double[subsegmentCount];
			subSegmentPosition = new double[subsegmentCount];
		
			length = 0.0;
			
			for( int i = 0; i < subsegmentCount; i++ )
			{
				subSegmentLength[i] = 0.0;
				subSegmentPosition[i] = 0.0;
			}
			
			for( int i = 0; i < spline.SegmentCount; i++ )
			{
				for( int j = 0; j < spline.interpolationAccuracy; j++ )
				{
					int index = i * spline.interpolationAccuracy + j;
					
					subSegmentLength[index] = spline.GetSegmentLengthInternal( i * spline.NodesPerSegment, j*invertedAccuracy, (j+1)*invertedAccuracy, 0.2 * invertedAccuracy );
					
					length += subSegmentLength[index];
				}
			}
			
			for( int i = 0; i < spline.SegmentCount; i++ )
			{
				for( int j = 0; j < spline.interpolationAccuracy; j++ )
				{
					int index = i*spline.interpolationAccuracy+j;
					
					subSegmentLength[index] /= length;
					
					if( index < subSegmentPosition.Length-1 )
						subSegmentPosition[index+1] = subSegmentPosition[index] + subSegmentLength[index];
				}
			}
			
			SetupSplinePositions( spline );
		}
		
		private void SetupSplinePositions( Spline spline )
		{
			foreach( SplineNode node in spline.splineNodesInternal )
				node.Parameters[spline].Reset( );
			
			for( int i = 0; i < subSegmentLength.Length; i++ )
			{
				int nodeIndex = ((i - (i % spline.interpolationAccuracy))/spline.interpolationAccuracy) * spline.NodesPerSegment;
				
				SplineNode node = spline.splineNodesInternal[nodeIndex];
				
				node.Parameters[spline].length += subSegmentLength[i];
			}
			
			for( int i = 0; i < spline.splineNodesInternal.Count-spline.NodesPerSegment; i+=spline.NodesPerSegment )
			{
				NodeParameters nodeParameters = spline.splineNodesInternal[i].Parameters[spline];
				
				spline.splineNodesInternal[i+spline.NodesPerSegment].Parameters[spline].position = nodeParameters.position + nodeParameters.length;
			}
			
			if( spline.IsBezier )
			{	
				for( int i = 0; i < spline.splineNodesInternal.Count-spline.NodesPerSegment; i+=spline.NodesPerSegment )
				{
					
					
					spline.splineNodesInternal[i+1].Parameters[spline].position = spline.splineNodesInternal[i].Parameters[spline].position;
					spline.splineNodesInternal[i+2].Parameters[spline].position = spline.splineNodesInternal[i].Parameters[spline].position;
					spline.splineNodesInternal[i+1].Parameters[spline].length = 0.0;
					spline.splineNodesInternal[i+2].Parameters[spline].length = 0.0;
				}
			}
			
			if( !spline.AutoClose )
				spline.splineNodesInternal[spline.splineNodesInternal.Count-1].Parameters[spline].position = 1.0;
		}
	}
}

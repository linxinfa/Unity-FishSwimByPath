using UnityEngine;

using System;

/**
* @class SplineSegment
*
* @brief This class represents a pair of two control nodes that define a segment on the Spline.
*
* A spline segment is represented by two control nodes. This class stores two references to such nodes and provides
* useful functions that allow you to convert a parameter that represents a point on the segment to a normalized 
* spline parameter that represents the same point on the spline. 
* This class becomes quite useful when handling Bézier curves!
*/ 
/// <summary>
/// This class represents a pair of two control nodes that define a segment on the Spline.
/// </summary>
/// <remarks>
/// A spline segment is represented by two control nodes. This class stores two references to such nodes and provides
/// useful functions that allow you to convert a parameter that represents a point on the segment to a normalized 
/// spline parameter that represents the same point on the spline. 
/// This class becomes quite useful when handling Bézier curves!
/// </remarks>
public class SplineSegment
{
	private readonly Spline parentSpline;
	private readonly SplineNode startNode;
	private readonly SplineNode endNode; 
	
	public Spline ParentSpline { get{ return parentSpline; } }  	///< Returns a reference to the containing spline.
	public SplineNode StartNode { get{ return startNode; } } 		///< Returns a reference to the spline segment's start point.
	public SplineNode EndNode { get{ return endNode; } } 			///< Returns a reference to the spline segment's end point.
	
	public float Length { 
		get { 
			return (startNode.Parameters[parentSpline].Length * parentSpline.Length); 
		} 
	} ///< Returns the actual length of the spline segment.
	
	public float NormalizedLength { 
		get { 
			return startNode.Parameters[parentSpline].Length; 
		} 
	} ///< Returns the normlaized length of the segment in the spline.
	
	/// <summary>
	/// Initializes a new instance of the <see cref="SplineSegment"/> class.
	/// </summary>
	/// <param name='pSpline'>
	/// The spline that contains the segment.
	/// </param>
	/// <param name='sNode'>
	/// The segment's start node.
	/// </param>
	/// <param name='eNode'>
	/// The segment's end node.
	/// </param>
	/// <exception cref='ArgumentNullException'>
	/// Is thrown when pSpline is null /> .
	/// </exception>
	public SplineSegment( Spline pSpline, SplineNode sNode, SplineNode eNode )
	{
		if( pSpline != null )
		{
			parentSpline = pSpline;
			
			startNode = sNode;
			endNode = eNode;
		}
		else
		{
			throw new ArgumentNullException( "Parent Spline must not be null" );
		}
	}
	
	/// <summary>
	/// This method converts a parameter [0..1] representing a point on the segment to a normalized parameter [0..1] representing a point on the whole spline.
	/// </summary>
	/// <returns>
	/// A normalized spline parameter.
	/// </returns>
	/// <param name='param'>
	/// The normalized segment parameter.
	/// </param>
	public float ConvertSegmentToSplineParamter( float param )
	{
		return (startNode.Parameters[parentSpline].PosInSpline + param * startNode.Parameters[parentSpline].Length);
	}
	
	/// <summary>
	/// This method converts a parameter [0..1] representing a point on the whole spline to a normalized parameter [0..1] representing a point on the segment.
	/// </summary>
	/// <returns>
	/// The normalized segment parameter.
	/// </returns>
	/// <param name='param'>
	/// The normalized spline parameter.
	/// </param>
	public float ConvertSplineToSegmentParamter( float param )
	{
		if( param < startNode.Parameters[parentSpline].PosInSpline )
			return 0;
		
		if( param > startNode.Parameters[parentSpline].PosInSpline + startNode.Parameters[parentSpline].Length )
			return 1;
		
		return ( (param - startNode.Parameters[parentSpline].PosInSpline) / startNode.Parameters[parentSpline].Length );
	}
	
	/// <summary>
	/// This method clamps a normalized spline parameter to spline parameters defining the segment. The returned parameter will only represent points on the segment.
	/// </summary>
	/// <returns>
	/// A clamped spline parameter that will only represent points on the segment.
	/// </returns>
	/// <param name='param'>
	/// A normalized spline parameter.
	/// </param>
	public float ClampParameterToSegment( float param )
	{
		if( param < startNode.Parameters[parentSpline].PosInSpline )
			return startNode.Parameters[parentSpline].PosInSpline;
		
		if( param > startNode.Parameters[parentSpline].PosInSpline + startNode.Parameters[parentSpline].Length )
			return startNode.Parameters[parentSpline].PosInSpline + startNode.Parameters[parentSpline].Length;
			
		return param;
	}
	
	/// <summary>
	/// This method checks whether a normalized spline parameter lies within the parameter interval of this spline segment.
	/// </summary>
	/// <returns>
	/// True if the parameter lies within the parameter interval of this spline segment.
	/// </returns>
	/// <param name='param'>
	/// A normalized spline parameter.
	/// </param>
	public bool IsParameterInRange( float param )
	{
		if( Mathf.Approximately( param, startNode.Parameters[parentSpline].PosInSpline ) )
			return true;
		
		if( Mathf.Approximately( param, startNode.Parameters[parentSpline].PosInSpline + startNode.Parameters[parentSpline].Length ) )
			return true;
		
		if( param < startNode.Parameters[parentSpline].PosInSpline )
			return false;
		
		if( param > startNode.Parameters[parentSpline].PosInSpline + startNode.Parameters[parentSpline].Length )
			return false;
		
		return true;
	}
}

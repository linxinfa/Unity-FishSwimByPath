using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// The BranchingSpline class groups multiple three-dimensional curves together, which enables junctions, and branched paths.
/// </summary>
/// <remarks>
/// It provides functions for calculate positions, rotations, etc. on and handling an arbitrarily branching path. The path is defined by individual splines.
/// These splines can be linked together by making them share their SplineNodes with each other. Each SplineNode that is used by two or more splines registred in the array,
/// will be treated as a junction. 
/// </remarks>
[AddComponentMenu("SuperSplines/Other/Branching Spline")]
public class BranchingSpline : MonoBehaviour 
{
	public List<Spline> splines = new List<Spline>( );									///< An array of Splines that will be used as possible paths.
	private int recoursionCounter = 0;
	
	/// <summary>
	/// This function adds an offset to a BranchingSplineParameter while automatically switching splines when a juction is passed.
	/// </summary>
	/// <param name='bParam'>
	/// A BranchingSplineParameter.
	/// </param>
	/// <param name='distanceOffset'>
	/// An offset that shall be added to the BranchingSplineParameter (in game units).
	/// </param>
	/// <param name='bController'>
	/// A BranchingController-delegate that decides which path to follow if a junction is passed.
	/// </param>
	/// <returns>
	/// True if the spline used as reference path has been changed; False if not.
	/// </returns>
	public bool Advance( BranchingSplineParameter bParam, float distanceOffset, BranchingController bController )
	{	
		bool splineChange = false;
		
		if( !SplinesAvailable )
			return false;
		
		if( ++recoursionCounter > 12 )
		{
			recoursionCounter = 0;
			return false;
		}
		
		CheckParameter( bParam );
		
		Spline currentSpline = bParam.spline;
		SplineNode currentNode = IsOnSplineNode( bParam.parameter, currentSpline );
		
		//Parameter on node?
		if( currentNode != null )
		{
			BranchingSplinePath nextPath = ChoseSpline( currentNode, bParam, bController, distanceOffset > 0 );
			
			bParam.spline = nextPath.spline;
			bParam.direction = nextPath.direction;
			bParam.parameter = currentNode.Parameters[bParam.spline].PosInSpline;
			
			SplineNode[] adjacentNodes = GetAdjacentSegmentNodes( nextPath.spline, currentNode );
			SplineNode nextNode = adjacentNodes[ForwardOnSpline( nextPath.direction, distanceOffset ) ? 1 : 0];
			
			if( nextNode != null )
			{
				bParam.parameter += (nextNode.Parameters[bParam.spline].PosInSpline - currentNode.Parameters[bParam.spline].PosInSpline) * 0.001f;
				Advance( bParam, distanceOffset, bController );
				
				splineChange = false;
			}
			else
			{
				splineChange = false;
			}
		}
		else
		{
			SplineSegment currentSegment = currentSpline.GetSplineSegment( bParam.parameter );
			
			float signedSplineLength = currentSpline.Length * (bParam.Forward ? 1 : -1);
			float normalizedOffsetDir = distanceOffset / signedSplineLength;
			
			float newParameter = bParam.parameter + normalizedOffsetDir;
			
			float clampedParameter = currentSegment.ClampParameterToSegment( newParameter );
			float offsetDifference = newParameter - clampedParameter;
			
			bParam.parameter = clampedParameter;
			
			if( Mathf.Approximately( offsetDifference, 0 ) )
				splineChange = false;
			else
				splineChange = Advance( bParam, offsetDifference * signedSplineLength, bController );
		}
		
		recoursionCounter = 0;
		
		return splineChange;
	}
	
	/// <summary>
	/// This function returns a point on the branched path for a BranchingSplineParameter.
	/// </summary>
	/// <returns>
	/// A point on the spline.
	/// </returns>
	/// <param name='bParam'>
	/// A BranchingSplineParameter.
	/// </param>
	public Vector3 GetPosition( BranchingSplineParameter bParam )
	{
		if( !SplinesAvailable )
			return Vector3.zero;
		
		CheckParameter( bParam );
		
		return bParam.spline.GetPositionOnSpline( bParam.parameter );
	}
	
	/// <summary>
	/// This function returns a rotation on the branched path for a BranchingSplineParameter.
	/// </summary>
	/// <returns>
	/// A rotation on the spline.
	/// </returns>
	/// <param name='bParam'>
	/// A BranchingSplineParameter.
	/// </param>
	public Quaternion GetOrientation( BranchingSplineParameter bParam )
	{
		if( !SplinesAvailable )
			return Quaternion.identity;
		
		CheckParameter( bParam );
		
		return bParam.spline.GetOrientationOnSpline( bParam.parameter );
	}
	
	/// <summary>
	/// This function returns a tangent to the branched path for a BranchingSplineParameter.
	/// </summary>
	/// <returns>
	/// A tangent to the spline.
	/// </returns>
	/// <param name='bParam'>
	/// A BranchingSplineParameter.
	/// </param>
	public Vector3 GetTangent( BranchingSplineParameter bParam )
	{
		if( !SplinesAvailable )
			return Vector3.zero;
		
		CheckParameter( bParam );
		
		return bParam.spline.GetTangentToSpline( bParam.parameter );
	}
	
	/// <summary>
	/// This function returns a custom value on the branched path for a BranchingSplineParameter.
	/// </summary>
	/// <returns>
	/// A custom value on the spline.
	/// </returns>
	/// <param name='bParam'>
	/// A BranchingSplineParameter.
	/// </param>
	public float GetCustomValue( BranchingSplineParameter bParam )
	{
		if( !SplinesAvailable )
			return 0;
		
		CheckParameter( bParam );
		
		return bParam.spline.GetCustomValueOnSpline( bParam.parameter );
	}
	
	
	/// <summary>
	/// This function returns a normal to the branched path for a BranchingSplineParameter.
	/// </summary>
	/// <returns>
	/// A normal to the spline.
	/// </returns>
	/// <param name='bParam'>
	/// A BranchingSplineParameter.
	/// </param>
	public Vector3 GetNormal( BranchingSplineParameter bParam )
	{
		if( !SplinesAvailable )
			return Vector3.zero;
		
		CheckParameter( bParam );
		
		return bParam.spline.GetNormalToSpline( bParam.parameter );
	}
	
	private BranchingSplinePath ChoseSpline( SplineNode switchNode, BranchingSplineParameter currentPath, BranchingController bController, bool positiveValue )
	{
		IList<Spline> possibleSplines = GetSplinesForNode( switchNode );
		
		List<BranchingSplinePath> possiblePaths = new List<BranchingSplinePath>( );
		
		//Eliminate unnecessary decisions
		if( possibleSplines.Count == 1 && possibleSplines[0] == currentPath.spline )
			return new BranchingSplinePath( currentPath.spline, currentPath.direction );
		
		if( IsMiddleNode( currentPath.spline, switchNode ) )
			possiblePaths.Add( new BranchingSplinePath( currentPath.spline, currentPath.direction ) );
		
		foreach( Spline spline in possibleSplines )
		{
			if( spline == currentPath.spline )
				continue;
			
			if( IsMiddleNode( spline, switchNode ) )
			{
				possiblePaths.Add( new BranchingSplinePath( spline, BranchingSplinePath.Direction.Forwards ) );
				possiblePaths.Add( new BranchingSplinePath( spline, BranchingSplinePath.Direction.Backwards ) );
			}
			else
			{
				SplineNode[] splineNodes = spline.SplineNodes;
		
				int nodeIndex = System.Array.IndexOf( splineNodes, switchNode );
						
				if( nodeIndex == 0 )
					possiblePaths.Add( new BranchingSplinePath( spline, positiveValue?BranchingSplinePath.Direction.Forwards:BranchingSplinePath.Direction.Backwards ) );
				
				if( nodeIndex == splineNodes.Length-1 )
					possiblePaths.Add( new BranchingSplinePath( spline, !positiveValue?BranchingSplinePath.Direction.Forwards:BranchingSplinePath.Direction.Backwards ) );
			}
		}
		
		return bController( currentPath, possiblePaths );
	}
	
	private SplineNode IsOnSplineNode( float param, Spline spline )
	{
		foreach( SplineNode node in spline.SegmentNodes )
			if( Mathf.Approximately( node.Parameters[spline].PosInSpline, param ) )
				return node;
		
		return null;
	}
	
	private SplineNode[] GetAdjacentSegmentNodes( Spline spline, SplineNode node )
	{
		SplineNode[] segmentNodes = spline.SegmentNodes;
		SplineNode[] returnNodes = new SplineNode[2];
		
		int index = System.Array.IndexOf( segmentNodes, node );
		
		returnNodes[0] = index<=0 ? null : segmentNodes[index-1];
		returnNodes[1] = index>=segmentNodes.Length-1 ? null : segmentNodes[index+1];
		
		return returnNodes;
	}
	
	private bool ForwardOnSpline( BranchingSplinePath.Direction direction, float v )
	{
		if( direction == BranchingSplinePath.Direction.Forwards )
			return v > 0;
		else
			return v < 0;
	}
	
	private bool IsMiddleNode( Spline spline, SplineNode node )
	{
		SplineNode[] splineNodes = spline.SplineNodes;
		
		int nodeIndex = System.Array.IndexOf( splineNodes, node );
				
		if( nodeIndex == 0 )
			return false;
		
		if( nodeIndex == splineNodes.Length-1 )
			return false;
		
		return true;
	}
	
	private bool SplinesAvailable{ 
		get{ 
			if( splines == null )
				return false;
			else if( splines.Count <= 0 )
				return false;
			
			return true;
		}
	}
	
	private IList<Spline> GetSplinesForNode( SplineNode node )
	{
		List<Spline> possibleSplines = new List<Spline>( );
		
		foreach( Spline spline in splines )
		{
			foreach( SplineNode splineNode in spline.SplineNodes )
			{
				if( node == splineNode )
					possibleSplines.Add( spline );
			}
		}
		
		return possibleSplines;
	}
	
	private void CheckParameter( BranchingSplineParameter bParam )
	{
		if( !SplinesAvailable )
			return;
		else if( bParam.spline == null )
			bParam.spline = splines[0];
		else if( !splines.Contains( bParam.spline ) )
			bParam.spline = splines[0];
		
		bParam.parameter = Mathf.Clamp01( bParam.parameter );
	}
	
	/// <summary>
	/// A delegate for a function that choses a BranchingSplinePath from an array of possible paths.
	/// </summary>
	/// <remarks>
	/// This function will be called by the Advance-mehtod when a junction is passed. The function has to decide which path of the possible paths passed as parameter shall be used.
	/// </remarks>
	/// <returns>
	/// One of the BranchingSplinePaths from the possiblePaths array. 
	/// </returns>
	/// <param name='currentParameter'>
	/// The current BranchingSplineParameter that contains information about the spline that was previously used for interpolation.
	/// This information can be used as reference for the decision which path of the possible paths shall be taken.
	/// </param>
	/// <param name='possiblePaths'>
	/// An array of possible paths. It might contain some splines twice, but with different directions. This can be used for crossing splines.
	/// </param>
	public delegate BranchingSplinePath BranchingController( BranchingSplineParameter currentParameter, List<BranchingSplinePath> possiblePaths );
}

/// <summary>
/// The BranchingSplinePath class combines a spline with a direction reference.
/// </summary>
[System.Serializable]
public class BranchingSplinePath
{
	public Spline spline;					///< A spline that the defines a specific path
	public Direction direction;				///< Specifies which directional orientation of the path
	
	protected BranchingSplinePath( )
	{
		spline = null;
		direction = Direction.Forwards;
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="BranchingSplinePath"/> class.
	/// </summary>
	/// <param name='spline'>
	/// A spline that defines the path.
	/// </param>
	public BranchingSplinePath( Spline spline )
	{
		this.spline = spline;
		this.direction = Direction.Forwards;
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="BranchingSplinePath"/> class.
	/// </summary>
	/// <param name='spline'>
	/// A spline that defines the path.
	/// </param>
	/// <param name='direction'>
	/// The direction of the path.
	/// </param>
	public BranchingSplinePath( Spline spline, Direction direction )
	{
		this.spline = spline;
		this.direction = direction;
	}
	
	/// <summary>
	/// Specifies the direction of a BranchingSplinePath
	/// </summary>
	public enum Direction
	{
		Forwards,		///< Specifies that a path will be passed according to the regular 'direction' of its spline. A spline parameter of 0 will reference the spline's start node.
		Backwards		///< Specifies that a path will be passed against the regular 'direction' of its spline. A spline parameter of 0 will reference the spline's end node.
	}
}

/// <summary>
/// The BranchingSplineParameter class extends the BranchingSplinePath by a normalized spline parameter.
/// </summary>
/// <remarks>
/// It represents a location on the spline that is associated with a specific direction of movement on the spline.
/// </remarks>
[System.Serializable]
public class BranchingSplineParameter : BranchingSplinePath
{
	public bool Forward{ get{ return direction == BranchingSplinePath.Direction.Forwards; } }	///< Quickly checks if the direction field is set to BranchingSplinePath.Direction.Forwards.
	
	public float parameter;		///< A normalized spline parameter that represents a specific location on the path
	
	/// <summary>
	/// Initializes a new instance of the <see cref="BranchingSplineParameter"/> class.
	/// </summary>
	public BranchingSplineParameter( )
	{
		this.spline = null;
		this.parameter = 0;
		this.direction = BranchingSplinePath.Direction.Forwards;
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="BranchingSplineParameter"/> class.
	/// </summary>
	/// <param name='spline'>
	/// A spline that defines the used path.
	/// </param>
	/// <param name='parameter'>
	/// A location on the path.
	/// </param>
	public BranchingSplineParameter( Spline spline, float parameter )
	{
		this.spline = spline;
		this.parameter = parameter;
		this.direction = BranchingSplinePath.Direction.Forwards;
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="BranchingSplineParameter"/> class.
	/// </summary>
	/// <param name='spline'>
	/// A spline that defines the used path.
	/// </param>
	/// <param name='parameter'>
	/// A location on the path.
	/// </param>
	/// <param name='forward'>
	/// Specifies the direction of the path / movement on the path.
	/// </param>
	public BranchingSplineParameter( Spline spline, float parameter, bool forward )
	{
		this.spline = spline;
		this.parameter = parameter;
		this.direction = forward ? BranchingSplinePath.Direction.Forwards : BranchingSplinePath.Direction.Backwards;
	}
}

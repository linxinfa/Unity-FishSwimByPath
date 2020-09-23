using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class represents a control node of the Spline.
/// </summary>
/// <remarks>
/// This class stores data about the position and orientation of the control node as well as information about 
/// the spline parameter that is associated to the control node and the normalized distance to the next adjacent control node.
/// For advanced use there is also a custom value that will be interpolated according to the interpolation mode that is used to calculate the spline.
/// </remarks>
[AddComponentMenu("SuperSplines/Spline Node")]
public class SplineNode : MonoBehaviour
{

    ///< Quick access to the control node's position.
    public Vector3 Position { get { return transform.position; } set { transform.position = value; } }

    ///< Quick access to the control node's orientation.
    public Quaternion Rotation { get { return transform.rotation; } set { transform.rotation = value; } }

    public float CustomValue { get { return customValue; } set { customValue = value; } }                   ///< Quick access to the control node's custom value.

    ///< Quick access to the control node's transformed normal.
    public Vector3 TransformedNormal { get { return Vector3.zero; } }

    public float customValue = 0f;									///< A custom value that can be interpolated by the Spline-class, for advanced applications
	public float tension = 1f;										///< Specifies the curve's tension at the node's position (used for hermite-interpolation)
	public Vector3 normal = Vector3.up;                             ///< Represents the spline's up-vector / normal at the node's position

    /// <summary>
    /// A dictionary of the spline's parameters.
    /// </summary>
    /// <remarks>
    /// Elements of the dictionary can be accessed by the spline that the parameter's are needed for.
    /// </remarks>
    /// <example>
    /// Get the location of the node on a specific spline:
    /// <code>
    /// 
    /// Spline mySpline;
    /// SplineNode node;
    /// 
    /// float splineParameter = node.Parameters[mySpline].PosOnSpline;
    /// float distanceToNextNode = node.Parameters[mySpline].Length;
    /// 
    /// </code>
    /// </example>
    /// <value>
    /// The parameters.
    /// </value>
    public NodeParameterRegister Parameters { get { return parameters; } }
    private NodeParameterRegister parameters = new NodeParameterRegister();

    /// <summary>
    /// A register for spline related node parameters.
    /// </summary>
    /// <remarks>
    /// The node's parameters can be accessed using the indexer with a specific spline as parameter.
    /// </remarks>
    public sealed class NodeParameterRegister : Dictionary<Spline, NodeParameters>
    {
        /// <summary>
        /// Gets the <see cref="NodeParameters"/> for the specified spline.
        /// </summary>
        /// <remarks>
        /// If there is no entry for the passed Spline reference, a new instance of NodeParameters will be generated.
        /// </remarks>
        /// <param name='spline'>
        /// A spline that the node is used in.
        /// </param>
        public new NodeParameters this[Spline spline]
        {
            get
            {
                if (!ContainsKey(spline))
                    Add(spline, new NodeParameters(spline, 0, 0));

                return base[spline];
            }
        }
    }

    //	private void OnDrawGizmosSelected( )
    //	{
    //		Gizmos.color = new Color( 1f, 0.5f, 0f, 0.75f );
    //		Gizmos.DrawSphere( Position, GetSizeMultiplier( this ) * 2.5f );
    //	}

    //	Copied from SplineGizmos.cs
    //	private float GetSizeMultiplier( SplineNode node )
    //	{
    //		if( !Camera.current.orthographic )
    //		{
    //			Plane screen = new Plane( );
    //			
    //			float sizeMultiplier = 0f;
    //			
    //			screen.SetNormalAndPosition( Camera.current.transform.forward, Camera.current.transform.position );
    //			screen.Raycast( new Ray( node.Position, Camera.current.transform.forward ), out sizeMultiplier );
    //			
    //			return sizeMultiplier * .0075f;
    //		}
    //	
    //		return Camera.current.orthographicSize * 0.01875f;
    //	}
}

/// <summary>
/// The parameters of SplineNode that specify its position on the spline in relation to its length. 
/// </summary>
public class NodeParameters
{
    public double position;
    public double length;

    public Spline spline;

    public float PosInSpline { get { return (float)position; } } 	///< Normalized position on the spline (parameter from 0 to 1).
	public float Length { get { return (float)length; } }           ///< Normalized distance to the next adjacent node.

    /// <summary>
    /// Initializes a new instance of the <see cref="NodeParameters"/> class.
    /// </summary>
    /// <param name='spline'>
    /// The Spline that the node belongs to.
    /// </param>
    /// <param name='position'>
    /// The spline parameter representing this node.
    /// </param>
    /// <param name='length'>
    /// The normalized distance to the next node.
    /// </param>
    public NodeParameters(Spline spline, float position, float length)
    {
        this.position = position;
        this.length = length;
        this.spline = spline;
    }

    /// <summary>
    /// Reset position and length of the node to zero.
    /// </summary>
    public void Reset()
    {
        position = 0;
        length = 0;
    }
}

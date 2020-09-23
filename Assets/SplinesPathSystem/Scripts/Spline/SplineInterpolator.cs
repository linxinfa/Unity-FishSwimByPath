using UnityEngine;
using System;

using QuaternionUtilities;
using System.Collections.Generic;

public partial class Spline : MonoBehaviour
{
	SplineInterpolator splineInterpolator = new HermiteInterpolator( );		///< The SplineInterpolator that will be used for spline interpolation. 
}

/// <summary>
/// The SplineInterpolator class provides functions that are necessary to interpolate positions, rotations, values, etc. of and on splines.
/// </summary>
/// <remarks>
/// It can be used to manually interpolate an array of vectors/quaternions/values without having to instanciate a whole new spline. 
/// Depending on its settings it provides different interpolation modes. 
/// 
/// Please note that this class doesn't provide constant-velocity interpolation, when used on its own. If you need constant-velocity interpolation
/// use the regular Spline class!
/// </remarks>
[Serializable]
public class SplineInterpolator
{
	protected double[] coefficientMatrix;
	protected int[] nodeIndices;
	
	protected SplineInterpolator( )
	{
		
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="SplineInterpolator"/> class.
	/// </summary>
	/// <param name='coefficientMatrix'>
	/// The coefficient 4x4 matrix that will be used for the spline interpolation.
	/// The array must contain exactly 16 elements!
	/// </param>
	/// <param name='nodeIndices'>
	/// The relative indices of the nodes that are needed for the interpolation. 
	/// The array must contain exactly 4 elements!
	/// </param>
	public SplineInterpolator( double[] coefficientMatrix, int[] nodeIndices )
	{
		CheckMatrix( coefficientMatrix );
		CheckIndices( nodeIndices );
		
		this.coefficientMatrix = coefficientMatrix;
		this.nodeIndices = nodeIndices;
	}
	
	public double[] CoefficientMatrix{ 
		get{ return coefficientMatrix; }
		set{ CheckMatrix( value ); coefficientMatrix = value; }
	} ///< Returns or sets the coefficients matrix for custom interpolation
	
	public int[] NodeIndices{ 
		get{ return nodeIndices; }
		set{ CheckIndices( value ); nodeIndices = value; }
	} ///< Returns or sets the coefficients matrix for custom interpolation
	
	public Matrix4x4 CoefficientMatrix4x4{ 
		get{
			Matrix4x4 matrix = new Matrix4x4( );
			
			matrix[ 0] = (float)coefficientMatrix[ 0]; matrix[ 1] = (float)coefficientMatrix[ 1]; matrix[ 2] = (float)coefficientMatrix[ 2]; matrix[ 3] = (float)coefficientMatrix[ 3];
			matrix[ 4] = (float)coefficientMatrix[ 4]; matrix[ 5] = (float)coefficientMatrix[ 5]; matrix[ 6] = (float)coefficientMatrix[ 6]; matrix[ 7] = (float)coefficientMatrix[ 7];
			matrix[ 8] = (float)coefficientMatrix[ 8]; matrix[ 9] = (float)coefficientMatrix[ 9]; matrix[10] = (float)coefficientMatrix[10]; matrix[11] = (float)coefficientMatrix[11];
			matrix[12] = (float)coefficientMatrix[12]; matrix[13] = (float)coefficientMatrix[13]; matrix[14] = (float)coefficientMatrix[14]; matrix[15] = (float)coefficientMatrix[15];
			
			return matrix;
		}
		set{
			coefficientMatrix[ 0] = value[ 0]; coefficientMatrix[ 1] = value[ 1]; coefficientMatrix[ 2] = value[ 2]; coefficientMatrix[ 3] = value[ 3];
			coefficientMatrix[ 4] = value[ 4]; coefficientMatrix[ 5] = value[ 5]; coefficientMatrix[ 6] = value[ 6]; coefficientMatrix[ 7] = value[ 7];
			coefficientMatrix[ 8] = value[ 8]; coefficientMatrix[ 9] = value[ 9]; coefficientMatrix[10] = value[10]; coefficientMatrix[11] = value[11];
			coefficientMatrix[12] = value[12]; coefficientMatrix[13] = value[13]; coefficientMatrix[14] = value[14]; coefficientMatrix[15] = value[15];
		}
	} ///< Returns or sets the coefficients matrix for custom interpolation
	
	/// <summary>
	/// Interpolates an array of vectors.
	/// </summary>
	/// <returns>
	/// The interpolated vector.
	/// </returns>
	/// <param name='t'>
	/// An interpolation parameter from 0 to 1.
	/// </param>
	/// <param name='index'>
	/// The index of the current spline segment.
	/// </param>
	/// <param name='autoClose'>
	/// If enabled the vector array will be treated as closed spline.
	/// </param>
	/// <param name='nodes'>
	/// An array of vectors.
	/// </param>
	/// <param name='derivationOrder'>
	/// The derivation order.
	/// </param>
	public virtual Vector3 InterpolateVector( double t, int index, bool autoClose, IList<Vector3> nodes, int derivationOrder )
	{
		Vector3 v0; Vector3 v1;
		Vector3 v2; Vector3 v3;
		
		GetNodeData( nodes, index, autoClose, out v0, out v1, out v2, out v3 );
		
		return InterpolateVector( t, v0, v1, v2, v3, derivationOrder );
	}
	
	/// <summary>
	/// Interpolates the positions of an array of SplineNodes.
	/// </summary>
	/// <returns>
	/// The interpolated position.
	/// </returns>
	/// <param name='t'>
	/// An interpolation parameter from 0 to 1.
	/// </param>
	/// <param name='index'>
	/// The index of the current spline segment.
	/// </param>
	/// <param name='autoClose'>
	/// If enabled the node array will be treated as closed spline.
	/// </param>
	/// <param name='nodes'>
	/// An array of SplineNodes.
	/// </param>
	/// <param name='derivationOrder'>
	/// The derivation order.
	/// </param>
	public virtual Vector3 InterpolateVector( Spline spline, double t, int index, bool autoClose, IList<SplineNode> nodes, int derivationOrder )
	{
		SplineNode n0; SplineNode n1;
		SplineNode n2; SplineNode n3;
		
		GetNodeData( nodes, index, autoClose, out n0, out n1, out n2, out n3 );
		
		return InterpolateVector( t, n0.Position, n1.Position, n2.Position, n3.Position, derivationOrder );
	}
	
	/// <summary>
	/// Interpolates an array of values.
	/// </summary>
	/// <returns>
	/// The interpolated value
	/// </returns>
	/// <param name='t'>
	/// An interpolation parameter from 0 to 1.
	/// </param>
	/// <param name='index'>
	/// The index of the current spline segment.
	/// </param>
	/// <param name='autoClose'>
	/// If enabled the value array will be treated as closed spline.
	/// </param>
	/// <param name='nodes'>
	/// An array of float values
	/// </param>
	/// <param name='derivationOrder'>
	/// Derivation order.
	/// </param>
	public virtual float InterpolateValue( double t, int index, bool autoClose, IList<float> nodes, int derivationOrder )
	{
		float v0; float v1;
		float v2; float v3;
		
		GetNodeData( nodes, index, autoClose, out v0, out v1, out v2, out v3 );
		
		return InterpolateValue( t, v0, v1, v2, v3, derivationOrder );
	}
	
	/// <summary>
	/// Interpolates the custom values of an array of SplineNodes.
	/// </summary>
	/// <returns>
	/// The interpolated value.
	/// </returns>
	/// <param name='t'>
	/// An interpolation parameter from 0 to 1.
	/// </param>
	/// <param name='index'>
	/// The index of the current spline segment.
	/// </param>
	/// <param name='autoClose'>
	/// If enabled the node array will be treated as closed spline.
	/// </param>
	/// <param name='nodes'>
	/// An array of SplineNodes
	/// </param>
	/// <param name='derivationOrder'>
	/// Derivation order.
	/// </param>
	public virtual float InterpolateValue( Spline spline, double t, int index, bool autoClose, IList<SplineNode> nodes, int derivationOrder )
	{
		SplineNode n0; SplineNode n1;
		SplineNode n2; SplineNode n3;
		
		GetNodeData( nodes, index, autoClose, out n0, out n1, out n2, out n3 );
		
		return InterpolateValue( t, n0.CustomValue, n1.CustomValue, n2.CustomValue, n3.CustomValue, derivationOrder );
	}
	
	/// <summary>
	/// Interpolates an array of rotations.
	/// </summary>
	/// <returns>
	/// The interpolated rotation.
	/// </returns>
	/// <param name='t'>
	/// An interpolation parameter from 0 to 1.
	/// </param>
	/// <param name='index'>
	/// The index of the current spline segment.
	/// </param>
	/// <param name='autoClose'>
	/// If enabled the quaternion array will be treated as closed spline.
	/// </param>
	/// <param name='nodes'>
	/// An array of rotations. 
	/// </param>
	/// <param name='derivationOrder'>
	/// Derivation order.
	/// </param>
	public virtual Quaternion InterpolateRotation( double t, int index, bool autoClose, IList<Quaternion> nodes, int derivationOrder )
	{
		Quaternion q0; Quaternion q1;
		Quaternion q2; Quaternion q3;
		
		GetNodeData( nodes, index, autoClose, out q0, out q1, out q2, out q3 );
		
		return InterpolateRotation( t, q0, q1, q2, q3, derivationOrder );
	}
	
	/// <summary>
	/// Interpolates the rotations of an array of SplineNodes.
	/// </summary>
	/// <returns>
	/// The interpolated rotation.
	/// </returns>
	/// <param name='t'>
	/// An interpolation parameter from 0 to 1.
	/// </param>
	/// <param name='index'>
	/// The index of the current spline segment.
	/// </param>
	/// <param name='autoClose'>
	/// If enabled the node array will be treated as closed spline.
	/// </param>
	/// <param name='nodes'>
	/// An array of SplineNodes. 
	/// </param>
	/// <param name='derivationOrder'>
	/// Derivation
	/// </param>
	public virtual Quaternion InterpolateRotation( Spline spline, double t, int index, bool autoClose, IList<SplineNode> nodes, int derivationOrder )
	{
		SplineNode n0; SplineNode n1;
		SplineNode n2; SplineNode n3;
		
		GetNodeData( nodes, index, autoClose, out n0, out n1, out n2, out n3 );
		
		return InterpolateRotation( t, n0.Rotation, n1.Rotation, n2.Rotation, n3.Rotation, derivationOrder );
	}
	
	/// <summary>
	/// Interpolates 4 vectors.
	/// </summary>
	/// <returns>
	/// The interpolated vector.
	/// </returns>
	/// <param name='t'>
	/// An interpolation parameter from 0 to 1.
	/// </param>
	/// <param name='v0'>
	/// 1st Vector
	/// </param>
	/// <param name='v1'>
	/// 2nd Vector
	/// </param>
	/// <param name='v2'>
	/// 3rd Vector
	/// </param>
	/// <param name='v3'>
	/// 4th Vector
	/// </param>
	/// <param name='derivationOrder'>
	/// Derivation order.
	/// </param>
	public Vector3 InterpolateVector( double t, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int derivationOrder )
	{
		float b0; float b1;
		float b2; float b3;
		
		GetCoefficients( t, out b0, out b1, out b2, out b3, derivationOrder );
		
		return b0 * v0 + b1 * v1 + b2 * v2 + b3 * v3;
	}
	
	/// <summary>
	/// Interpolates 4 float values.
	/// </summary>
	/// <returns>
	/// The interpolated float value.
	/// </returns>
	/// <param name='t'>
	/// An interpolation parameter from 0 to 1.
	/// </param>
	/// <param name='v0'>
	/// 1st float value
	/// </param>
	/// <param name='v1'>
	/// 2nd float value
	/// </param>
	/// <param name='v2'>
	/// 3rd float value
	/// </param>
	/// <param name='v3'>
	/// 4th float value
	/// </param>
	/// <param name='derivationOrder'>
	/// Derivation order.
	/// </param>
	public float InterpolateValue( double t, float v0, float v1, float v2, float v3, int derivationOrder )
	{
		float b0; float b1;
		float b2; float b3;
		
		GetCoefficients( t, out b0, out b1, out b2, out b3, derivationOrder );
		
		return b0 * v0 + b1 * v1 + b2 * v2 + b3 * v3;
	}
	
	/// <summary>
	/// Interpolates 4 quaternions.
	/// </summary>
	/// <returns>
	/// The interpolated quaternion.
	/// </returns>
	/// <param name='t'>
	/// An interpolation parameter from 0 to 1.
	/// </param>
	/// <param name='q0'>
	/// 1st float quaternion
	/// </param>
	/// <param name='q1'>
	/// 2nd float quaternion
	/// </param>
	/// <param name='q2'>
	/// 3rd float quaternion
	/// </param>
	/// <param name='q3'>
	/// 4th float quaternion
	/// </param>
	/// <param name='derivationOrder'>
	/// Derivation order.
	/// </param>
	public Quaternion InterpolateRotation( double t, Quaternion q0, Quaternion q1, Quaternion q2, Quaternion q3, int derivationOrder )
	{
		float b0; float b1;
		float b2; float b3;
		
		if( Quaternion.Dot( q0, q1 ) < 0 )
			q1 = q1.Negative( );
		
		if( Quaternion.Dot( q1, q2 ) < 0 )
			q2 = q2.Negative( );
		
		if( Quaternion.Dot( q2, q3 ) < 0 )
			q3 = q3.Negative( );
		
		GetCoefficients( t, out b0, out b1, out b2, out b3, derivationOrder );
		
		Vector3 imaginary0 = new Vector3( q0.x, q0.y, q0.z );
		Vector3 imaginary1 = new Vector3( q1.x, q1.y, q1.z );
		Vector3 imaginary2 = new Vector3( q2.x, q2.y, q2.z );
		Vector3 imaginary3 = new Vector3( q3.x, q3.y, q3.z );
		
		Vector3 interpolatedImaginary = b0 * imaginary0 + b1 * imaginary1 + b2 * imaginary2 + b3 * imaginary3;
		float interpolatedReal = b0 * q0.w + b1 * q1.w + b2 * q2.w + b3 * q3.w;
		
		Quaternion result = new Quaternion( interpolatedImaginary.x, interpolatedImaginary.y, interpolatedImaginary.z, interpolatedReal );
		
		return result.Normalized( );
	}
	
	/// <summary>
	/// Select 4 elements of an array according to the indices of nodes that are used for interpolation (nodeIndices).
	/// </summary>
	/// <param name='array'>
	/// A generic array.
	/// </param>
	/// <param name='index'>
	/// The index of the currently processed spline node
	/// </param>
	/// <param name='autoClose'>
	/// If set to true, the array will be treated as "looping array" / closed spline.
	/// </param>
	/// <param name='d0'>
	/// 1st element.
	/// </param>
	/// <param name='d1'>
	/// 2nd element.
	/// </param>
	/// <param name='d2'>
	/// 3rd element.
	/// </param>
	/// <param name='d3'>
	/// 4th element
	/// </param>
	/// <typeparam name='T'>
	/// The type of the array's elements.
	/// </typeparam>
	public void GetNodeData<T>( IList<T> array, int index, bool autoClose, out T d0, out T d1, out T d2, out T d3 )
	{
		int arrayLength = array.Count;
		
		d0 = array[GetNodeIndex( autoClose, arrayLength, index, nodeIndices[0] )];
		d1 = array[GetNodeIndex( autoClose, arrayLength, index, nodeIndices[1] )];
		d2 = array[GetNodeIndex( autoClose, arrayLength, index, nodeIndices[2] )];
		d3 = array[GetNodeIndex( autoClose, arrayLength, index, nodeIndices[3] )];
	}
	
	private int GetNodeIndex( bool autoClose, int arrayLength, int index, int offset )
	{
		int idxNode = index + offset;
		
		if( autoClose )
			return (idxNode % arrayLength + arrayLength) % arrayLength;
		else
			return Mathf.Clamp( idxNode, 0, arrayLength-1 );
	}
	
	private void GetCoefficients( double t, out float b0, out float b1, out float b2, out float b3, int derivationOrder )
	{
		switch( derivationOrder )
		{
		case 0: GetCoefficients( t, out b0, out b1, out b2, out b3 ); return;
		case 1: GetCoefficientsFirstDerivative( t, out b0, out b1, out b2, out b3 ); return;
		case 2: GetCoefficientsSecondDerivative( t, out b0, out b1, out b2, out b3 ); return;
		}
		
		b0 = 0; b1 = 0; b2 = 0; b3 = 0;
	}
	
	private void GetCoefficients( double t, out float b0, out float b1, out float b2, out float b3 )
	{
		double t2 = t * t;
		double t3 = t2 * t;
		
		b0 = (float) (coefficientMatrix[ 0] * t3 + coefficientMatrix[ 1] * t2 + coefficientMatrix[ 2] * t + coefficientMatrix[ 3]);
		b1 = (float) (coefficientMatrix[ 4] * t3 + coefficientMatrix[ 5] * t2 + coefficientMatrix[ 6] * t + coefficientMatrix[ 7]);
		b2 = (float) (coefficientMatrix[ 8] * t3 + coefficientMatrix[ 9] * t2 + coefficientMatrix[10] * t + coefficientMatrix[11]);
		b3 = (float) (coefficientMatrix[12] * t3 + coefficientMatrix[13] * t2 + coefficientMatrix[14] * t + coefficientMatrix[15]);
	} ///< Returns the interpolation coefficients for the curve
	
	private void GetCoefficientsFirstDerivative( double t, out float b0, out float b1, out float b2, out float b3 )
	{
		double t2 = t * t;
		
		t = t * 2.0;
		t2 = t2 * 3.0;
		
		b0 = (float) (coefficientMatrix[ 0] * t2 + coefficientMatrix[ 1] * t + coefficientMatrix[ 2]);
		b1 = (float) (coefficientMatrix[ 4] * t2 + coefficientMatrix[ 5] * t + coefficientMatrix[ 6]);
		b2 = (float) (coefficientMatrix[ 8] * t2 + coefficientMatrix[ 9] * t + coefficientMatrix[10]);
		b3 = (float) (coefficientMatrix[12] * t2 + coefficientMatrix[13] * t + coefficientMatrix[14]);
	} ///< Returns the interpolation coefficients for the curve's first derivative 
	
	private void GetCoefficientsSecondDerivative( double t, out float b0, out float b1, out float b2, out float b3 )
	{
		t *= 6.0;
		
		b0 = (float) (coefficientMatrix[ 0] * t + coefficientMatrix[ 1] * 2f);
		b1 = (float) (coefficientMatrix[ 4] * t + coefficientMatrix[ 5] * 2f);
		b2 = (float) (coefficientMatrix[ 8] * t + coefficientMatrix[ 9] * 2f);
		b3 = (float) (coefficientMatrix[12] * t + coefficientMatrix[13] * 2f);
	} ///< Returns the interpolation coefficients for the curve's second derivative
	
	private void CheckMatrix( double[] coefficientMatrix )
	{
		if( coefficientMatrix.Length != 16 )
			throw new ArgumentException( "The coefficientMatrix-array must contain exactly 16 doubles!" );
	}
	
	private void CheckIndices( int[] nodeIndices )
	{
		if( nodeIndices.Length != 4 )
			throw new ArgumentException( "nodeIndices-array must contain exactly 4 ints!" );
	}
}

/// <summary>
/// A Hermite Interpolator.
/// </summary>
/// <remarks>
/// In contrast to the other default SplineInterpolators this Interpolator provides special functions that are necessary for hermite spline interpolation
/// </remarks>
public class HermiteInterpolator : SplineInterpolator
{
	/// <summary>
	/// Initializes a new instance of the <see cref="HermiteInterpolator"/> class.
	/// </summary>
	public HermiteInterpolator( )
	{
		CoefficientMatrix = new double[] {
			 2.0, -3.0,  0.0,  1.0,
			-2.0,  3.0,  0.0,  0.0,
			 1.0, -2.0,  1.0,  0.0,
			 1.0, -1.0,  0.0,  0.0
		};
		
		NodeIndices = new int[] {  0, 1, -1, 2 };
	}
	
	public override Vector3 InterpolateVector( Spline spline, double t, int index, bool autoClose, IList<SplineNode> nodes, int derivationOrder )
	{
		SplineNode n0; SplineNode n1;
		SplineNode n2; SplineNode n3;
		
		GetNodeData( nodes, index, autoClose, out n0, out n1, out n2, out n3 );
		
		Vector3 v2 = n2.Position; 
		Vector3 v3 = n3.Position;
		
		RecalcVectors( spline, n0, n1, ref v2, ref v3 );
		
		return InterpolateVector( t, n0.Position, n1.Position, v2, v3, derivationOrder );
	}
	
	public override Vector3 InterpolateVector( double t, int index, bool autoClose, IList<Vector3> nodes, int derivationOrder )
	{
		Vector3 v0; Vector3 v1;
		Vector3 v2; Vector3 v3;
		
		GetNodeData( nodes, index, autoClose, out v0, out v1, out v2, out v3 );
		
		RecalcVectors( v0, v1, ref v2, ref v3 );
		
		return InterpolateVector( t, v0, v1, v2, v3, derivationOrder );
	}
	
	public override float InterpolateValue( Spline spline, double t, int index, bool autoClose, IList<SplineNode> nodes, int derivationOrder )
	{
		SplineNode n0; SplineNode n1;
		SplineNode n2; SplineNode n3;
		
		GetNodeData( nodes, index, autoClose, out n0, out n1, out n2, out n3 );
		
		float v2 = n2.CustomValue; 
		float v3 = n3.CustomValue;
		
		RecalcScalars( spline, n0, n1, ref v2, ref v3 );
		
		return InterpolateValue( t, n0.CustomValue, n1.CustomValue, v2, v3, derivationOrder );
	}
	
	public override float InterpolateValue( double t, int index, bool autoClose, IList<float> nodes, int derivationOrder )
	{
		float v0; float v1;
		float v2; float v3;
		
		GetNodeData( nodes, index, autoClose, out v0, out v1, out v2, out v3 );
		
		RecalcScalars( v0, v1, ref v2, ref v3 );
		
		return InterpolateValue( t, v0, v1, v2, v3, derivationOrder );
	}
	
	public override Quaternion InterpolateRotation( Spline spline, double t, int index, bool autoClose, IList<SplineNode> nodes, int derivationOrder )
	{
		SplineNode n0; SplineNode n1;
		SplineNode n2; SplineNode n3;
		
		GetNodeData( nodes, index, autoClose, out n0, out n1, out n2, out n3 );
		
		Quaternion q2 = n2.Rotation; 
		Quaternion q3 = n3.Rotation;
		
		RecalcRotations( n0.Rotation, n1.Rotation, ref q2, ref q3 );
		
		return InterpolateRotation( t, n0.Rotation, n1.Rotation, q2, q3, derivationOrder );
	}
	
	/// <summary>
	/// A function that returns the tangents for two spline nodes
	/// </summary>
	/// <param name='spline'>
	/// The used spline
	/// </param>
	/// <param name='node0'>
	/// 1st node.
	/// </param>
	/// <param name='node1'>
	/// 2nd node.
	/// </param>
	/// <param name='P2'>
	/// In: Position of 3rd node.
	/// Out: Tangent of node0.
	/// </param>
	/// <param name='P3'>
	/// In: Position of 4th node.
	/// Out: Tangent of node1.
	/// </param>
	public void RecalcVectors( Spline spline, SplineNode node0, SplineNode node1, ref Vector3 P2, ref Vector3 P3 )
	{	
		float tension0;
		float tension1;
		
		if( spline.perNodeTension )
		{
			tension0 = node0.tension;
			tension1 = node1.tension;
		}
		else
		{
			tension0 = spline.tension;
			tension1 = spline.tension;
		}
		
		if( spline.tangentMode == Spline.TangentMode.UseNodeForwardVector )
		{
			P2 = node0.transform.forward * tension0;
			P3 = node1.transform.forward * tension1;
		}
		else
		{
			P2 = node1.Position - P2;
			P3 = P3 - node0.Position;
			
			if( spline.tangentMode != Spline.TangentMode.UseTangents )
			{
				P2.Normalize( );
				P3.Normalize( );
			}
			
			P2 = P2 * tension0;
			P3 = P3 * tension1;
		}
	}
	
	/// <summary>
	/// A function that returns the tangents for two spline nodes.
	/// </summary>
	/// <param name='P0'>
	/// Position of 1st spline node.
	/// </param>
	/// <param name='P1'>
	/// Position of 2nd spline node.
	/// </param>
	/// <param name='P2'>
	/// In: Position of 3rd node.
	/// Out: Tangent of node0.
	/// </param>
	/// <param name='P3'>
	/// In: Position of 4th node.
	/// Out: Tangent of node1.
	/// </param>
	public void RecalcVectors( Vector3 P0, Vector3 P1, ref Vector3 P2, ref Vector3 P3 )
	{
		float tension = 0.5f;
		
		P2 = P1 - P2;
		P3 = P3 - P0;
		
		P2 = P2 * tension;
		P3 = P3 * tension;
	}
	
	/// <summary>
	/// A function that returns the tangents for two spline nodes
	/// </summary>
	/// <param name='spline'>
	/// The used spline
	/// </param>
	/// <param name='node0'>
	/// 1st node.
	/// </param>
	/// <param name='node1'>
	/// 2nd node.
	/// </param>
	/// <param name='P2'>
	/// In: Value of 3rd node.
	/// Out: Tangent of node0.
	/// </param>
	/// <param name='P3'>
	/// In: Value of 4th node.
	/// Out: Tangent of node1.
	/// </param>
	public void RecalcScalars( Spline spline, SplineNode node0, SplineNode node1, ref float P2, ref float P3 )
	{
		float tension0;
		float tension1;
		
		if( spline.perNodeTension )
		{
			tension0 = node0.tension;
			tension1 = node1.tension;
		}
		else
		{
			tension0 = spline.tension;
			tension1 = spline.tension;
		}
		
		P2 = node1.customValue - P2;
		P3 = P3 - node0.customValue;
			
		P2 = P2 * tension0;
		P3 = P3 * tension1;
	}
	
	/// <summary>
	/// A function that returns the tangents for two spline nodes.
	/// </summary>
	/// <param name='P0'>
	/// Value of 1st spline node.
	/// </param>
	/// <param name='P1'>
	/// Value of 2nd spline node.
	/// </param>
	/// <param name='P2'>
	/// In: Value of 3rd node.
	/// Out: Tangent of node0.
	/// </param>
	/// <param name='P3'>
	/// In: Value of 4th node.
	/// Out: Tangent of node1.
	/// </param>
	public void RecalcScalars( float P0, float P1, ref float P2, ref float P3 )
	{
		float tension = 0.5f;
		
		P2 = P1 - P2;
		P3 = P3 - P0;
			
		P2 = P2 * tension;
		P3 = P3 * tension;
	}
	
	/// <summary>
	/// A function that returns the intermediate rotations for two spline nodes
	/// </summary>
	/// <param name='Q0'>
	/// The rotation of the 1st spline node.
	/// </param>
	/// <param name='Q1'>
	/// The rotation of the 2nd spline node.
	/// </param>
	/// <param name='Q2'>
	/// In: Rotation of the 3rd spline node.
	/// Out: intermediate rotation of node0.
	/// </param>
	/// <param name='Q3'>
	/// In: Rotation of the 4th spline node.
	/// Out: intermediate rotation of node1.
	/// </param>
	public void RecalcRotations( Quaternion Q0, Quaternion Q1, ref Quaternion Q2, ref Quaternion Q3 )
	{
		//Recalc rotations
		Q2 = QuaternionUtils.GetSquadIntermediate( Q0, Q1, Q2 );
		Q3 = QuaternionUtils.GetSquadIntermediate( Q1, Q2, Q3 );
	}
}

/// <summary>
/// A Bezier Interpolator.
/// </summary>
public class BezierInterpolator : SplineInterpolator
{
	/// <summary>
	/// Initializes a new instance of the <see cref="BezierInterpolator"/> class.
	/// </summary>
	public BezierInterpolator( )
	{
		CoefficientMatrix = new double[] {
			-1.0,  3.0, -3.0,  1.0,
			 3.0, -6.0,  3.0,  0.0,
			-3.0,  3.0,  0.0,  0.0,
			 1.0,  0.0,  0.0,  0.0
		};
		
		NodeIndices = new int[] { 0, 1, 2, 3 };
	}
}

/// <summary>
/// A B-spline Interpolator.
/// </summary>
public class BSplineInterpolator : SplineInterpolator
{
	/// <summary>
	/// Initializes a new instance of the <see cref="BSplineInterpolator"/> class.
	/// </summary>
	public BSplineInterpolator( )
	{
		CoefficientMatrix = new double[] {
			-1.0/6.0,   3.0/6.0, - 3.0/6.0,  1.0/6.0,
			 3.0/6.0, - 6.0/6.0,   0.0/6.0,  4.0/6.0,
			-3.0/6.0,   3.0/6.0,   3.0/6.0,  1.0/6.0,
			 1.0/6.0,   0.0/6.0,   0.0/6.0,  0.0/6.0
		};
		
		NodeIndices = new int[] { -1, 0, 1, 2 };
	}
}

/// <summary>
/// A Linear Interpolator.
/// </summary>
public class LinearInterpolator : SplineInterpolator
{
	/// <summary>
	/// Initializes a new instance of the <see cref="LinearInterpolator"/> class.
	/// </summary>
	public LinearInterpolator( )
	{
		CoefficientMatrix = new double[] {
			0.0,   0.0, - 1.0,  1.0,
			0.0,   0.0,   1.0,  0.0,
			0.0,   0.0,   0.0,  0.0,
			0.0,   0.0,   0.0,  0.0
		};
		
		NodeIndices = new int[] { 0, 1, 2, 3 };
	}
}

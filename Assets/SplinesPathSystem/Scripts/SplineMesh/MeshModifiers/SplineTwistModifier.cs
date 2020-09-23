using UnityEngine;
using System.Collections;

/// <summary>
/// Twists a SplineMesh around the spline's local tangents.
/// </summary>
[AddComponentMenu("SuperSplines/Other/Spline Mesh Modifiers/Twist Modifier")]
public class SplineTwistModifier : SplineMeshModifier 
{
	public float twistCount = 10f;		///< Number of twists in the whole spline mesh
	public float twistOffset = 0f;		///< The twists' offset related to the spline's start position
	
	private Quaternion rotationQuaternion;
	
	public override Vector3 ModifyVertex( SplineMesh splineMesh, Vector3 vertex, float splineParam )
	{
		//In order to avoid redundant calculations in ModifyNormal( ) and ModifyTangent( ), we store the
		//generated quaternion in a lokal variable.
		//This is possible, because ModifyVertex( ) is called before all other Modify**( )-functions.
		rotationQuaternion = Quaternion.Euler( Vector3.forward * (splineParam-twistOffset) * 360f * twistCount );
		
		return rotationQuaternion * vertex;
	}
	
	public override Vector2 ModifyUV( SplineMesh splineMesh, Vector2 uvCoord, float splineParam )
	{
		return uvCoord;
	}
	
	public override Vector3 ModifyNormal( SplineMesh splineMesh, Vector3 normal, float splineParam )
	{
		return rotationQuaternion * normal;
		//return Quaternion.Euler( Vector3.forward * (splineParam-twistOffset) * 360f * twistCount ) * normal;
	}
	
	public override Vector4 ModifyTangent( SplineMesh splineMesh, Vector4 tangent, float splineParam )
	{
		return rotationQuaternion * tangent;
		//return Quaternion.Euler( Vector3.forward * (splineParam-twistOffset) * 360f * twistCount ) * tangent;
	}
}

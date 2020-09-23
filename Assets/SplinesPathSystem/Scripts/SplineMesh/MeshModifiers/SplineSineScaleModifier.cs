using UnityEngine;
using System.Collections;

/// <summary>
/// Periodically alters a mesh's local scale around the spline.
/// </summary>
[AddComponentMenu("SuperSplines/Other/Spline Mesh Modifiers/Sine Scale Modifier (scale periodically)")]
public class SplineSineScaleModifier : SplineMeshModifier 
{
	public float frequency = 10f;			///< Frequency of the sine function
	public float offset = 0f;				///< X offset of the sine function
	
	public float sinMultiplicator = 1f;		///< Y scale of the sine function
	public float sinOffset = .25f;			///< Y offset of the sine function
	
	public override Vector3 ModifyVertex( SplineMesh splineMesh, Vector3 vertex, float splineParam )
	{
		return vertex * (Mathf.Pow( Mathf.Sin( splineParam * frequency + offset ), 2 ) * sinMultiplicator + sinOffset);
	}
	
	public override Vector2 ModifyUV( SplineMesh splineMesh, Vector2 uvCoord, float splineParam )
	{
		return uvCoord;
	}
	
	public override Vector3 ModifyNormal( SplineMesh splineMesh, Vector3 normal, float splineParam )
	{
		return normal;
	}
	
	public override Vector4 ModifyTangent( SplineMesh splineMesh, Vector4 tangent, float splineParam )
	{
		return tangent;
	}
}

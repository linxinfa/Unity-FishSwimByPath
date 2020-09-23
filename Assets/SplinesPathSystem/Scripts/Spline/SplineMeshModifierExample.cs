using UnityEngine;
using System.Collections;

/* This script does nothing special and is only meant for explaining the SplineMeshModifier-classes.
 * -------------------------------------------------------------------------------------------------
 * 
 * In order to give you more control over the mesh generation process of the SplineMesh-class, we
 * decided to add something like a vertex shader system to the mesh generator. Before the generated
 * vertices are stored in the mesh's vertex array, they will be passed to all scripts that derive 
 * from the SplineMeshModifier-class and are attached to the SplineMesh's gameObject. This pprocess 
 * will also be performed for all normals, tangents, and UVs, if the base mesh provides these. 
 * 
 * Inside the Modify**()-methods you can alter the vertex, normal, tangent and UV data as you like.
 * The functions receive a reference to the SplineMesh-class that invokes them as well as a spline
 * parameter that corresponds to the vertex's position in the spline.
 * 
 * The Modify**()-methods will be executed for each vertex in this order:
 * 1. ModifyVertex( )
 * 2. ModifyNormal( )
 * 3. ModifyTangent( )
 * 4. ModifyUV( )
 * 
 * This is important if some of the functions share the same calculations. In order to improve 
 * performance you can store results of calculations locally inside your Modifier-class and reuse 
 * them later in one of the Modify**()-methods. Check the SplineTwistModifier-class: The quaternion
 * calculated in the ModifyVertex()-method is later reused in ModifyNormal() and ModifyTangent()
 * 
 * Every class that derives from SplineMeshModifier must implement all Modify**()-methods using the
 * override keyword!
 * You can use this class as template for your own Modifier-classes.
 * 
*/
[AddComponentMenu("SuperSplines/Other/Spline Mesh Modifiers/Mesh Modifier Template")]
public class SplineMeshModifierExample : SplineMeshModifier //SplineMesh modifiers must derive from SplineMeshModifier
{
	//use the override keyword to implement the abstract methods of the SplineMeshModifier-class
	public override Vector3 ModifyVertex( SplineMesh splineMesh, Vector3 vertex, float splineParam )
	{
		return vertex;
	}
	
	public override Vector3 ModifyNormal( SplineMesh splineMesh, Vector3 normal, float splineParam )
	{
		return normal;
	}
	
	public override Vector4 ModifyTangent( SplineMesh splineMesh, Vector4 tangent, float splineParam )
	{
		return tangent;
	}
	
	public override Vector2 ModifyUV( SplineMesh splineMesh, Vector2 uvCoord, float splineParam )
	{
		return uvCoord;
	}
}

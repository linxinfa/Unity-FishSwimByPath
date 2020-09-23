using UnityEngine;
using System.Collections;

/// <summary>
/// A base class for a mesh modifier.
/// </summary>
/// <remarks>
/// Mesh modifiers can be used to customize the output of the SplineMesh class.
/// They act like a preprocessor for vertex, normal, tangent and UV data: Before these values are rotated and transfored according to the spline's path,
/// the SplineMesh-class calls the corresponding functions of a SplineMeshModifier. You can then alter the geometry data in relation the position on the spline, etc.
/// 
/// In order to create a new mesh modifier, you need to write a new class that derives from SplineMeshModifier.
/// Inside the class's Modify**()-methods you can alter the vertex, normal, tangent and UV data as you like.
/// The functions receive a reference to the SplineMesh-class that invokes them as well as a spline
/// parameter that corresponds to the vertex's position in the spline.
/// 
/// The Modify**()-methods will be executed for each vertex in this order:
/// 1. ModifyVertex( )
/// 2. ModifyNormal( )
/// 3. ModifyTangent( )
/// 4. ModifyUV( )
/// 
/// This is important if some of the functions share the same calculations. In order to improve 
/// performance you can store results of calculations locally inside your Modifier-class and reuse 
/// them later in one of the Modify**()-methods. The SplineTwistModifier-class does so for example: The quaternion
/// calculated in the ModifyVertex()-method is later reused in ModifyNormal() and ModifyTangent()
/// 
/// Every class that derives from SplineMeshModifier must implement all Modify**()-methods using the
/// override keyword!
/// 
/// You can use this template class for implementing your own SplineMeshModifier classes:
/// 
/// <code>
/// 	public class SplineMeshModifierExample : SplineMeshModifier //SplineMesh modifiers must derive from SplineMeshModifier
/// 	{
/// 		//use the override keyword to implement the abstract methods of the SplineMeshModifier-class
/// 		public override Vector3 ModifyVertex( SplineMesh splineMesh, Vector3 vertex, float splineParam )
/// 		{
/// 			return vertex;
/// 		}
/// 		
/// 		public override Vector3 ModifyNormal( SplineMesh splineMesh, Vector3 normal, float splineParam )
/// 		{
/// 			return normal;
/// 		}
/// 		
/// 		public override Vector4 ModifyTangent( SplineMesh splineMesh, Vector4 tangent, float splineParam )
/// 		{
/// 			return tangent;
/// 		}
/// 		
/// 		public override Vector2 ModifyUV( SplineMesh splineMesh, Vector2 uvCoord, float splineParam )
/// 		{
/// 			return uvCoord;
/// 		}
/// 	}
/// </code>
/// 
/// </remarks>
public abstract class SplineMeshModifier : MonoBehaviour
{
	/// <summary>
	/// Modifies the vertex position.
	/// </summary>
	/// <returns>
	/// The modified  vertex position.
	/// </returns>
	/// <param name='splineMesh'>
	/// The SplineMesh-class that called this function.
	/// </param>
	/// <param name='vertex'>
	/// The vertex position.
	/// </param>
	/// <param name='splineParam'>
	/// The current spline parameter.
	/// </param>
	public abstract Vector3 ModifyVertex( SplineMesh splineMesh, Vector3 vertex, float splineParam );
	
	/// <summary>
	/// Modifies the UV coordinates of a vertex.
	/// </summary>
	/// <returns>
	/// The modified UV coordinates of a vertex.
	/// </returns>
	/// <param name='splineMesh'>
	/// The SplineMesh-class that called this function.
	/// </param>
	/// <param name='uvCoord'>
	/// The UV coordinates of a vertex.
	/// </param>
	/// <param name='splineParam'>
	/// The current spline parameter.
	/// </param>
	public abstract Vector2 ModifyUV( SplineMesh splineMesh, Vector2 uvCoord, float splineParam );
	
	/// <summary>
	/// Modifies the vertex normal.
	/// </summary>
	/// <returns>
	/// The modified normal.
	/// </returns>
	/// <param name='splineMesh'>
	/// The SplineMesh-class that called this function.
	/// </param>
	/// <param name='normal'>
	/// The vertex normal.
	/// </param>
	/// <param name='splineParam'>
	/// The current spline parameter.
	/// </param>
	public abstract Vector3 ModifyNormal( SplineMesh splineMesh, Vector3 normal, float splineParam );
	
	/// <summary>
	/// Modifies the vertex tangent.
	/// </summary>
	/// <returns>
	/// The modified tangent.
	/// </returns>
	/// <param name='splineMesh'>
	/// The SplineMesh-class that called this function.
	/// </param>
	/// <param name='tangent'>
	/// The vertex Tangent.
	/// </param>
	/// <param name='splineParam'>
	/// The current spline parameter.
	/// </param>
	public abstract Vector4 ModifyTangent( SplineMesh splineMesh, Vector4 tangent, float splineParam );
}


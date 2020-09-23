using UnityEngine;
using System.Collections;

[AddComponentMenu("")]
[RequireComponent(typeof(Spline))]
public class SplineAutoAlign : MonoBehaviour 
{
	public LayerMask raycastLayers = -1;
	
	public float offset = 0.1f; 
	
	public string[] ignoreTags;
	
	public Vector3 raycastDirection = Vector3.down; 
	
	public void AutoAlign( )
	{
		if( raycastDirection.x == 0f && raycastDirection.y == 0f && raycastDirection.z == 0f )
		{
			Debug.LogWarning( this.gameObject.name + ": The raycast direction is zero!", this.gameObject );
			return;
		}
		
		Spline spline = GetComponent<Spline>( );
		
		foreach( SplineNode item in spline.SplineNodes )
		{
			RaycastHit[] raycastHits = Physics.RaycastAll( item.Position, raycastDirection, Mathf.Infinity, raycastLayers );
			RaycastHit closestHit = new RaycastHit( );
			
			closestHit.distance = Mathf.Infinity;
			
			foreach( RaycastHit hit in raycastHits )
			{
				//ignore specific tags
				bool ignore = false;
				
				foreach( string ignoreTag in ignoreTags )
					if( hit.transform.tag == ignoreTag )
						ignore = true;
				
				if( ignore )
					continue;
				
				if( closestHit.distance > hit.distance )
					closestHit = hit;
				
			}
			
			if( closestHit.distance == Mathf.Infinity )
				continue;
			
			item.Position = closestHit.point - raycastDirection * offset;
		}
	}
}

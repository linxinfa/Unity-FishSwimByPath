using UnityEngine;

namespace QuaternionUtilities
{
	public static class QuaternionUtils
	{
		public static Quaternion Exponential( this Quaternion q )
		{
			return GetQuatExp( q );
		}
		
		public static Quaternion Logarithm( this Quaternion q )
		{
			return GetQuatLog( q );
		}
		
		public static Quaternion Conjugate( this Quaternion q )
		{
			return GetQuatConjugate( q );
		}
		
		public static Quaternion Negative( this Quaternion q )
		{
			return GetQuatNegative( q );
		}
		
		public static Quaternion Normalized( this Quaternion q )
		{	
			float magnitudeInv = 1f/(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
			
			Quaternion result = new Quaternion( q.x, q.y, q.z, q.w );
			
			result.x *= magnitudeInv; result.y *= magnitudeInv; 
			result.z *= magnitudeInv; result.w *= magnitudeInv;
			
			return result;
		}
		
		public static Quaternion GetSquadIntermediate( Quaternion q0, Quaternion q1, Quaternion q2 )
		{
			Quaternion q1Inv = GetQuatConjugate( q1 );
			
			Quaternion p0 = GetQuatLog( q1Inv * q0 );
			Quaternion p2 = GetQuatLog( q1Inv * q2 );
			
			Quaternion sum = new Quaternion( -0.25f * (p0.x + p2.x), -0.25f * (p0.y + p2.y), -0.25f * (p0.z + p2.z), -0.25f * (p0.w + p2.w) );
			
			return q1 * GetQuatExp( sum );
		}
		
		public static Quaternion GetQuatLog( Quaternion q )
		{
			Quaternion res = q;
			
			res.w = 0;
	
			if( Mathf.Abs( q.w ) < 1.0f )
			{
				float theta = Mathf.Acos( q.w );
				float sin_theta = Mathf.Sin( theta );
	
				if( Mathf.Abs( sin_theta ) > 0.0001f )
				{
					float coef = theta / sin_theta;
					res.x = q.x * coef;
					res.y = q.y * coef;
					res.z = q.z * coef;
				}
			}
	
			return res;
		}
		
		public static Quaternion GetQuatExp( Quaternion q )
		{
			Quaternion res = q;
	
			float fAngle = Mathf.Sqrt( q.x * q.x + q.y * q.y + q.z * q.z );
			float fSin = Mathf.Sin( fAngle );
	
			res.w = Mathf.Cos( fAngle );
	
			if( Mathf.Abs( fSin ) > 0.0001f )
			{
				float coef = fSin / fAngle;
				res.x = coef * q.x;
				res.y = coef * q.y;
				res.z = coef * q.z;
			}
	
			return res;
		}
		
		public static Quaternion GetQuatConjugate( Quaternion q )
		{
			return new Quaternion( -q.x, -q.y, -q.z, q.w );
		}
		
		public static Quaternion GetQuatNegative( Quaternion q )
		{
			return new Quaternion( -q.x, -q.y, -q.z, -q.w );
		}
	}
}
using UnityEngine;
using System.Collections;
using OpenNI;

/// <summary>
/// just a collection of useful static methods
/// </summary>
public class Utils : ScriptableObject {
	
	
	/// <summary>
	/// converts an openni matrix rotation to a unity quaternion 
	/// flipping z axis. 
	/// </summary>
	/// <param name="m">
	/// A <see cref="SkeletonJointOrientation"/>
	/// </param>
	/// <returns>
	/// A <see cref="Quaternion"/>
	/// </returns>
	public static Quaternion openNIMatrixToQuat(SkeletonJointOrientation m)
	{
		SkeletonJointOrientation n = new SkeletonJointOrientation();
		n.X1 = m.X1;
		n.X2 = m.X2;
		n.X3 = -m.X3;
		n.Y1 = m.Y1;
		n.Y2 = m.Y2;
		n.Y3 = -m.Y3;
		n.Z1 = -m.Z1;
		n.Z2 = -m.Z2;
		n.Z3 = m.Z3;
		
		return matrixToQuat(n);
	}
	
	
	/// <summary>
	/// convert a matrix rotation to a quaternion
	/// based on:  http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
	/// </summary>
	/// <param name="m">
	/// A <see cref="SkeletonJointOrientation"/>
	/// </param>
	/// <returns>
	/// A <see cref="Quaternion"/>
	/// </returns>
	public static Quaternion matrixToQuat(SkeletonJointOrientation m)
	{
		float tr = m.X1 + m.Y2 + m.Z3;
		float qw = 0f;
		float qx = 0f;
		float qy = 0f;
		float qz = 0f;
		
		if (tr > 0)
		{ 
			float S = Mathf.Sqrt(tr+1.0f) * 2f; // S=4*qw 
			qw = 0.25f * S;
		 	qx = (m.Y3 - m.Z2) / S; //(m21 - m12) / S;
		  	qy = (m.Z1 - m.X3) / S; //(m02 - m20) / S; 
		  	qz = (m.X2 - m.Y1) / S; //(m10 - m01) / S; 
		} else if ((m.X1 > m.Y2) && (m.X1 > m.Z3))//((m00 > m11)&(m00 > m22)) { 
		{
		 	 float S = Mathf.Sqrt(1.0f + m.X1 - m.Y2 - m.Z3) * 2f; //sqrt(1.0 + m00 - m11 - m22) * 2; // S=4*qx 
		 	 qw = (m.Y3 - m.Z2) / S;//(m21 - m12) / S;
		 	 qx = 0.25f * S;
		 	 qy = (m.Y1 + m.X2) / S;//(m01 + m10) / S; 
		 	 qz = (m.Z1 + m.X3) / S;//(m02 + m20) / S; 
		} else if (m.Y2 > m.Z3)//(m11 > m22)
		{ 
		  	float S = Mathf.Sqrt(1.0f + m.Y2 - m.X1 - m.Z3) * 2f;//sqrt(1.0 + m11 - m00 - m22) * 2; // S=4*qy
		  	qw = (m.Z1 - m.X3) / S;	//(m02 - m20) / S;
		  	qx = (m.Y1 + m.X2) / S;	//(m01 + m10) / S; 
		  	qy = 0.25f * S;
		  	qz = (m.Z2 + m.Y3) / S;	//(m12 + m21) / S; 
		} else
		{ 
		  	float S = Mathf.Sqrt(1.0f + m.Z3 - m.X1 - m.Y2) * 2f;//sqrt(1.0 + m22 - m00 - m11) * 2; // S=4*qz
		  	qw = (m.X2 - m.Y1) / S;//(m10 - m01) / S;
		 	qx = (m.Z1 + m.X3) / S;//(m02 + m20) / S;
		 	qy = (m.Z2 + m.Y3) / S;//(m12 + m21) / S;
		  	qz = 0.25f * S;
		}
		
		return new Quaternion(qx, qy, qz, qw);
	}
}

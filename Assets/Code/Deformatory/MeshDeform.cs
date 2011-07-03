using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// class containing both static methods for deforming meshes and
/// a mouse run test case when instantiated
/// </summary>
public class MeshDeform : MonoBehaviour {
	
	private Ray ray;
	private RaycastHit hit;
	public float radius = 1f;
	public float power = 10f;
	
	private MeshFilter colliderMesh;
	
//	private List<int> affectedVerts;
	
//	void Start()
//	{
//		affectedVerts = new List<int>();
//	}
	
	/// <summary>
	/// this is for standalone use when used on a gameobject with a mouse
	/// </summary>
	void Update ()
	{
		
		//while button down //TODO: need to change this to during contact // amount of contact
		if (Input.GetMouseButton(0))
		{
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit))
			{
				MeshFilter mf = hit.collider.GetComponent<MeshFilter>();
				if (mf != null)
				{
					// can't update every frame
					if (mf != colliderMesh)
					{
						UpdateCollider();
						colliderMesh = mf;
					}

					
					// get the contact point on the mesh in local space
					Vector3 point = mf.transform.InverseTransformPoint(hit.point);
					
					DeformMe(mf.mesh, point, power * Time.deltaTime, radius, false);
				}
			}
		}
		else
		{
			UpdateCollider();
		}
		
		// this is just to test the "smoothing operation"
		if (Input.GetMouseButton(1))
		{
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit))
			{
				MeshFilter mf = hit.collider.GetComponent<MeshFilter>();
				if (mf != null)
				{
					// SMOOTH IT
					Vector3[] verts = mf.mesh.vertices;
					int[] triangles = mf.mesh.triangles;
					Color[] colors = mf.mesh.colors;
					Vector3 point = mf.transform.InverseTransformPoint(hit.point);
					int pointIdx = GetNearestVertIdx(point, verts);
					List<int> affected = Adjacents(verts, triangles, pointIdx);
					affected.Add(pointIdx);
					
					Debug.Log("neighbors: " + affected.Count);
					foreach (int i in affected)
					{
						Debug.Log("changing color on vertex: " + i + " = " + mf.mesh.vertices[i]);
						colors[i] = Color.blue;
					}

					List<int> t = new List<int>();
					t.Add(pointIdx);
					//t.AddRange(Adjacents(verts, triangles, pointIdx));
					mf.mesh.vertices = SmoothAffectedVerts(verts,triangles,t);
					mf.mesh.colors = colors;
					mf.mesh.RecalculateNormals();
					mf.mesh.RecalculateBounds();
				}
			}
		}

			
	
	}
	
	
	/// <summary>
	/// deforms the mesh within a radius given a position and an amount
	/// pulls out along the average normal
	/// </summary>
	/// <param name="mesh">
	/// A <see cref="Mesh"/>
	/// </param>
	/// <param name="pos">
	/// A <see cref="Vector3"/>
	/// </param>
	/// <param name="amount">
	/// A <see cref="System.Single"/>
	/// </param>
	public static void DeformMe(Mesh mesh, Vector3 pos, float amount, float radius, Vector3 normal, bool pull)
	{
		Vector3[] verts = mesh.vertices;
		Color[] colors = mesh.colors;
		float radiusSqr = radius*radius;
		
		
		//deform in the direction of the normal
		for (int i=0; i < verts.Length; i++)
		{

			float distanceSqrd = (verts[i] - pos).sqrMagnitude;
			
			if (distanceSqrd < radiusSqr) //make sure within range
			{
				// from unity's example procedural mesh project
				// gaussian falloff
				float falloff = Mathf.Clamp01 (Mathf.Pow (360.0f, -Mathf.Pow (Mathf.Sqrt(distanceSqrd) / radius, 2.5f) - 0.01f));
				
				//move the vertex
				//Debug.Log("vert: " + verts[i] + ",normal: " + normals[i] + ",falloff: " + falloff + ",amount: " + amount + ", distance: " + distanceSqrd);
				//verts[i] += normal * falloff * amount; 
				if (pull)
				{
					verts[i] += normal * falloff * amount;
					colors[i] = Color.Lerp(colors[i], Color.white, falloff);
				}
				else
				{
					verts[i] -= normal * falloff * amount;
					colors[i] =  Color.Lerp(colors[i], Color.gray, falloff);
				}

				//affectedVerts.Add(i);
			}
		}
		
		//apply it
		mesh.colors = colors;
		mesh.vertices = verts;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}

	
	/// <summary>
	/// deforms the mesh within a radius given a position and an amount
	/// pulls out along the average normal
	/// </summary>
	/// <param name="mesh">
	/// A <see cref="Mesh"/>
	/// </param>
	/// <param name="pos">
	/// A <see cref="Vector3"/>
	/// </param>
	/// <param name="amount">
	/// A <see cref="System.Single"/>
	/// </param>
	public static void DeformMe(Mesh mesh, Vector3 pos, float amount, float radius, bool pull)
	{
		Vector3[] verts = mesh.vertices;
		Vector3[] normals = mesh.normals;
		
		//find an average normal for all vertices within range
		//otherwise it pulls them out at weird angles
		Vector3 avNormal = Vector3.zero;
		float radiusSqr = radius*radius;
		
		for (int i =0; i< verts.Length; i++)
		{
			float distanceSqrd = (verts[i] - pos).sqrMagnitude;
			if (distanceSqrd < radiusSqr) //make sure within range
			{
				float falloff = Mathf.Clamp01(1f - Mathf.Sqrt(distanceSqrd) / radius);
				avNormal += falloff * normals[i];
			}
		}
		
		avNormal = avNormal.normalized;
		
		DeformMe(mesh, pos, amount, radius, avNormal, pull);
	}
	
	
	/// <summary>
	/// updates the collider mesh - a costly operation in unity!
	/// </summary>
	private void UpdateCollider()
	{
		if (colliderMesh && colliderMesh.GetComponent<MeshCollider>())
		{
			colliderMesh.GetComponent<MeshCollider>().sharedMesh = null;
			colliderMesh.GetComponent<MeshCollider>().sharedMesh = colliderMesh.mesh;
			Debug.LogWarning("updated collidermesh");
		}
		colliderMesh = null;
	}
	
	/// <summary>
	/// does a basic averaging of the vertices specified in the affected list of vertex indices
	/// </summary>
	/// <param name="vertices">
	/// A <see cref="Vector3[]"/>
	/// </param>
	/// <param name="triangles">
	/// A <see cref="System.Int32[]"/>
	/// </param>
	/// <param name="affected">
	/// A <see cref="List<System.Int32>"/>
	/// </param>
	/// <returns>
	/// A <see cref="Vector3[]"/>
	/// </returns>
	public static Vector3[] SmoothAffectedVerts(Vector3[] vertices, int[] triangles, List<int> affected)
	{
		Vector3[] newverts = vertices;
		
		foreach (int i in affected)
		{
			//Debug.Log("smoothing around vertex: " + i + ": " + vertices[i]);
			List<int> adjacentIndexes = Adjacents(vertices, triangles, i);
			
			if (adjacentIndexes.Count > 0)
			{
				//Debug.Log("number of neighbors: " + adjacentIndexes.Count);
				Vector3 nv = new Vector3(0f,0f,0f);
				for (int j = 0; j < adjacentIndexes.Count; j++)
				{
					//Debug.Log("found neighbor: " + vertices[adjacentIndexes[j]]);
					nv = nv + vertices[adjacentIndexes[j]];
				}
				
				// just average em
				newverts[i] = nv / adjacentIndexes.Count;
				//Debug.Log("new vertex = " + newverts[i]);
			}
		}
		return newverts;
	}
			
	
	/// <summary>
	/// gets the immediate adjacent vertices of a vertex
	/// </summary>
	/// <param name="vertices">
	/// A <see cref="Vector3[]"/>
	/// </param>
	/// <param name="triangles">
	/// A <see cref="System.Int32[]"/>
	/// </param>
	/// <param name="vertexIdx">
	/// A <see cref="System.Int32"/>
	/// </param>
	/// <returns>
	/// A <see cref="List<System.Int32>"/>
	/// </returns>
	public static List<int> Adjacents(Vector3[] vertices, int[] triangles, int vertexIdx)
	{
		List<int> adjacents = new List<int>();
		List<int> faces = new List<int>();
		int faceCount = 0;
		
		
		int neighbor1 = 0;
		int neighbor2 = 0;
		bool found = false;
		
		for (int j = 0; j < triangles.Length; j = j+3)
		{
			if (faces.Contains(j) == false)
			{
				if (vertexIdx == triangles[j])
				{
					neighbor1 = triangles[j+1];
					neighbor2 = triangles[j+2];
					found = true;
				}
				else if (vertexIdx == triangles[j+1])
				{
					neighbor1 = triangles[j+1];
					neighbor2 = triangles[j+2];
					found = true;
				}
				else if (vertexIdx == triangles[j+2])
				{
					neighbor1 = triangles[j];
					neighbor2 = triangles[j+1];
					found = true;
				}
				
				faceCount++;
				
				if (found)
				{
					faces.Add(j);
					if (!adjacents.Contains(neighbor1))
					{
						adjacents.Add(neighbor1);
					}
					if (!adjacents.Contains(neighbor2))
					{
					    adjacents.Add(neighbor1);
					}
					found = false;
				}
			}
		}
		return adjacents;
	}

			
	/// <summary>
	/// gets the nearest vertex index to the point in local space given
	/// </summary>
	/// <param name="point">
	/// A <see cref="Vector3"/>
	/// </param>
	/// <param name="vertices">
	/// A <see cref="Vector3[]"/>
	/// </param>
	/// <returns>
	/// A <see cref="System.Int32"/>
	/// </returns>
	public static int GetNearestVertIdx(Vector3 point, Vector3[] vertices)
	{
		float distance = float.MaxValue;
		int minIdx = 0;

		for (int i = 0; i< vertices.Length; i++)
		{
			float ndistance = (point - vertices[i]).sqrMagnitude;
			if (ndistance < distance)
			{
				minIdx = i;
				distance = ndistance;
			}
		}
		return minIdx;
	}
							
	
}

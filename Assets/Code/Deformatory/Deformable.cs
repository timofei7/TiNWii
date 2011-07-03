using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// attach this to an object to make it deformable by a deformer
/// </summary>
public class Deformable : MonoBehaviour
{

	public Ray ray;
	private float bufferZone = .1f;

	private CentralDeformatory c;
	
	public bool attached = false;
	
	public MeshFilter colliderMesh;
	
	public float distanceSave = 0f;
	public RaycastHit lastHit;
	public float newDist = 0;
	public Collider lastDeformerCollider;
	public int lastVertexIdx;
	public Deformer lastDeformerDeformer;
	public MeshFilter mf;
	
	
	void Awake()
	{
		c = FindObjectOfType(typeof(CentralDeformatory)) as CentralDeformatory;
	}

	void Start()
	{
		lastHit = new RaycastHit();
		mf = GetComponent<MeshFilter>();
	}	
	
	void Update ()
	{				
		// checks the distance from the last hit point (tracked through its movement in the mesh)
		// with the position of the deformer
		if (lastHit.collider != null)
		{
			//Debug.LogWarning("comparing: " + lastHit.point + " vs " + lastCollider.transform.position);
			//newDist = Vector3.Distance(lastHit.point, lastCollider.transform.position);
			Vector3 pointInWorld = mf.transform.TransformPoint(mf.mesh.vertices[lastVertexIdx]);
			newDist = Vector3.Distance(pointInWorld, lastDeformerCollider.transform.position);
			StartCoroutine(DrawLine(pointInWorld, lastHit.point, Color.blue));
		}
			
		// if we've moved out of the bufferzone detach
		if (distanceSave > newDist+bufferZone || distanceSave < newDist-bufferZone)
		{
			//Debug.LogWarning("detaching: " + distanceSave + ":" + newDist);
			attached = false;
		}
		
		// if we're attached do some deformation
		if (attached)
		{
			// can't update collider all the time
			if (mf != colliderMesh)
			{
				UpdateCollider();
				colliderMesh = mf;
			}

			// get the contact point on the mesh in local space
			Vector3 point = mf.transform.InverseTransformPoint(lastHit.point);
			
			//averaged normals version
			MeshDeform.DeformMe(mf.mesh, point, lastDeformerDeformer.distance, lastDeformerDeformer.radius, !c.handDetector.open);			
			//giving it a direction  //all this does is swirls duh!
			//MeshDeform.DeformMe(mf.mesh, point, lastDeformerDeformer.distance, lastDeformerDeformer.GetComponent<SphereCollider>().radius, lastDeformerDeformer.direction, !c.handDetector.open);			
		}
		else
		{
			UpdateCollider();
		}
	}

	
	
	public void OnTriggerStay(Collider other)
	{
		//Debug.Log("OnTriggerStay");
		// these are reversed so this is from the perspective of the touching object/Deformer
		Vector3 source = other.collider.transform.position; // the touching object/Deformer
		GameObject target = this.gameObject; // ourselves as target
		lastDeformerDeformer = other.gameObject.GetComponent<Deformer>();
		lastDeformerCollider = other;
		
		RaycastHit hit = GetSurfacePoint(source, target, Color.red);
		if (!hit.point.Equals(null))
		{
			//keep the distance during collision so if we move out of distance release
			distanceSave = Vector3.Distance(hit.point, source);
			attached = true;
			lastHit = hit;
			lastVertexIdx = MeshDeform.GetNearestVertIdx(mf.transform.InverseTransformPoint(hit.point), mf.mesh.vertices);
//			Color[] clrs = mf.mesh.colors;
//			// set colors that we hit depending on pushing or pulling
//			if (c.handDetector.open)
//				clrs[lastVertexIdx] = Color.cyan;
//			else
//				clrs[lastVertexIdx] = Color.white;
//			mf.mesh.colors = clrs;
			//Debug.Log("attaching: " + hit.collider.name + ", distance: " + distanceSave);
		}
	}
		
	
	private RaycastHit GetSurfacePoint(Vector3 source, GameObject target, Color debugColor)
	{
		//Debug.Log("GetSurfacePoint");
		int layerMask = 1 << 10;
		RaycastHit ht = new RaycastHit(); 
		//Debug.Log("GetSurfacePoint from source: " + source + " to: " + target.name + "-" + target.gameObject.layer);
		//Vector3 targetPt = target.transform.position;
		
		//FOR POINT ON TARGET
		int layersave = target.layer;
		bool istriggerSave = target.collider.isTrigger;
		target.collider.isTrigger = false;
		target.layer = 10; //only look for target
		Vector3 direction = (target.transform.position - source).normalized;
		if (Physics.SphereCast(source, lastDeformerDeformer.radius, lastDeformerDeformer.direction, out ht, bufferZone*2, layerMask))
		//if (Physics.SphereCast(source, lastDeformerDeformer.gameObject.GetComponent<SphereCollider>().radius, lastDeformerDeformer.gameObject.transform.forward, out ht, 10f, layerMask))
		{
			//Debug.Log("hit: " + ht.collider.name + ", " + ht.point);
			StartCoroutine(DrawLine(source, ht.point, debugColor));
		}
		else
		{
			//Debug.LogError("collided but no raycasthit... :-( this should not happen");
			//Debug.LogError("target: " + target.name + " deformer: " + lastDeformerDeformer.name);
			Debug.DrawRay(source, direction * 10f, Color.blue);
		}
		target.layer = layersave;
		target.collider.isTrigger = istriggerSave;
		
		return ht;

	}

	
	/// <summary>
	/// helper coroutine to draw a line for a few seconds
	/// </summary>
	/// <param name="a">
	/// A <see cref="Vector3"/>
	/// </param>
	/// <param name="b">
	/// A <see cref="Vector3"/>
	/// </param>
	/// <param name="col">
	/// A <see cref="Color"/>
	/// </param>
	/// <returns>
	/// A <see cref="IEnumerator"/>
	/// </returns>
	private IEnumerator	DrawLine(Vector3 a, Vector3 b, Color col)
	{
		float c = 0f;
		while (c< 3f)
		{
			Debug.DrawLine(a, b, col);
			c += Time.deltaTime;
			yield return null;
		}
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
			//Debug.LogWarning("updated collidermesh");
		}
		colliderMesh = null;
	}
	

}	
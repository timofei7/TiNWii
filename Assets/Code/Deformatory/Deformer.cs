using UnityEngine;
using System.Collections;

/// <summary>
/// simple class that calculates and keeps a trajectory variable uptodate
/// </summary>
/// 
public class Deformer : MonoBehaviour {
	
	public Vector3 direction;	
	private Vector3 previousPos;
	
	public float speed;
	public float distance;
	private CentralDeformatory c;
	public float radius;
	//private Color colorEmission;
	private Color colorMain;
		
	void Start ()
	{
		previousPos = transform.position;
		speed = 0f;
		rigidbody.inertiaTensor = new Vector3(1f,1f,1f);
		radius = Vector3.Distance(transform.TransformPoint(gameObject.GetComponent<MeshFilter>().mesh.vertices[0]), transform.position);
		//colorEmission = renderer.material.GetColor("_Emission");
		colorMain = renderer.material.GetColor("_TintColor");
	}
	
	
	void Update()
	{
		distance = Vector3.Distance(transform.position, previousPos);
		direction = (previousPos - transform.position).normalized;
		speed = Mathf.Lerp(speed, distance / Time.deltaTime, .1f);
		previousPos = transform.position;
		float lerp = Mathf.PingPong(Time.time, .5f);
		//renderer.material.SetColor("_Emission", Color.Lerp(colorEmission, colorMain, lerp));
		renderer.material.SetColor("_TintColor", Color.Lerp(colorMain, Color.red, lerp));
	}
	
	
	/// <summary>
	/// hack to use collisions if the objects are both non-kinematic rigidbodies 
	/// </summary>
	/// <param name="col">
	/// A <see cref="Collision"/>
	/// </param>
	void OnCollisionStay(Collision col)
	{
		ContactPoint c = col.contacts[0];
		Deformable d =c.otherCollider.GetComponent<Deformable>();
		d.OnTriggerStay(c.thisCollider);
	}
	
}

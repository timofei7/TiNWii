using UnityEngine;
using System.Collections;

public class BallAnimator : MonoBehaviour {

	public Transform rightHand;
	public Transform leftHand;
	public Transform floor;
	
	private bool animate = false;
	
	public Transform target;
	public Transform previous;
	
	
	void Awake()
	{
		target = floor;
		previous = rightHand;
	}
	
	
	void Update()
	{
		if (animate)
		{
			transform.position =  Vector3.Lerp(transform.position, target.position, Time.deltaTime * 10f);
		}
	}
		
	
	void OnTriggerEnter(Collider other)
	{	
		//Debug.LogWarning("TRIGGER ENTER:" + other.gameObject.name);
		if (other.gameObject.transform.Equals(rightHand))
		{
			//Debug.Log("targeting floor");
			target = floor;
			Vector3 tpos = SizeShape.GetMidpt(rightHand.gameObject, leftHand.gameObject);
			target.position = new Vector3(tpos.x, target.position.y, tpos.z);
			previous = rightHand;
		} 
		else if (other.gameObject.transform.Equals(leftHand))
		{
			//Debug.Log("targeting floor");
			target = floor;
			Vector3 tpos = SizeShape.GetMidpt(rightHand.gameObject, leftHand.gameObject);
			target.position = new Vector3(tpos.x, target.position.y, tpos.z);
			previous = leftHand;
		}
		else if (other.gameObject.transform.Equals(floor))
		{
			if (previous.Equals(rightHand))
			{
				target = leftHand;
			}
			else
			{
				target = rightHand;
			}
			previous = floor;
		}
	}
	
	
	public void Start()
	{
		rigidbody.isKinematic = true;
		animate = true;
		collider.isTrigger = true;
	}
	
	public void Stop()
	{
		animate=false;
		rigidbody.isKinematic = false;
		collider.isTrigger = false;
	}

}

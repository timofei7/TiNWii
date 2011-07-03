using UnityEngine;
using System.Collections;

/// <summary>
/// allows an object to be "carried" with two hands
/// object needs to have a collider and rigidbody
/// </summary>
public class Carryable : MonoBehaviour {
	
	
	[HideInInspector]
	public Transform hand1;
	[HideInInspector]
	public Transform hand2;
	
	private Vector3 currVelocity = Vector3.zero;
	
	
	void Update () {
		if (hand1 && hand2)
		{
			Vector3 npos = Vector3.Lerp(hand1.position, hand2.position, .5f);
			transform.position = Vector3.SmoothDamp(transform.position, npos, ref currVelocity, .15f);
		}
	}
		
	
	void OnTriggerEnter(Collider hit)
	{
		Debug.Log("Trigger in");
		if(hit.transform.name.ToLower().Contains("hand"))
		{
			Debug.Log("hand in:" + hit.gameObject.name);
			if (hand1)
			{
				hand2 = hit.transform;
			}
			else
			{
				hand1 = hit.transform;
			}
			
		}
	}
	void OnTriggerExit(Collider hit)
	{
		Debug.Log("Trigger out");
		if(hit.transform.name.ToLower().Contains("hand"))
		{
			Debug.Log("hand out:" + hit.gameObject.name);	
			if (hit.transform == hand1)
			{
				hand1 = null;
				Debug.Log("Hand1 OUT");
			}	
			if (hit.transform == hand2)
			{
				hand2 = null;
				Debug.Log("Hand2 OUT");
			}	
		}
	}
}

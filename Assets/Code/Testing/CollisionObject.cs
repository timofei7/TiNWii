using UnityEngine;
using System.Collections;

public class CollisionObject : MonoBehaviour
{
	
	
	void OnTriggerStay(Collider other)
	{
		Debug.Log("OnTriggerStay");
		Vector3 direction = (other.collider.transform.position - transform.position).normalized;
		//StartCoroutine(DrawLine(transform.position, other.collider.transform.position, Color.red));
		RaycastHit hit; 
		//other.Raycast(new Ray(transform.position, movementThisStep), out hit, 100f);
		//if (Physics.Raycast(transform.position, other.collider.transform.position, out hit, 100f))
		if (Physics.SphereCast(transform.position, Vector3.Distance(transform.position, other.collider.transform.position) + 1f, direction, out hit))
		{
			Debug.Log("TriggerStay: " + other.name + ", " + hit.point + ", " + hit.collider.name);
			StartCoroutine(DrawLine(transform.position, hit.point, Color.blue));
		}
		else
		{
			Debug.Log("collided but no raycasthit... :-(");
		}
	}
	
	
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
	
	
}

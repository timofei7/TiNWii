using UnityEngine;
using System.Collections;

public class ReMesh : MonoBehaviour {
	
	
	public GameObject collid;
	int i = 0;
	
	void Start()
	{
		StartCoroutine(FillUp());
	}
	
	
	IEnumerator FillUp()
	{
		Vector3 op = gameObject.transform.position;
		float sign = -1;
		while (i < 2000)
		{
			sign = sign * -1;
			float f = Random.Range(0, 100) / 75f * sign;
			Vector3 pos = new Vector3(op.x+f, op.y+f, op.z-f);
			GameObject foo = (GameObject) Instantiate(collid,pos, Quaternion.identity);
			if (Mathf.Approximately(sign, -1))
			    foo.renderer.enabled = false;
			i++;
			yield return 0;
		}
		Debug.Log("DONE");

	}
	
}

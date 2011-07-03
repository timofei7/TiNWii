using UnityEngine;
using System.Collections;

/// <summary>
/// @author Divya Gunasekaran
/// </summary>
public class SelectShape : MonoBehaviour {
	
	public GameObject shape;
	public GameObject handLeft;
	public GameObject handRight;
	public PhysicMaterial physicsMaterial;
	
	public float differenceTheshold;
	public float timeoutThreshold;
	
	
	private CentralPhysics c;
	
	void Awake ()
	{
		c = FindObjectOfType(typeof(CentralPhysics)) as CentralPhysics;
	}
	
	
	//Create GameObject Primitive according to the selected type
	public void CreateShape(int shapeType){
		c.nite.HUD.text = "Select a shape and give it a size!";
		
		switch (shapeType) {
		//Sphere
		case 1:
			shape = GameObject.CreatePrimitive(PrimitiveType.Sphere);	
		break;
		
		//Cube
		case 2:
			shape = GameObject.CreatePrimitive(PrimitiveType.Cube);	
		break;
			
		//Cylinder
		case 3:
			shape = GameObject.CreatePrimitive(PrimitiveType.Cylinder);	
		break;
		
		//Default is a sphere
		default:
			shape = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		break;
		}
		
		//Set default position to origin
		//Set default size
		//Not sure if setting defaults is necessary -- size and position will be
		//determined by hand positions (see SizeShape.cs script)
		//shape.transform.position = new Vector3(0,0,0);
		//shape.transform.localScale = new Vector3(1,1,1);
		
		//Add SizeShape script and RigidBody to object
		shape.AddComponent("SizeShape");
		shape.AddComponent("Rigidbody");
		shape.collider.material = physicsMaterial;
		shape.rigidbody.isKinematic = true;
		
		//Get reference to SizeShape script
		SizeShape sizeScript = shape.GetComponent<SizeShape>();
		
		//Initialize public variables in SizeShape script
		sizeScript.setup(differenceTheshold, timeoutThreshold, handLeft, handRight);
		sizeScript.selected = true;
		sizeScript.SizeNotifierEvent += HandleSizeNotifierEvent;
		
	}
	
	void HandleSizeNotifierEvent (System.Object o, System.EventArgs e)
	{
		//c.NextState();
		//TODO: do the right thing when done sizing
	}
	
	
	
}

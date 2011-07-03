using UnityEngine;
using System.Collections;

public class UserGuiDeformatory : MonoBehaviour {

	private CentralDeformatory c;
	
	public Rect buttonOne;
	public Rect buttonTwo;
	public Rect buttonThree;
	
	void Awake ()
	{
		c = FindObjectOfType(typeof(CentralDeformatory)) as CentralDeformatory;
		buttonOne = new Rect(20,40,100,100);
		buttonTwo = new Rect (20,160,100,100);
		buttonThree = new Rect(20,280,100,100);

	}
	
	void OnGUI ()
	{
		if (c.state == CentralDeformatory.State.Setup) this.ShapeButtons();
		//if (c.nite.needsCalibration) c.nite.CalibratingGui();
		c.nite.CalibratingGui(); // just display it always now
		if (c.handDetector.handExists) c.handDetector.Gui(); 
	}
	
	
	
	//Select shape to create -- currently implemented as a GUI with buttons, but will be controlled by gestures
	void ShapeButtons () {
		//Background box
		GUI.Box(new Rect(10,10,120,380), "Select a Shape");

		//Button to create sphere
		//Calls CreateShape function
		if (GUI.Button(buttonOne, "Sphere")) {
			//c.selectShape.CreateShape(1);
		}	

		//Button to create cube
		//Calls CreateShape function
		if (GUI.Button(buttonTwo, "Cube")) {
			//c.selectShape.CreateShape(2);
		}
		
		//Button to create cylinder
		//Calls CreateShape function
		if (GUI.Button(buttonThree, "Cylinder")) {
			//c.selectShape.CreateShape(3);
		}
	}

}

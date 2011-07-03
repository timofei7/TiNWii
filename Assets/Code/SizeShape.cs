using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// @author Divya Gunasekaran
/// </summary>
public class SizeShape : MonoBehaviour {
	
	//Initialized in SelectShape script
	public GameObject handLeft;
	public GameObject handRight;
	public bool selected = false;
	
	
	//Constants
	private float TIMEOUT = 3f; //Length of time shape needs to be stationary
	  								 //for sizing process to terminate
	private float THRESHOLD = .01f; //Distance each hand needs to be within
	 									  //with respect to its previous pos for
										  //the shape to be considered stationary
	
	//Set in Update function
	private float sizeTimeCount = 0;
	private Vector3 leftPosition;
	private Vector3 rightPosition;
	private Vector3 finalPos;
	private float finalSize;
	
	public event EventHandler SizeNotifierEvent;
	
	public void setup(float threshold, float timeout, GameObject handLeft, GameObject handRight)
	{
		this.TIMEOUT = timeout;
		this.THRESHOLD = threshold;
		this.handLeft = handLeft;
		this.handRight = handRight;
	}
	
	// Update is called once per frame
	void Update () {
		//If shape has been created and sizing process is still in progress
		if(selected && (sizeTimeCount < TIMEOUT)){

			//Get positions of left and right hands
			Vector3 newLeftPos = handLeft.transform.position;
			Vector3 newRightPos = handRight.transform.position;
			
			//Get distance between two positions
			float dist = GetDist(newLeftPos, newRightPos);
			
			//Get midpoint between two positions
			Vector3 midpt = GetMidpt(handLeft, handRight);
		
			//Set position and size of shape
			transform.position = new Vector3(midpt.x, midpt.y, midpt.z);
			transform.localScale = new Vector3(dist, dist, dist);
			
			//Get changes in left and right hand positions, respectively
			float deltaLeft = GetDist(newLeftPos, leftPosition);
			float deltaRight = GetDist(newRightPos, rightPosition);
			
			//Increment counter if left and right hand positions relatively
			//stable; else reset counter
			if((deltaLeft < THRESHOLD) && (deltaRight < THRESHOLD)){
				sizeTimeCount = sizeTimeCount + Time.deltaTime;
			}
			else{
				sizeTimeCount = 0;
			}
				
			//Update public variables
			leftPosition = newLeftPos;
			rightPosition = newRightPos;
			finalPos = midpt;
			finalSize = dist;
		}
		else
		{
			//Set position and size of shape
			transform.position = new Vector3(finalPos.x, finalPos.y, finalPos.z);
			transform.localScale = new Vector3(finalSize, finalSize, finalSize);
			
			//FOR TESTING
			Debug.LogWarning("Sizing done!"); 
			
			//notify listeners
			if (SizeNotifierEvent != null)
       		{
                SizeNotifierEvent(this, EventArgs.Empty);
        	}

			Destroy(this);
		}
			
	}
	
	
	//Function to calculate Euclidean distance between two points in 3D space
	public static float GetDist(Vector3 first, Vector3 second){	
		
		//Get coordinates of first and second GameObjects
		float firstX = first.x;
		float firstY = first.y;
		float firstZ = first.z;
		
		float secondX = second.x;
		float secondY = second.y;
		float secondZ = second.z;
		
		//Calculate Euclidean distance between first and second positions
		float distSq = Mathf.Pow((firstX - secondX), 2) + Mathf.Pow((firstY - secondY), 2) + Mathf.Pow((firstZ - secondZ), 2); 
		float newDist = Mathf.Sqrt(distSq);
		
		return newDist;
	}
	
	//Function to calculate midpoint between two GameObjects in 3D space
	public static Vector3 GetMidpt(GameObject first, GameObject second){
		
		//Get coordinates of first and second GameObjects
		float firstX = first.transform.position.x;
		float firstY = first.transform.position.y;
		float firstZ = first.transform.position.z;
		
		float secondX = second.transform.position.x;
		float secondY = second.transform.position.y;
		float secondZ = second.transform.position.z;
		
		//Calculate midpoint
		float midX = (firstX + secondX) / 2;
		float midY = (firstY + secondY) / 2;
		float midZ = (firstZ + secondZ) / 2;
		
		Vector3 midpt = new Vector3(midX, midY, midZ);
		
		return midpt;
	}
				   
				   
	
}

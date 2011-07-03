using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NITE;

public class Bouncer : MonoBehaviour {

	private CentralPhysics c;
	public Transform rootJoint;
	public enum State { Prompt, Bounces, DetectDropAndBounce, DropHeight, BounceHeight, PlayBack } 
	public State state;
	public bool bouncing = false;
	
	private float cMax=.5f;
	private float counter = 0;
	private bool swipeLeft = false;
	private bool swipeRight = false;
	private bool steadyEvent = false;

	private float jumpCounter = 0f;
	private float jumpMax = 5f;
	private float dropHeight = 0f;
	private float bounceHeight = 0f;
	private float currentHandY = 0f;
	private float coeffRestitution = 0f;
	
	public Transform rightHand;
	public Transform leftHand;
	
	public GameObject testBall;
		
	public event System.EventHandler BouncerNotifierEvent;

	void Awake ()
	{
		c = FindObjectOfType(typeof(CentralPhysics)) as CentralPhysics;
	}
	
	
	void Update ()
	{
		if (counter < cMax)
			counter += Time.deltaTime;
		else {
			//Debug.Log(rootJoint.position.y);
			counter = 0f;
		}
	}
	
	public void StartBounceDetector()
	{
		ResetEvents();
		c.nite.steadyDetector = new SteadyDetector();
		c.nite.pointDenoiser.AddListener(c.nite.steadyDetector);
		c.nite.steadyDetector.Steady += HandleSteadyDetectorSteady;
		c.nite.swipeDetector.SwipeLeft += HandleSwipeDetectorSwipeLeft;
		c.nite.swipeDetector.SwipeRight += HandleSwipeDetectorSwipeRight;
		bouncing = true;
		state = Bouncer.State.Prompt;
		NextState();
	}

	public void StopBounceDetector()
	{
		c.nite.steadyDetector.Steady -= HandleSteadyDetectorSteady;
		c.nite.swipeDetector.SwipeLeft -= HandleSwipeDetectorSwipeLeft;
		c.nite.swipeDetector.SwipeRight -= HandleSwipeDetectorSwipeRight;

		bouncing = false;
		StopAllCoroutines();
	}
	
	private IEnumerator PromptState()
	{
		ResetEvents();
		c.nite.HUD.text = "Swipe your arm left when you are ready to set the bounciness!"; 
		while (state == Bouncer.State.Prompt  && !swipeLeft)
		{
			yield return new WaitForFixedUpdate();
		}
		state = Bouncer.State.DetectDropAndBounce;
		NextState();
	}
	
	/// <summary>
	/// this detects coefficient of restitution from bouncing the whole body
	/// </summary>
	/// <returns>
	/// A <see cref="IEnumerator"/>
	/// </returns>
//	private IEnumerator BouncesState()
//	{
//		jumpMax = 10f;
//		List<Vector2> heights = new List<Vector2>();
//		c.nite.HUD.text = "Jump to show bounciness! you have 10 seconds";
//		yield return new WaitForSeconds(2f);
//		bool up = false;
//		bool min = false;
//		bool max = false;
//		float curr = rootJoint.position.y;
//		float prev = curr;
//		float minV = float.MaxValue;
//		float maxV = float.MinValue;
//		int numJumps = 0;
//		while (state == Bouncer.State.Bounces && (jumpCounter < jumpMax && numJumps < 3))
//		{
//			curr = Mathf.Round(rootJoint.position.y*10)/10f;  //lose precision
//			curr = Mathf.Lerp(prev, curr, .9f); // average with previous a little bit
//			curr = Mathf.Round(curr*10)/10f; //lose more precision bitches
//			//Debug.LogWarning("smoothing from: " + rootJoint.position.y + " to " + curr);
//			
//			if (min && max)
//			{
//				Vector2 pair = new Vector2(minV, maxV);
//				heights.Add(pair);
//				numJumps++;
//				min = false;
//				max = false;
//			} 
//			else
//			{
//				if (curr > prev && up)
//				{
//					//Debug.Log("still up: " + curr + "," + prev + ": " + up);
//					maxV = curr;
//				}
//				else if (curr < prev && !up)
//				{
//					//Debug.Log("still down: " + curr + "," + prev + ": " + up);
//					minV = curr;
//				}
//				else if (curr < prev && up)
//				{
//					Debug.LogError("changing to down: " + curr + "," + prev + ": " + up);
//					c.nite.HUD.text += "\n down detected "+ curr + "," + prev;
//					max = true;
//					up = false;
//				}
//				else if (curr > prev && !up)
//				{
//					c.nite.HUD.text += "\n up detected "+ curr + "," + prev;
//					Debug.LogError("changing to up: " + curr + "," + prev + ": " + up);
//					min = true;
//					up = true;
//				}
//			}
//			jumpCounter += Time.fixedDeltaTime; //since we're running in physics time here with the yield below
//			prev = curr;
//			yield return new WaitForFixedUpdate();
//		}
//		jumpCounter = 0f;
//		c.nite.HUD.text = "bounce configuration completed: ";
//		foreach (Vector2 v in heights)
//		{	
//			Debug.LogError("Detected jump: " + v.x +"," + v.y);
//			c.nite.HUD.text += "\n\t" + v.x +"," + v.y;
//		}
//		float a = heights[1].y - heights[1].x;
//		float b = heights[2].y - heights[2].x;
//		coeffRestitution = Mathf.Sqrt(Mathf.Min(a,b) / Mathf.Max(a, b));
//		Debug.LogWarning("Cr = " + coeffRestitution);
//		testBall.collider.material.bounciness = coeffRestitution;
//		//c.selectShape.shape.collider.material.bounciness = coeffRestitution;
//		yield return new WaitForSeconds(3f);
//		c.nite.HUD.text += "\ncoefficient of restitution = " + coeffRestitution;
//		yield return new WaitForSeconds(3f);
//		state = Bouncer.State.PlayBack;
//		NextState();
//	}
	
	/// <summary>
	/// detect drop and bounce in one state with two hands
	/// </summary>
	/// <returns>
	/// A <see cref="IEnumerator"/>
	/// </returns>
	private IEnumerator DetectDropAndBounceState()
	{
		c.ballAnimator.Start();
		c.nite.HUD.text = "Show the height that the object is DROPPING FROM with one hand and the FIRST BOUNCE HEIGHT with the other!";
		yield return new WaitForSeconds(3f);
		ResetEvents();
		while (state == Bouncer.State.DropHeight && !steadyEvent)
		{
			
			yield return new WaitForFixedUpdate();
		}
		dropHeight = Mathf.Max(rightHand.transform.position.y, leftHand.transform.position.y);
		bounceHeight = Mathf.Min(rightHand.transform.position.y, leftHand.transform.position.y);
		c.nite.HUD.text += "\n Detected drop height: " + dropHeight;
		c.nite.HUD.text += "\n Detected bounce height: " + bounceHeight;
		coeffRestitution = Mathf.Sqrt(bounceHeight /dropHeight);
		testBall.collider.material.bounciness = coeffRestitution;
		c.nite.HUD.text += "\n Cr = " + coeffRestitution;
		yield return new WaitForSeconds(3f);
		c.ballAnimator.Stop();
		state = Bouncer.State.PlayBack;
		NextState();
	}
	
//	private IEnumerator DropHeightState()
//	{
//		c.ballAnimator.Start();
//		c.nite.HUD.text = "Show the height that the object is DROPPING from with your hand!";
//		yield return new WaitForSeconds(2f);
//		c.nite.steadyDetector.Steady += HandleSteadyDetectorSteady;
//		c.nite.steadyDetector.PrimaryPointUpdate += HandleSteadyDetectorPrimaryPointUpdate;
//		while (state == Bouncer.State.DropHeight && !steadyEvent)
//		{
//			
//			yield return new WaitForFixedUpdate();
//		}
//		steadyEvent = false;
//		dropHeight = currentHandY; //grab the current height of the primary hand... in wacko coords but doesn't matter
//		c.nite.HUD.text += "\n Detected drop height: " + dropHeight;
//		yield return new WaitForSeconds(2f);
//		state = Bouncer.State.BounceHeight;
//		c.nite.steadyDetector.Steady -= HandleSteadyDetectorSteady;
//		c.nite.steadyDetector.PrimaryPointUpdate -= HandleSteadyDetectorPrimaryPointUpdate;
//		NextState();
//	}
//	
//	private IEnumerator BounceHeightState()
//	{
//		c.nite.HUD.text = "Show the height that the object is bouncing back up to with your hand!";
//		yield return new WaitForSeconds(2f);
//		c.nite.steadyDetector.Steady += HandleSteadyDetectorSteady;
//		c.nite.steadyDetector.PrimaryPointUpdate += HandleSteadyDetectorPrimaryPointUpdate;
//		while (state == Bouncer.State.DropHeight && !steadyEvent)
//		{
//			yield return new WaitForFixedUpdate();
//		}
//		steadyEvent = false;
//		bounceHeight = currentHandY; //grab it
//		coeffRestitution = Mathf.Sqrt(Mathf.Min(dropHeight,bounceHeight) / Mathf.Max(dropHeight,bounceHeight));
//		c.nite.HUD.text += "\n Detected bounce height: " + bounceHeight;
//		c.nite.HUD.text += "\n Cr = " + coeffRestitution;
//		Debug.LogWarning("Cr = " + coeffRestitution);
//		testBall.collider.material.bounciness = coeffRestitution;
//		yield return new WaitForSeconds(2f);
//		state = Bouncer.State.PlayBack;
//		c.ballAnimator.Stop();
//		c.nite.steadyDetector.Steady -= HandleSteadyDetectorSteady;
//		c.nite.steadyDetector.PrimaryPointUpdate -= HandleSteadyDetectorPrimaryPointUpdate;
//		NextState();
//	}
	
	
	
	private IEnumerator PlayBackState()
	{
		ResetEvents();
		c.nite.HUD.text  += "\nSee how it bounces? Swipe LEFT if you want to try again or RIGHT if you are done!";
//		c.selectShape.shape.rigidbody.isKinematic = true;
//		Vector3 pos = c.selectShape.shape.transform.position;
//		c.selectShape.shape.transform.position = new Vector3(pos.x, 3f, pos.z); //move it up!
//		c.selectShape.shape.rigidbody.isKinematic = false; //allow to drop!
		testBall.rigidbody.isKinematic = true;
		Vector3 pos = testBall.transform.position;
		testBall.transform.position = new Vector3(pos.x, 3f, pos.z +3f); //move it up and forward so we can see it!
		testBall.rigidbody.isKinematic = false; //allow to drop!

		while (state == Bouncer.State.PlayBack  && (!swipeLeft && !swipeRight)) //let it bounce until we detect a swipe
		{
			yield return new WaitForSeconds(10f);
			testBall.rigidbody.isKinematic = true;
			pos = testBall.transform.position;
			testBall.transform.position = new Vector3(pos.x, 3f, pos.z); //move it up and forward so we can see it!
			testBall.rigidbody.isKinematic = false; //allow to drop!
		}
//		if (swipeLeft)
//		{	
			// try again!
			state = Bouncer.State.DetectDropAndBounce;
			NextState();
//		}
//		else
//		{ //DISABLED FOR NOW
//			//tell everyone we're done
//			bouncing = false;
//			if (BouncerNotifierEvent != null)
//			{
//				BouncerNotifierEvent(this, System.EventArgs.Empty);
//			}
//		}
	}

	//EVENT HANDLERS:
	
	void HandleSwipeDetectorSwipeLeft (object sender, VelocityAngleEventArgs e)
	{
		
		//c.nite.HUD.text = "Swipe Left Detected: " + e.Angle + ", " + e.Velocity;
		swipeLeft = true;
	}
	
	void HandleSwipeDetectorSwipeRight (object sender, VelocityAngleEventArgs e)
	{
		
		//c.nite.HUD.text = "Swipe Right Detected: " + e.Angle + ", " + e.Velocity;
		swipeRight = true;
	}
	
	void HandleSteadyDetectorPrimaryPointUpdate (object sender, HandEventArgs e)
	{
		currentHandY =  e.Hand.Position.Y;
	}

	void HandleSteadyDetectorSteady (object sender, SteadyEventArgs e)
	{
		steadyEvent = true;
		Debug.Log("Steady Detected: " + e.ID);
	}
	

	void ResetEvents()
	{
		steadyEvent = false;
		swipeRight = false;
		swipeLeft = false;
	}
		
		
	private void NextState()
	{
		string methodName = state.ToString() + "State";
        StartCoroutine(methodName);
	}

}

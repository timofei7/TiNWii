using UnityEngine;
using System.Collections;
using NITE;


/// <summary>
/// the central state manager class for deformation game
/// </summary>
public class CentralDeformatory : MonoBehaviour {
	
	public enum State { Setup, Shaping }
			
	public NiteOpenNet nite;
	public UserGuiDeformatory userGui;
	public HandDetector handDetector;
	
	public State state;
	
	private int swipe;
	private float swipeResetCounter = 0f;
	private float swipeResetMax = 5f;
	
	void Awake()
	{
		nite = FindObjectOfType(typeof(NiteOpenNet)) as NiteOpenNet;
		userGui = FindObjectOfType(typeof(UserGuiDeformatory)) as UserGuiDeformatory;
		handDetector = FindObjectOfType(typeof(HandDetector)) as HandDetector;
		nite.NiteInitializingEvent += HandleNiteInitializingEvent;
		
		state = CentralDeformatory.State.Shaping;
		NextState();
	}
	
	void Update()
	{
		if (swipe > 0 && swipeResetCounter > swipeResetMax)
		{
			swipe = 0;
			swipeResetCounter=0f;
		}
		else if (swipe > 0)
		{
			swipeResetCounter += Time.deltaTime;
		}
	}
	
	/// <summary>
	/// register all of our handlers here once nite has started up!
	/// </summary>
	/// <param name="sender">
	/// A <see cref="System.Object"/>
	/// </param>
	/// <param name="e">
	/// A <see cref="System.EventArgs"/>
	/// </param>
	void HandleNiteInitializingEvent (object sender, System.EventArgs e)
	{
		nite.swipeDetector.SwipeLeft += HandleNiteSwipeDetectorSwipeLeft;
	}
	
	/// <summary>
	/// handle swiping left
	/// </summary>
	/// <param name="sender">
	/// A <see cref="System.Object"/>
	/// </param>
	/// <param name="e">
	/// A <see cref="NITE.VelocityAngleEventArgs"/>
	/// </param>
	void HandleNiteSwipeDetectorSwipeLeft (object sender, NITE.VelocityAngleEventArgs e)
	{
		++swipe;
		if (swipe == 1)
		{
			nite.HUD.text = "Swipe Left Again to Reset!";
		}
		else if (swipe == 2)
		{
			//Application.LoadLevel("TiNWiiTestScene");
		}
	}
	
	
	private void NextState()
	{
		string methodName = state.ToString() + "State";
        StartCoroutine(methodName);
	}
	
	
	private IEnumerator SetupState()
	{
		while (state == State.Setup)
		{
			//fillin in exit conditions/transitions
			yield return null;
		}
		NextState();
	}
		
	private IEnumerator ShapingState()
	{
		while (state == State.Shaping)
		{
			//fillin in exit conditions/transitions
			yield return null;
		}
		NextState();
	}

		
}

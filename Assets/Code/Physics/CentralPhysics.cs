using UnityEngine;
using System.Collections;


/// <summary>
/// the central state manager class for the physics game
/// </summary>
public class CentralPhysics: MonoBehaviour {
	
	public enum State { Creating, Weighing, Bouncing, Frictioning, Magneting, Coloring, Thermaling, Playing, Shaping }
	
	public enum Mode { Implicit, Explicit }
		
	public NiteOpenNet nite;
	public SelectShape selectShape;
	public UserGuiDeformatory userGui;
	public Bouncer bouncer;
	public HandDetector handDetector;
	public BallAnimator ballAnimator;
	
	public State state;
	
	
	void Awake()
	{
		nite = FindObjectOfType(typeof(NiteOpenNet)) as NiteOpenNet;
		selectShape = FindObjectOfType(typeof(SelectShape)) as SelectShape;
		userGui = FindObjectOfType(typeof(UserGuiDeformatory)) as UserGuiDeformatory;
		bouncer = FindObjectOfType(typeof(Bouncer)) as Bouncer;
		ballAnimator = FindObjectOfType(typeof(BallAnimator)) as BallAnimator;
	}
	
	void Start()
	{
		// only start when nite is finished starting
		nite.NiteInitializingEvent += delegate {
			state = CentralPhysics.State.Bouncing;
			NextState();
		};
	}
	
	
	private void NextState()
	{
		string methodName = state.ToString() + "State";
        StartCoroutine(methodName);
	}
	
	
	private IEnumerator PlayingState()
	{
		while (state == State.Playing)
		{
			//fillin in exit conditions/transitions
			yield return null;
		}
		NextState();
	}
	
	private IEnumerator CreatingState()
	{
		while (state == State.Creating)
		{
			//fillin in exit conditions/transitions
			yield return null;
		}
		NextState();
	}
	
	private IEnumerator BouncingState()
	{
		yield return new WaitForSeconds(5f); //to let nite initialize
		bouncer.StartBounceDetector();
		while (state == State.Bouncing && bouncer.bouncing)
		{
			//fillin in exit conditions/transitions
			yield return null;
		}
		bouncer.StopBounceDetector();
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

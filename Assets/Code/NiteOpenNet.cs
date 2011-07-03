using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenNI;
using NITE;

/// <summary>
/// this is the main Kinect Connector Class
/// it uses the OpenNI.net.dll version 1.1.0.39 from April 12th 2011
/// 
/// (some parts based loosely on UnityWrapper by Shlomo Zippel and Nite5.cs by Amir Hirsch)
/// 
/// @author Tim Tregubov April, 2011
/// </summary>
public class NiteOpenNet : MonoBehaviour
{	
	
	private readonly string XML_SETUP_FILE = @".//OpenNI.xml";
	public Context context;
	public DepthGenerator depthGenerator;
	public SessionManager sessionManager;
	public GestureGenerator gestureGenerator;
	public UserGenerator userGenerator;
	public HandsGenerator handsGenerator;
	public SkeletonCapability skeletonCapability;
	public PoseDetectionCapability poseDetectionCapability;
	public SwipeDetector swipeDetector;
	public SelectableSlider2D selectableSlider2D;
	public PointDenoiser pointDenoiser;
	public FlowRouter flowRouter;
	public SteadyDetector steadyDetector;
	private string calibPose;
	private bool shouldRun; //checks to make sure we've finished initializing before running

	public GUIText HUD; //the overhead text display for directions for now
	public GUITexture Hand;  //the hand display
	
	//exposing the transforms to be assigned to any model
	public Transform rightHand;
	public Transform leftHand;
	public Transform leftWrist;
	public Transform rightWrist;
	public Transform leftElbow;
	public Transform rightElbow;
	public Transform rightArm;
	public Transform leftArm;
	public Transform rightKnee;
	public Transform leftKnee;
	public Transform rightHip;
	public Transform leftHip;
	public Transform rightAnkle;
	public Transform leftAnkle;
	public Transform rightFoot;
	public Transform leftFoot;
	public Transform waist;
	public Transform torso;
	public Transform neck;
	public Transform leftCollar;
	public Transform rightCollar;
	public Transform head;
	

	private Quaternion[] initialRotations; //save rotations
	private Quaternion initialRoot; //save root


	public Vector3 bias;
	public float scale;
		
	//for usermap/depthmap
	public Texture2D usersLabelTexture;
    public Color[] usersMapColors;
    public Rect usersMapRect;
    public int usersMapSize;
    public short[] usersLabelMap;
    public short[] usersDepthMap;
    public float[] usersHistogramMap;
		
	public bool needsCalibration = true; //turns on/off the depth map only during calibration
	private string hudtext = "Testing 1,2,3";
	private float hudcount = 0;
	private float updateDepthCounter = 0f; 
	private float oldY = 1f;
	
	private SelectShape selectShape;
	
	public event EventHandler NiteInitializingEvent;

	
	/// <summary>
	/// puts skeleton into the original position
	/// </summary>
	private void RotateToInitialPosition ()
	{
		//waist.rotation = Quaternion.LookRotation(Vector3.forward);
		//root.rotation = initialRoot;
		torso.rotation = initialRotations[(int)SkeletonJoint.Torso];
		rightArm.rotation = initialRotations[(int)SkeletonJoint.RightShoulder];
		leftArm.rotation = initialRotations[(int)SkeletonJoint.LeftShoulder];
		rightElbow.rotation = initialRotations[(int)SkeletonJoint.RightElbow];
		
		//VT
		//rightWrist.rotation = initialRotations[(int)SkeletonJoint.RightWrist];
		
		leftElbow.rotation = initialRotations[(int)SkeletonJoint.LeftElbow];
		rightHip.rotation = initialRotations[(int)SkeletonJoint.RightHip];
		leftHip.rotation = initialRotations[(int)SkeletonJoint.LeftHip];
		rightKnee.rotation = initialRotations[(int)SkeletonJoint.RightKnee];
		leftKnee.rotation = initialRotations[(int)SkeletonJoint.LeftKnee];
		//Debug.Log ("Rotated to Initial Position");
	}


	/// <summary>
	/// sets the skeleton in calibration pose
	/// simply initial position with hands raised up
	/// </summary>
	private void RotateToCalibrationPose ()
	{
		RotateToInitialPosition ();
		//Debug.Log ("RightElbow: " + rightElbow.rotation + "Left Elbow: " + leftElbow.rotation);
		rightElbow.rotation = Quaternion.Euler (0, -90, 90) * initialRotations[(int)SkeletonJoint.RightElbow];
		leftElbow.rotation = Quaternion.Euler (0, 90, -90) * initialRotations[(int)SkeletonJoint.LeftElbow];
		//Debug.Log ("RightElbow: " + rightElbow.rotation + "Left Elbow: " + leftElbow.rotation);
		//Debug.Log ("Character Initialized to Calibration Pose");
	}

	
	/// <summary>
	/// initializes characters
	/// stores an array of initial rotations
	/// and then puts into calibratition pose
	/// </summary>
	private void InitializeCharacter ()
	{
		
		//waist.rotation = Quaternion.LookRotation(-Vector3.forward);
		initialRotations = new Quaternion[24];
		//TODO: note there are more (24) joints in xn.SkeletonJoint spec however
		initialRotations[(int)SkeletonJoint.LeftElbow] = leftElbow.rotation;
		initialRotations[(int)SkeletonJoint.RightElbow] = rightElbow.rotation;
		initialRotations[(int)SkeletonJoint.LeftShoulder] = leftArm.rotation;
		initialRotations[(int)SkeletonJoint.RightShoulder] = rightArm.rotation;
		initialRotations[(int)SkeletonJoint.RightKnee] = rightKnee.rotation;
		initialRotations[(int)SkeletonJoint.LeftKnee] = leftKnee.rotation;
		initialRotations[(int)SkeletonJoint.RightHip] = rightHip.rotation;
		initialRotations[(int)SkeletonJoint.LeftHip] = leftHip.rotation;
		initialRotations[(int)SkeletonJoint.Torso] = torso.rotation;
		initialRotations[(int)SkeletonJoint.Waist] = waist.rotation;
		//foreach (Quaternion quat in initialRotations) {
		//	Debug.Log (quat);
		//}
		//Debug.Log ("initialRotations set" + (int)SkeletonJoint.LeftElbow);
		//VT
		//initialRotations[(int)SkeletonJoint.RightWrist] = rightWrist.rotation;

		//initialRoot = root.rotation;
		
		RotateToCalibrationPose ();
	}


	void Start ()
	{
		InitializeCharacter ();
		context = new Context (XML_SETUP_FILE);
		sessionManager = new SessionManager(context, "Wave", "RaiseHand");
		depthGenerator = context.FindExistingNode(NodeType.Depth) as DepthGenerator;
		handsGenerator = context.FindExistingNode(NodeType.Hands) as HandsGenerator;
		
		//image = context.FindExistingNode(NodeType.Image) as ImageGenerator;
		//gesture = context.FindExistingNode(NodeType.Gesture) as GestureGenerator;
		if (depthGenerator == null) throw new Exception("Viewer must have a depth node!");
		//if (gesture == null) throw new Exception("Viewer must have a gesture node!");
		//if (image == null) throw new Exception("Viewer must have a image node!");
		
		userGenerator = new UserGenerator (context);
		skeletonCapability = userGenerator.SkeletonCapability;
		poseDetectionCapability = userGenerator.PoseDetectionCapability;
		calibPose = skeletonCapability.CalibrationPose;
		//gestureGenerator = new GestureGenerator(context);
		
		foreach (String s in poseDetectionCapability.GetAllAvailablePoses())
			Debug.LogWarning("available pose found: " +s);
		
		//foreach (String s in gestureGenerator.EnumerateAllGestures())
		//	Debug.LogWarning("available gesture found: " + s);
		
		//gestureGenerator.GestureRecognized += HandleGestureGestureRecognized;
		//gesture.AddGesture("Click");
		//gestureGenerator.AddGesture("RaiseHand"); //seems buggy
		//gestureGenerator.AddGesture("Wave");
		
		swipeDetector = new SwipeDetector();
		//steadyDetector = new SteadyDetector();//(3, 1f);
		
		//swipeDetector.SwipeLeft += HandleSwipeDetectorSwipeLeft;
				
		//Session Managment
		sessionManager.SessionStart += HandleSessionManagerSessionStart;
		sessionManager.SessionEnd += HandleSessionManagerSessionEnd;
		
		//User Generator
		userGenerator.NewUser += HandleUserGeneratorNewUser;
		userGenerator.LostUser += HandleUserGeneratorLostUser;
		poseDetectionCapability.PoseDetected += HandlePoseDetectionCapabilityPoseDetected;
		skeletonCapability.CalibrationEnd += HandleSkeletonCapabilityCalibrationEnd;
		
		skeletonCapability.SetSkeletonProfile (SkeletonProfile.All);
		skeletonCapability.SetSmoothing(.5f); // give us some smooothing
		
		//Start generating
		userGenerator.StartGenerating();
		//handsGenerator.MirrorCapability.SetMirror(true); // TODO: should this be true? 
		handsGenerator.StartGenerating();
		//gestureGenerator.StartGenerating();
		
		
		//slider selector
		//selectableSlider2D = new SelectableSlider2D(Screen.width, Screen.height);
		//selectableSlider2D.ValueChange += HandleSelectableSlider2DValueChange;
		//selectableSlider2D.ItemHover += HandleSelectableSlider2DItemHover;
		//selectableSlider2D.ItemSelect += HandleSelectableSlider2DItemSelect;
		
		pointDenoiser = new PointDenoiser();
		pointDenoiser.AddListener(swipeDetector);
		//pointDenoiser.AddListener(steadyDetector);
		//pointDenoiser.AddListener(selectableSlider2D);
		
		//flowRouter = new FlowRouter();
		//flowRouter.ActiveListener = pointDenoiser;
		
		sessionManager.AddListener(pointDenoiser);
	
		MapOutputMode mapMode = depthGenerator.MapOutputMode;
				
		
		// Init depth & label map related stuff
		usersMapSize = mapMode.XRes * mapMode.YRes;
		//usersLabelTexture = new Texture2D(mapMode.XRes, mapMode.YRes); //nonPOT slow
		usersLabelTexture = new Texture2D(1024,512);
		//speed up by using power of two and then setpixel() with blockwidth (640) and blockheight (480)
        usersMapColors = new Color[usersMapSize];
        usersMapRect = new Rect(Screen.width - usersLabelTexture.width / 2, Screen.height - usersLabelTexture.height / 2, usersLabelTexture.width / 2, usersLabelTexture.height / 2);
        usersLabelMap = new short[usersMapSize];
        usersDepthMap = new short[usersMapSize];
        usersHistogramMap = new float[5000];

		this.shouldRun = true;
		
		if (NiteInitializingEvent != null) //notify others that we're done initializing
			NiteInitializingEvent(this, EventArgs.Empty);
	}

	void HandleSwipeDetectorSwipeLeft (object sender, VelocityAngleEventArgs e)
	{
		
		Debug.Log("Swipe Detected: " + e.Angle + ", " + e.Velocity);
	}
	
	
	/// <summary>
	/// this is a way to have a two dimensional pointer
	/// </summary>
	/// <param name="sender">
	/// A <see cref="System.Object"/>
	/// </param>
	/// <param name="e">
	/// A <see cref="Index2DirectionEventArgs"/>
	/// </param>
	void HandleSelectableSlider2DItemSelect (object sender, Index2DirectionEventArgs e)
	{
		//Debug.Log("item selected: " + e.IndexX + "," + e.IndexY + " dir: " + e.Direction);
		//CheckGui(new Vector2(e.IndexX, e.IndexY));
	}

	void HandleSelectableSlider2DItemHover (object sender, Index2EventArgs e)
	{
		//HUD.text = "hovered over: " + e.IndexX + "," + e.IndexY;
	}

	void HandleSelectableSlider2DValueChange (object sender, Value2EventArgs e)
	{
		Hand.transform.position = new Vector3(e.ValueX, e.ValueY, 0);
	}

	void HandleSessionManagerSessionEnd(object sender, EventArgs e)
	{
		HUD.text = HUD.text + "\nSession End - Wave to restart";
		Debug.Log("Session End");
	}
	
	void HandleSessionManagerSessionStart(object sender, PositionEventArgs e)
	{
		Debug.Log("Session Start: " + e.Position);
	}
	
	
	void HandleSkeletonCapabilityCalibrationEnd(object sender, CalibrationEndEventArgs ce)
	{
		if (ce.Success) {
			HUD.text =  (HUD.text + "\ncallibration ended successfully");
			skeletonCapability.StartTracking (ce.ID);
			needsCalibration = false;
			//c.bouncer.StartBounceDetector();
		} else {
			poseDetectionCapability.StartPoseDetection (calibPose, ce.ID);
		}
	}
	
	void HandlePoseDetectionCapabilityPoseDetected(object sender, PoseDetectedEventArgs pd)
	{
		HUD.text = (HUD.text + "\nPose Detected: " + pd.Pose);
		poseDetectionCapability.StopPoseDetection (pd.ID);
		skeletonCapability.RequestCalibration (pd.ID, true);
	}
	
	void HandleUserGeneratorLostUser(object sender, UserLostEventArgs ul)
	{
		HUD.text =  (HUD.text + "\nLost User: " + ul.ID);
		needsCalibration = true;
		//     this.joints.Remove(id);
	}
	
	void HandleGestureGestureRecognized(object sender, GestureRecognizedEventArgs ge)
	{
		// these points don't appear useful haha
		Vector3 point = new Vector3(ge.IdentifiedPosition.X, ge.IdentifiedPosition.Y, ge.IdentifiedPosition.Z);
		Vector3 spoint = Camera.mainCamera.WorldToScreenPoint(point);
		HUD.text = (HUD.text + "\nGesture Recognized: " + ge.Gesture + " at: " + spoint.ToString());

	}
	
	void HandleUserGeneratorNewUser(object sender, NewUserEventArgs nu)
	{
		HUD.text =  (HUD.text + "\nNew User Found: " + nu.ID);
		this.poseDetectionCapability.StartPoseDetection(calibPose, nu.ID);
	}
	
			
	
	void Update ()
	{
		if (hudcount > 10) { HUD.text = ""; hudcount = 0; }; 
		if (HUD.text == hudtext) { hudcount += Time.deltaTime; }
		else {hudtext = HUD.text; hudcount = 0;}
		
		
		if (shouldRun) { //check to make sure that start is finished?
			try {
				context.WaitOneUpdateAll(depthGenerator); //TODO: try WaitNoneUpdateAll? 
				sessionManager.Update(context); //TODO: do we need this?
			} catch (Exception) {
				Debug.LogWarning("why would update fail?");
			}
			
			//update/generate the depth map for display //TODO: very expensive 50fps slowdown, fix this
			//if (needsCalibration)
			if (true)
			{
				if (updateDepthCounter > .05f)
				{
					UpdateUserMap(); 
					updateDepthCounter = 0f;
				} else
				{
					updateDepthCounter += Time.deltaTime;
				}
			}

			
			int[] users = userGenerator.GetUsers ();
			
			//TODO: support multiple users
			//Debug.Log(users.Length);
			foreach (int user in users) {
				if (skeletonCapability.IsTracking(user)) {
					needsCalibration = false;
					
					//TODO: using joint positions should be more accurate than joint angles according to the docs
					//TODO: figure out the missing joints
					
					//	MoveTransform(user, SkeletonJoint.Head, head);
					//	MoveTransform(user, SkeletonJoint.Neck, neck);
					TransformBone (user, SkeletonJoint.Torso, torso);
					MoveTransform (user, SkeletonJoint.Torso, waist, true); //this moves the root 
					
					//	MoveTransform(user, SkeletonJoint.RightHand, rightHand);
					//	MoveTransform(user, SkeletonJoint.RightWrist, rightWrist);
					TransformBone (user, SkeletonJoint.RightElbow, rightElbow);
					TransformBone (user, SkeletonJoint.RightShoulder, rightArm);
					
					
					//	MoveTransform(user, SkeletonJoint.RightCollar, rightCollar);							
					TransformBone (user, SkeletonJoint.RightHip, rightHip);
					TransformBone (user, SkeletonJoint.RightKnee, rightKnee);
					//	MoveTransform(user, SkeletonJoint.RightAnkle, rightAnkle);
					//	MoveTransform(user, SkeletonJoint.RightFoot, rightFoot);														
					
					
					//	MoveTransform(user, SkeletonJoint.LeftHand, leftHand);
					//	MoveTransform(user, SkeletonJoint.LeftWrist, leftWrist);
					TransformBone (user, SkeletonJoint.LeftElbow, leftElbow);
					TransformBone (user, SkeletonJoint.LeftShoulder, leftArm);
					//	MoveTransform(user, SkeletonJoint.LeftCollar, leftCollar);
					TransformBone (user, SkeletonJoint.LeftHip, leftHip);
					TransformBone (user, SkeletonJoint.LeftKnee, leftKnee);
					//	MoveTransform(user, SkeletonJoint.LeftAnkle, leftAnkle);
					//	MoveTransform(user, SkeletonJoint.LeftFoot, leftFoot);	
					
					// this applies a fix to roots height after we've already moved everything
					FixY(user);
				}
			}
		}
	}
	
	
	//TODO: needs to allow jumps
	/// <summary>
	/// fixes Y to so that its always on the ground 
	/// 
	/// </summary>
	/// <param name="userId">
	/// A <see cref="System.Int32"/>
	/// </param>
	private void FixY(int userId)
	{
		SkeletonJointPosition pos = skeletonCapability.GetSkeletonJointPosition(userId, SkeletonJoint.Torso);
		Vector3 v3pos = new Vector3 (pos.Position.X, pos.Position.Y, -pos.Position.Z);
		Vector3 newpos = (v3pos / scale) + bias;

		float maxY = Mathf.Max(newpos.y - leftFoot.position.y, newpos.y - rightFoot.position.y);
		float newY = Mathf.Lerp(oldY, maxY, 10f * Time.deltaTime);
		oldY = newY; 
		waist.position = new Vector3(waist.position.x, newY, waist.position.z);
	}
	
	
	/// <summary>
	/// moves a transform (joint) given a Skeleton Joint Position
	/// can be less error prone than Rotating accordingn to documentation
	/// </summary>
	/// <param name="userId">
	/// A <see cref="System.Int32"/>
	/// </param>
	/// <param name="joint">
	/// A <see cref="SkeletonJoint"/>
	/// </param>
	/// <param name="dest">
	/// A <see cref="Transform"/>
	/// </param>
	private void MoveTransform (int userId, SkeletonJoint joint, Transform dest, bool root)
	{
		SkeletonJointPosition pos = skeletonCapability.GetSkeletonJointPosition(userId, joint);
		float y;
		if (root)
		{
			y = pos.Position.Y;
		}
		else
		{
			y = pos.Position.Y;
		}
		Vector3 v3pos = new Vector3 (pos.Position.X, y, -pos.Position.Z);
		dest.position = (v3pos / scale) + bias;

	}
	
	
	/// <summary>
	/// rotates a joint
	/// </summary>
	/// <param name="userId">
	/// A <see cref="System.Int32"/>
	/// </param>
	/// <param name="joint">
	/// A <see cref="SkeletonJoint"/>
	/// </param>
	/// <param name="dest">
	/// A <see cref="Transform"/>
	/// </param>
	void TransformBone (int userId, SkeletonJoint joint, Transform dest)
	{
		
		SkeletonJointOrientation ori = skeletonCapability.GetSkeletonJointOrientation(userId, joint);
		
		// only modify joint if confidence is high enough in this frame
		if (ori.Confidence > 0.5)
		{
//			//  Z coordinate in OpenNI is opposite from Unity. We will create a quat
//			//  to rotate from OpenNI to Unity (relative to initial rotation)
//			//  elem1 elem2 elem3  = X1 Y1 Z1
//			//  elem4 elem5 elem6  = X2 Y2 Z2
//			//  elem7 elem8 elem9  = X3 Y2 Z3
//			TODO:  which is better, convert matrix to quat using formula or extracting worldz and worldy and doing a lookrototion?
//			Vector3 worldZVec = new Vector3 (-ori.Z1, -ori.Z2, ori.Z3);			
//			Vector3 worldYVec = new Vector3 (ori.Y1, ori.Y2, -ori.Y3); 
//			
//			Quaternion jointRotation = Quaternion.LookRotation (worldZVec, worldYVec);  
			//   which is:  LookRotation (forward : Vector3, upwards : Vector3 = Vector3.up) : Quaternion
			//   Creates a rotation that looks along forward with the the head upwards along upwards
			
			Quaternion jointRotation = Utils.openNIMatrixToQuat(ori);
			Quaternion newRotation = (jointRotation * initialRotations[(int)joint]);  
			
			// Some smoothing
			dest.rotation = Quaternion.Slerp (dest.rotation, newRotation, Time.deltaTime * 10); // smoothing used to b 20
		}
		
	}
	
	
	/// <summary>
	/// updates the usermap
	/// from Shlomo 
	/// </summary>
	void UpdateUserMap()
    {
		SceneMetaData smd = userGenerator.GetUserPixels(0); //TODO: handle multiple users?
        // copy over the maps
        Marshal.Copy(smd.LabelMapPtr, usersLabelMap, 0, usersMapSize);         //slowest
        Marshal.Copy(depthGenerator.DepthMapPtr, usersDepthMap, 0, usersMapSize);  //slowest
		
        // we will be flipping the texture as we convert label map to color array
        int flipIndex, i;
        int numOfPoints = 0;
		Array.Clear(usersHistogramMap, 0, usersHistogramMap.Length);

        // calculate cumulative histogram for depth
        for (i = 0; i < usersMapSize; i++)
        {
            // only calculate for depth that contains users
            if (usersLabelMap[i] != 0)
            {
                usersHistogramMap[usersDepthMap[i]]++;
                numOfPoints++;
            }
        }
        if (numOfPoints > 0)
        {
            for (i = 1; i < usersHistogramMap.Length; i++)
	        {   
		        usersHistogramMap[i] += usersHistogramMap[i-1];
	        }
            for (i = 0; i < usersHistogramMap.Length; i++)
	        {
                usersHistogramMap[i] = 1.0f - (usersHistogramMap[i] / numOfPoints);
	        }
        }

        // create the actual users texture based on label map and depth histogram
        for (i = 0; i < usersMapSize; i++)
        {
            flipIndex = usersMapSize - i - 1;
            if (usersLabelMap[i] == 0)
            {
                usersMapColors[flipIndex] = Color.clear;
            }
            else
            {
                // create a blending color based on the depth histogram
                Color c = new Color(usersHistogramMap[usersDepthMap[i]], usersHistogramMap[usersDepthMap[i]], usersHistogramMap[usersDepthMap[i]], 0.9f);
                switch (usersLabelMap[i] % 4)
                {
                    case 0:
                        usersMapColors[flipIndex] = Color.red * c;
                        break;
                    case 1:
                        usersMapColors[flipIndex] = Color.green * c;
                        break;
                    case 2:
                        usersMapColors[flipIndex] = Color.blue * c;
                        break;
                    case 3:
                        usersMapColors[flipIndex] = Color.magenta * c;
                        break;
                }
            }
        }

        // usersLabelTexture.SetPixels(usersMapColors);
		usersLabelTexture.SetPixels(0,0,640,480, usersMapColors); //so much faster! nonPOT is slow
        usersLabelTexture.Apply();
    }
	
	
	public void CalibratingGui()
	{
		if (!usersMapRect.Equals(null) && usersLabelTexture != null)
			GUI.DrawTexture(usersMapRect, usersLabelTexture);
	}
	
	
//	void CheckGui(Vector2 v)  //TODO: make this functionally elsehwere
//	{
//		if (c.currState == Central.State.Creating)
//		{
//			v.y = Screen.height - v.y;
//			//Debug.LogWarning(v.ToString());
//			//Button to create sphere
//			if (c.userGui.buttonOne.Contains(v))
//				c.selectShape.CreateShape(1);
//			//Button to create cube
//			else if (c.userGui.buttonTwo.Contains(v))
//				c.selectShape.CreateShape(2);
//			//Button to create cylinder
//			else if (c.userGui.buttonThree.Contains(v))
//				c.selectShape.CreateShape(3);
//		}
//	}

	
}


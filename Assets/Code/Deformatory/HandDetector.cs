using UnityEngine;
using System.Collections;
using OpenNI;
using NITE;
using System;
using Emgu.CV;
using Emgu.CV.Structure;

/// <summary>
/// detects whether hand is open or closed and generates events
/// 
/// @author Tim Tregubov May 2011
/// </summary>
public class HandDetector : MonoBehaviour {
	
	public Texture2D handClosed;
	public Texture2D handOpen;
	public GUITexture handTexture;
	
	public Point3D handPosition;
		
	public bool open = true;
	public bool handExists = false;
		
	private NiteOpenNet nOn;
	
	private Image<Gray, Byte> testImage;
	private Texture2D testTexture;
	private Rect testTextureRect;
	
	// turn on to show the debug image of the hand image that is being analyzed for open/closed hand
	private bool debugHandImage = false;
	
	public float smoothedDefectsSize = 0f;
	public float openThreshold = 3f;
	
	
	void Awake()
	{
		nOn = FindObjectOfType(typeof(NiteOpenNet)) as NiteOpenNet;
		nOn.NiteInitializingEvent += HandleNiteInitializingEvent;
		if (debugHandImage)
		{
			testTexture = new Texture2D(1024,512);
			testTextureRect = new Rect(testTexture.width / 2, testTexture.height / 2, testTexture.width / 2, testTexture.height / 2);
		}
	}
	
	void Update()
	{
//		if (handExists)
//		{
//			DetectHand();
//		}

	}

	void HandleNiteInitializingEvent (object sender, EventArgs e)
	{
		nOn.handsGenerator.HandCreate += HandleHandsGeneratorHandCreate;
		nOn.handsGenerator.HandUpdate += HandleHandsGeneratorHandUpdate;
		nOn.handsGenerator.HandDestroy += HandleHandsGeneratorHandDestroy;
	}
	

	void HandleHandsGeneratorHandDestroy (object sender, HandDestroyEventArgs e)
	{
		handExists = false;
		Debug.Log("Deleted Hand: " + e.UserID);
		//nOn.HUD.text = "Deleted Hand:" + e.UserID;
		nOn.handsGenerator.HandUpdate -= HandleHandsGeneratorHandUpdate;
		smoothedDefectsSize = 0f;
	}
	
	void HandleHandsGeneratorHandUpdate(object sender, HandUpdateEventArgs e)
	{
		// update the hand position
		handPosition = e.Position;
		DetectHand();
	}
	
	void HandleHandsGeneratorHandCreate(object sender, HandCreateEventArgs e)
	{
		smoothedDefectsSize = 0f;
		Debug.Log("Created Hand: " + e.UserID + " at: " + e.Position.X + "," + e.Position.Y +","+e.Position.Z);
		//nOn.HUD.text = "Created Hand" + e.UserID + " at: " + e.Position.X + "," + e.Position.Y +","+e.Position.Z;
		handPosition = e.Position;
		handExists = true;
		nOn.handsGenerator.HandUpdate += HandleHandsGeneratorHandUpdate;
		DetectHand();
	}
	
	
	/// <summary>
	/// runs and detects the shape of the current hand if it exists
	/// </summary>
	void DetectHand()
	{
		int width = nOn.depthGenerator.MapOutputMode.XRes;
		int height = nOn.depthGenerator.MapOutputMode.YRes;
		Point3D handProjective = nOn.depthGenerator.ConvertRealWorldToProjective(handPosition);
		handProjective = new Point3D(width - handProjective.X, height - handProjective.Y, handProjective.Z);
		//TODO:  adjust boxsize based on distance
		int boxsize = 50;
		int minX = (int)handProjective.X - boxsize;
		int maxX = (int)handProjective.X + boxsize;
		int minY = (int)handProjective.Y - boxsize;
		int maxY = (int)handProjective.Y + boxsize;
		// show the box 
		
		testImage = new Image<Gray, Byte>(height, width, new Gray(0.0));
				
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				int idx = nOn.usersMapSize - (y * width + x) - 1; // flip the depth index as the depth map is inverte
				int depth = nOn.usersDepthMap[idx]; 
				// if we're in a box surround the hand point add those points to the texture
				// use non projective for depth - in mm as is point
				if (y > minY && y < maxY && x > minX && x < maxX && handPosition.Z < depth + boxsize + 10 && handPosition.Z > depth - boxsize + 10 && nOn.usersLabelMap[idx] !=0)
				{
					try {testImage[x,y] = new Gray(255.0);}
					catch (Exception e) {Debug.LogError("ERROR INDEX: " + x + "," + y + " : " + e.Message);}
					if (debugHandImage)
						testTexture.SetPixel(x, y, Color.white);
				}
				else
				{
					if (debugHandImage)
						testTexture.SetPixel(x,y, Color.black);
				}
			}
		}
		
		if (debugHandImage)
			testTexture.Apply();
		
		
		smoothedDefectsSize = Mathf.Lerp(smoothedDefectsSize, GetContourDefects(testImage), .1f); //smooths it over time 
		
		//decide if its open or closed based on the size of the average defects // bigger in case of open hand by a lot
		if (smoothedDefectsSize < openThreshold)
		{
			open = false;
			if (handTexture.texture.Equals(handOpen))
				handTexture.texture = handClosed;
		}
		else
		{
			open = true;
			if (handTexture.texture.Equals(handClosed))
				handTexture.texture = handOpen;
		}
		//nOn.HUD.text = "smoothed defect size: " + smoothedDefectsSize;
	}
	
	
	/// <summary>
	/// if we have a Gui put it here so that its controllable from the one gui class and combines into one single OnGUI call rather than multiple
	/// </summary>
	public void Gui()
	{
		if (debugHandImage)
			GUI.DrawTexture(testTextureRect, testTexture);
	}
	
	
	
	/// <summary>
	/// this analyzes the image for covexity defects and returns the average size of the defects found
	/// this is large if hand open and small if hand closed in the range of >2 is open and less than is closed
	/// </summary>
	/// <param name="image">
	/// A <see cref="Image<Gray, Byte>"/>
	/// </param>
	/// <returns>
	/// A <see cref="System.Int32"/>
	/// </returns>
	private float GetContourDefects(Image<Gray, Byte> image)
    {
		
        Seq<MCvConvexityDefect> defects;
        MCvConvexityDefect[] defectArray;
		
		//initialize a storage for all the following
        using (MemStorage storage = new MemStorage())
        {
			
			//find the contours
            Contour<System.Drawing.Point> contours = image.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST, storage);
            Contour<System.Drawing.Point> biggestContour = null;
			
			//find the biggest contour which should be the hand as its the only thing in the image
            double areaA = 0;
            double areaB = 0;
			//pull out the biggest contour from the ones we found
            while (contours != null)
            {
                areaA = contours.Area;
                if (areaA > areaB)
                {
                    areaB = areaA;
                    biggestContour = contours;
                }
                contours = contours.HNext;
            }
			
			//analyze the biggest
            if (biggestContour != null)
            {
				//smooth the contour
                Contour<System.Drawing.Point> currentContour = biggestContour.ApproxPoly(biggestContour.Perimeter * 0.0025, storage);
                biggestContour = currentContour;
            
				//get the convex hull from the smoothed contour
                biggestContour.GetConvexHull(Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);
				
				//get the convexity defect points
                defects = biggestContour.GetConvexityDefacts(storage, Emgu.CV.CvEnum.ORIENTATION.CV_CLOCKWISE);

                defectArray = defects.ToArray();
				//nOn.HUD.text = "DEFECTS FOUND = " + defectArray.Length;
				
				//average the depths of the points
				float depthAverage = 0;
				for (int i=0; i < defectArray.Length-1; i++)
				{
					depthAverage += defectArray[i].Depth;
				}
				depthAverage = depthAverage / defectArray.Length;
				//nOn.HUD.text += "depth average: " + depthAverage;
				return depthAverage;
            }
			else
			{
				//nOn.HUD.text = "NO CONTOURS";
				return 0f;
			}
        }
	}

	
}

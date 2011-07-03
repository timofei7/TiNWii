## This is Not Wii ##

Independent Study COGS81, Spring 2011



<iframe id="kinectscuplt" width="853" height="480" src="http://www.youtube.com/embed/4tIXcKJwe8M?rel=0&amp;hd=1" frameborder="0" allowfullscreen></iframe>
(Tim Tregubov, CS80 Spring 2011)

I had wanted to play/hack a Kinect so I completed an independent study project researching  motion detection techniques, physics reconstruction, and geometry sculpting. I implemented a program to sculpt some 3D geometry with your hands using the Kinect.

<!--more-->


Originally the goal was simple: do some Kinect programming.  It then morphed into what was going to become a physics based game in which you have to define physical properties of objects using intuitive physics based gestures.  For instance to define the bounciness of an object you would set the coefficient of restitution by showing the dropped-from height and the first bounce-up-to height using your hands -- simple yet also correct. For friction you could indicate the angle of incline necessary to break static friction with you arms.  Other properties were harder to do intuitively.  One idea was a constant force model in which a progress bar would indicate some total possible energy available to expend and a user would, for instance, mime lifting an object and the progress bar (on a simple timer) would indicate how much force they need to lift the object (mass in relation to earths gravity), the longer (per distance) it took, the more mass the object had.  This was starting to become less intuitive.

The final physical property was shape, and with that came the idea of exploring mesh deformation using the Kinect.  So the final project after much paper reading became simply to implement some of that.  I had never done any motion programming or any image recognition so playing with this was interesting.  After coming up with a fairly uninteresting demo of the coefficient of restitution, I focused on mesh manipulation.

Mesh manipulation (such as in 3D sculpting tools like Mudbox) turned out to be difficult problem to do well.  The geometry tended to get pretty ugly after too much vertex displacement without resorting to lots of tricks to keep the triangles average sized and regular shaped and without subdividing them when they get too stretched out (what you see in the demo is pretty simple: a laplacian smoothing is applied periodically to keep vertices from getting too crazy)

I also wanted to be able to push and pull so I learned a bit of image processing to recognize open vs closed hands.  Open hand pushes the vertices away while closed pulls them toward you.  The pixels of the hand are extracted based on the depth map and hand point from the Kinect and then using the number of convexity defects in the contour to determine whether it is open or closed. 

Components:

* Unity3D
* C#
* OpenNI (for skeletal mocap and gestures)
* OpenCV (for hand image processing)


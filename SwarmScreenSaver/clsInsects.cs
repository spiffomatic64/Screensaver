using System;
using System.Drawing;
using CommonCode;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Reflection;

namespace Insects
{
	/// <summary>
	/// Copyright (c) 1988-91 by Patrick J. Naughton.<br>
	/// Permission to use, copy, modify, and distribute this software and its<br>
	/// documentation for any purpose and without fee is hereby granted,<br>
	/// provided that the above copyright notice appear in all copies and that<br>
	/// both that copyright notice and this permission notice appear in<br>
	/// supporting documentation.<br>
	/// <br>
	/// This file is provided AS IS with no warranties of any kind.  The author<br>
	/// shall have no liability with respect to the infringement of copyrights,<br>
	/// trade secrets or any patents by this file or any part thereof.  In no<br>
	/// event will the author be liable for any lost revenue or profits or<br>
	/// other special, indirect and consequential damages.<br>
    /// <br>
	/// ______________________________________________________________________<br>
	/// Patrick J. Naughton                email: naughton@sun.com<br>
	/// Sun Microsystems Laboratories, Inc.        voice: (415) 336 - 1080<br>
	/// Ported from Unix to Dos then to Windows by: Garry Freemyer (530) 877-1053<br>
	/// <br>
	/// This class contains the info on the swarms, the state of the swarm, and the wasp and the<br>
	/// intitialization routines and drawing methods to run this screensaver to any Graphics object passed in.
	/// 
	/// </summary>
	
	public class clsInsects
	{	
		/// <summary>
		/// This is used for the caller to inform Insects that it needs to stop now!<br>
		/// This is used in IF and For statements to help this class stop drawing<br>
		/// when the screen is to be destroyed.<br>
		/// </summary>
		public bool StopNow = false;

		private bool eraseOldLines = false;
		
		/// <summary>
		/// Default value is a count of 144. Count of bees. This is configureable. Defualts are required here<br>
		/// because the program cries foul when I pass them to methods, it thinks I'm going to try to access an<br>
		/// unassigned variable. Pah!
		/// </summary>
		private int Bees;            
		/// <summary>
		/// Default Bee Velocity although this is recalculated later to be 92% of overall velocity.<br>
		/// This is configureable.
		/// </summary>
		private int BeeVel;         
		/// <summary>
		/// How many seconds before the swarm color changes. Default:1 Configureable.
		/// </summary>
		private int ColorCycleSeconds;
		/// <summary>
		/// Whether Glotter is wanted. Default is false. <br>
		/// Drawing is done to a in memory bit map and then is copied to the screen. No flicker, faster drawing!
		/// </summary>
		private bool GlitterModeWanted;

		/// <summary>
		/// height of drawing area in pixels.
		/// </summary>
		private int screenHeight;	

		/// <summary>
		/// Width of drawing area in pixels.
		/// </summary>
		private int screenWidth; 

		/// <summary>
		/// Wasp is not allowed any closer to edge of screen than this value.<br>
		/// This is to reduce the swarm going off screen.
		/// </summary>
		private int Border;
		// <summary>
		// The Old X location of the wasp. I needed to erase the wasp then draw instead of how the original<br>
		// program did it because I instituted double-buffering. Init it to some arbitary value.<br>
		// but I don't have to keep checking all the time. We don't need the old positions. 
		// </summary>
		private int[] WaspOldX = new int[2]{1,2};
		private int[] WaspOldY = new int[2]{3,4};

		/// <summary>
		/// Wasp X position as a three int element array. I don't know why 3 elements.
		/// </summary>
		private int[] WaspX = new int[3]{0,0,0};
		/// <summary>
		/// Wasp Y position as a three int element array. I don't know why 3 elements.
		/// </summary>
		private int[] WaspY = new int[3]{0,0,0};
		/// <summary>
		/// Default wasp max velocity. Configureable. It is compared against the two X and Y<br>
		/// velocity of the Wasp and the bees.
		/// </summary>
		private int WaspVel;  
		private int WaspXVel = 0;        
		private int WaspYVel = 0;		
		/// <summary>
		/// Used to determine if it's time for the swarm to change color. First run through, we add -1 variables to<br>
		/// thia variable, to make it PAST time to pick a color. I don't want to always start with red.
		/// </summary>
		DateTime dtColorCycleSecondsTime = DateTime.Now.AddSeconds(-1); 
		/// <summary>
		/// I think there could have been a better name for this variable but such is the original code.<br>
		/// First Rank for x and y in swarmstruc. Ex: x[Times,Bee#] = SomeVelocity; 
		/// </summary>
		private const int TIMES = 4; 
		//------------------------------
		/// <summary>
		/// Ohhh some constants. I wish steady work was a constant for me instead<br>
		/// of jumping from one Titanic to another.<br> 
		/// Maximum accelleration for bees. A speed limit for the byte cops!
		/// </summary>
		private const byte BeeAccMax = 3; 
		//---------------------------------
		/// <summary>
		/// Maximum Wasp Accelleration. Accelleration is not to be confused with Velocity.
		/// </summary>
		private const byte WaspAccMax = 5;
		//---------------------------------------
		/// <summary>
		/// This pen is for drawing the swarm. Second parameter is the width of the line in pixels.<br>
		/// Default color is red.
		/// </summary>
		Pen SwarmPen = new Pen(Color.Red,1);
		Pen WaspPen = new Pen(Color.White,1);
		Pen BlackPen = new Pen(Color.Black,1);
		// ----------------------------------
		/// <summary>
		/// An instance of the class CommonFunctions. The random number and color contrast functions are here.<br>
		/// The contrast functions are used to ensure that the swarm not too dark to see.
		/// </summary>
		private CommonFunctions commFunctions = new CommonFunctions();
		/// <summary>
		/// Creates ONE swarm of bees to flee. Ouch!!
		/// </summary>
		private SwarmStruct swarm = new SwarmStruct(); 

		// ---------------------------------------------------------

		public clsInsects(int waspVelArg, int beesArg, int colorCycleSecsArg, bool glitterModeWantedArg)
		{
			WaspVel = waspVelArg;
			Bees = beesArg;
			ColorCycleSeconds = colorCycleSecsArg;
			GlitterModeWanted = glitterModeWantedArg;
		}
		/// <summary>
		/// Created a destructor to rid the SwarmPen and commFunctions.
		/// </summary>

		~clsInsects()
		{
			SwarmPen.Dispose();
			WaspPen.Dispose();
			BlackPen.Dispose();
			commFunctions = null;
		}

		/// <summary>
		/// Initializes some variables and allocates arrays. See source code in clsInsects.cs<br>
		/// In the GetConfig call, the out variables are set there in the call like a reference.
		/// </summary>
		public void initSwarm(int border, int ScreenWidth, int ScreenHeight, bool EraseOldLines)
		{
			int	b; // Used in Looping.
			Border = border;
			screenWidth = ScreenWidth; // Set screen width and height to this value that represents integer pixels.
			screenHeight = ScreenHeight;
			eraseOldLines = EraseOldLines;
			// Fetch the user configureable values from the registry for use in this class exclusively.
			BeeVel =  (int) (((double) WaspVel) * .92); // Velocity of bees which is about 8% slower than the Wasp Velocity which is called Base Velocity in the screensaver configuration screen.

			// Adjust array sizes. 
			swarm.segs = new Segment[Bees]; // Bees should have been BeeCount. Oh well. I'm a ditz at times! Allocate memory for an array of Segment structures with elements = the value in Bees.
			if (eraseOldLines)  
			{
				swarm.old_segs = new Segment[Bees]; // Same but for old segments. Not needed anymore.
			}
			swarm.x = new int[TIMES,Bees]; // Allocate for a two dimential array TIMES = 4 so it would be swarm.x[4,Bees] for each bee's X position.
			swarm.y = new int[TIMES,Bees]; // Does x or y stand for the end, the middle or the beginning of the segment. /shrug.
			swarm.xv = new int[Bees]; // Allocate an X velocity for each Bee. Do the same for Y Velocity in the line below.
			swarm.yv = new int[Bees]; // Space for the Y velocity for bees.
			
			// Initialize point positions, velocities, etc. 
			
			// Get random Wasp beginning and ending segment points. I could have used the Net Point class, but I didn't need all the extra baggage in the form on un-needed methods and properties.
			// Border for the wasp to bounce off of.
			WaspX[0] = Border + commFunctions.rand() % (screenWidth - 2 * Border);
			WaspY[0] = Border + commFunctions.rand() % (screenHeight - 2 * Border);
			WaspX[1] = WaspX[0]; // Make the first and second wasp segment Match. I think the idea was that the next drawing position would
			WaspY[1] = WaspY[0]; // correspond to the exact end point of the previously drawn segment that is now invisible via painting black over it to match the background.
			WaspXVel = 0; // C# gets sharp with you if you attempt to use an uninitialized variable as a parameter, and some other situations. like using the += operator.
			WaspYVel = 0; // See line above.


			// Process Bees 
			for (b = 0; b < this.Bees; b++) 
			{   // Sure is a lot of conversion to be done.
				swarm.x[0,b] = commFunctions.rand() % screenWidth; // Pick a random Bee X location.
				swarm.x[1,b] = swarm.x[0,b]; // Syncronize beginning of the next Y bee line with the end of the previous bee if any.
				swarm.y[0,b] = commFunctions.rand() % screenHeight; // Pick a random Bee Y location.
				swarm.y[1,b] = swarm.y[0,b]; // Syncronize beginning of the next Y bee line with the end of the previous bee if any.
				swarm.xv[b] = commFunctions.RAND(7); // Set Bee X Velocity to something. I don't know what RAND(7) does. My test suite was accidentally compiled with C++ not C#
				swarm.yv[b] = commFunctions.RAND(7); // MY guess, is that it says to move a number of pixels not to exceed 7 pizels. in the X and Y direction.
			} // End For loop for initialization.
		} // End Initswarm
		//-----------------------------------------------------

		private bool IsSegmentVisible(int X1, int Y1, int X2, int Y2)
		{
			return 
				!(X1 < 0 || X2 < 0 || Y1 < 0 || Y1 < 0 || 
				X1 > screenWidth || X2 > screenWidth || Y1 > screenHeight || Y2 > screenHeight);
		}

		/// <summary>
		/// Gets a random color into the SwarmPen.Color.<br>
		/// then calls AdjustForeColorBrightnessForBackColor to brighten up invisible colors.
		/// </summary>
		private void SetSwarmPenToRandRgbColor() 
		{ 
			Color rndColor = new Color();
			rndColor = Color.FromArgb(255,Color.FromArgb(commFunctions.randInRange(1,255255254)));
			commFunctions.AdjustForeColorBrightnessForBackColor(ref rndColor,Color.Black,0.5f);
			SwarmPen.Color = rndColor;
		}
		//---------------------------------------------------------------------------------------------------
		private Color GetRandRgbColor() 
		{ 
			Color rndColor = new Color();
			rndColor = Color.FromArgb(255,Color.FromArgb(commFunctions.randInRange(1,255255254)));
			commFunctions.AdjustForeColorBrightnessForBackColor(ref rndColor,Color.Black,0.5f);
			return rndColor;
		}
		//---------------------------------------------------------------------------------------------------
		/// <summary>
		/// draws bees.<br>
		/// </summary>
		/// <param name="targetGraphic"> an instance of the graphics object passed in.</param>
		/// <param name="beesArray">An array of lines or as this program calls them, segments to draw.</param>
		void DrawBees(Graphics targetGraphic, Segment[] beesArray)
		{	
			Pen thePen = new Pen(SwarmPen.Color);
			for (int i = 0; (i < beesArray.Length) && (StopNow == false); i++)  
			{   // Only draw withing bounds.	
				if (IsSegmentVisible(beesArray[i].x1, beesArray[i].y1, beesArray[i].x2, beesArray[i].y2))
				{
					if (GlitterModeWanted)
					{
						thePen.Color = GetRandRgbColor();
					}
					targetGraphic.DrawLine(thePen,beesArray[i].x1, beesArray[i].y1,beesArray[i].x2,beesArray[i].y2);
				}
			}
			thePen.Dispose();
		} // End of DrawBees.
		//---------------------------------------------------------------------
		void EraseBees(Graphics targetGraphic, Segment[] beesArray)
		{	
			for (int i = 0; (i < beesArray.Length) && (StopNow == false); i++)  
			{   	
				// Only draw within bounds.
				if (IsSegmentVisible(beesArray[i].x1, beesArray[i].y1, beesArray[i].x2,beesArray[i].y2))
				{
					targetGraphic.DrawLine(BlackPen, beesArray[i].x1, beesArray[i].y1, beesArray[i].x2,beesArray[i].y2);
				}
			}
		} // End of EraseBees.
		//---------------------------------------------------------------------
		
		/// <summary>
		/// This draws the wasp, and the bees and then moves them, and if needed,<br>
		/// changes the color for the next draw.<br>
		/// See source code for further details.
		/// </summary>
		/// <param name="targetScreen"> the grapics object to draw to.</param>
		public void DrawWaspThenSwarm(Graphics targetGraphic)
		{ 
			try
			{
				if (StopNow == false) // do nothing if caller is disposing.
				{
					int b; // Used to iterate.
					// <=- Wasp -=> 
					// Age the arrays. Gradually copy the contents of the indexes up, until all three values in the arrays = the value at the first index.
					WaspX[2] = WaspX[1]; // I do not know why this is done.
					WaspX[1] = WaspX[0];
					// ----------------------
					WaspY[2] = WaspY[1];
					WaspY[1] = WaspY[0];
					// Accelerate by adding a random accelleration value that is no more than WaspAxxMax.
					WaspXVel += commFunctions.RAND(WaspAccMax);
					WaspYVel += commFunctions.RAND(WaspAccMax);

					// Wasp Speed Limit Checks. WaspVel value never changes and is never negative!
					if (WaspXVel > WaspVel) // Checks foreward velocity, if too fast set to WaspVel.
						WaspXVel = WaspVel;
					else                     // Only test X velocity again if we haven't corrected it yet.
						if (WaspXVel < -WaspVel) // check reverse velocity if too fast set to max reverse velocity.
						WaspXVel = -WaspVel;
					//---------------------------
					if (WaspYVel > WaspVel)
						WaspYVel = WaspVel;
					else                     // Only test Y velocity again if we haven't corrected it yet.
						if (WaspYVel < -WaspVel)
						WaspYVel = -WaspVel;

					// --------------- Move ------------------------
					WaspX[0] = WaspX[1] + WaspXVel;
					WaspY[0] = WaspY[1] + WaspYVel;

					// BOUNCE CHECK FOR WASP!
					// IF leading edge of Wasp as X,Y location gets too too close to edge then reverse it's direction.
					if ((WaspX[0] < Border) || (WaspX[0] > screenWidth - Border - 1)) 
					{
						WaspXVel = -WaspXVel; // Reverse the X velocity.
						WaspX[0] += WaspXVel; // Add in the Velocity.
					}
					if ((WaspY[0] < Border) || (WaspY[0] > screenHeight - Border - 1)) 
					{
						WaspYVel = -WaspYVel;
						WaspY[0] += WaspYVel;
					}
					// Don't let things settle down into a boring pattern, intro a new random velocity that speeds up and slows down in small steps.
					swarm.xv[commFunctions.rand() % Bees] += commFunctions.RAND(3);
					swarm.yv[commFunctions.rand() % Bees] += commFunctions.RAND(3);

					/* <=- Bees -=> */
					for (b = 0;( b < Bees) && (StopNow == false); b++) 
					{
						int distance; // Distance I think is the length of line I think moves in an interval.
						int dx; // Used to save the distances in X Y coordinates.
						int dy; // Used to save the distances.
						// Age the arrays.  Do that same strange shifting of the indexes from first to last till all = value at first index after 3 iterations.
						swarm.x[2, b] = swarm.x[1, b];
						swarm.x[1, b] = swarm.x[0, b];
						swarm.y[2, b] = swarm.y[1, b];
						swarm.y[1, b] = swarm.y[0, b];

						// Accelerate Speed up the wasp and slow down the bee if one of the bees catches up. (Distance = 0);
						dx = WaspX[1] - swarm.x[1, b]; // dx = difference between Wasp X and Bee X. Index 0 = OldX, 1 = CurrentX, 2 = NextX.
						dy = WaspY[1] - swarm.y[1, b]; // Same as above but for Y.
						distance = Math.Abs(dx) + Math.Abs(dy);	// approximation 
						if (distance == 0)
						{
							distance = 1;
						}
						swarm.xv[b] += (dx * BeeAccMax) / distance;
						swarm.yv[b] += (dy * BeeAccMax) / distance;

						// Speed Limit Checks for foreward and reverse sepeed.
						if (swarm.xv[b] > BeeVel) // If over foreward speed limit reduce to speed limit.
							swarm.xv[b] = BeeVel;
						else // If we have corrected X velocity, don't recheck it.
							if (swarm.xv[b] < -BeeVel)
							swarm.xv[b] = -BeeVel;
						// ----------------------------------------------
						if (swarm.yv[b] > BeeVel)
							swarm.yv[b] = BeeVel;
						else // If we have corrected Y velocity, don't recheck it.
							if (swarm.yv[b] < -BeeVel)
							swarm.yv[b] = -BeeVel;
				
						// Move by adding in velocity for this bee.
						swarm.x[0, b] = swarm.x[1, b] + swarm.xv[b];
						swarm.y[0, b] = swarm.y[1, b] + swarm.yv[b];

						// Fill the segment array with the values for the current bee, so we have data on the state of all the bees.
						swarm.segs[b].x1 = swarm.x[0, b];
						swarm.segs[b].y1 = swarm.y[0, b];
						swarm.segs[b].x2 = swarm.x[1, b];
						swarm.segs[b].y2 = swarm.y[1, b];
						if (eraseOldLines) 
						{
							swarm.old_segs[b].x1 = swarm.x[1, b];
							swarm.old_segs[b].y1 = swarm.y[1, b];
							swarm.old_segs[b].x2 = swarm.x[2, b];
							swarm.old_segs[b].y2 = swarm.y[2, b];
						}
					} // End of the for loop for initializing the wasp and bees and moving them.
					
					if (eraseOldLines) 
					{
						// Erase old Wasp position then redraw ini new position.
						if (IsSegmentVisible(WaspOldX[0], WaspOldY[0], WaspOldX[0], WaspOldY[0]))
						{
							targetGraphic.DrawLine(BlackPen,WaspOldX[0], WaspOldY[0], WaspOldX[1], WaspOldY[1]);
						}
						WaspOldX[0] = WaspX[1];
						WaspOldX[1] = WaspX[2];
						WaspOldY[0] = WaspY[1];
						WaspOldY[1] = WaspY[2];
					}
					if (StopNow == false)
					{	
						if (IsSegmentVisible(WaspX[1], WaspY[1], WaspX[2], WaspY[2]))
						{
							targetGraphic.DrawLine(WaspPen, WaspX[1], WaspY[1], WaspX[2],WaspY[2]); // Draw wasp.
						}
						if (DateTime.Now > dtColorCycleSecondsTime) // If its past time to change the color, do so.
						{
							SetSwarmPenToRandRgbColor(); // Get the current chosen color, and set the pen to draw the bees to that color.
							dtColorCycleSecondsTime = DateTime.Now.AddSeconds(ColorCycleSeconds);
						}
					}
					if (eraseOldLines) 
					{   // Erase the bees contained in olg.segs.
						EraseBees(targetGraphic, swarm.old_segs); 
					}
					 DrawBees(targetGraphic, swarm.segs); // Draw the swarm using the new segments.					
			} // End of if statement to do drawing only if StopNow is false.
		}
			catch  
			{
				Cursor.Show();
				throw;
				// Just catch and exit any error. They are all unrecovereable. Exit out.
			} // End catch.
		}	// End of DrawWaspThenSwarm
	}		// Name class definition.
}			// End namespace Insects.
















using System;
using System.Drawing; // Used to get at the Point struct for determining mouse location.
using System.Windows.Forms;
using Insects;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using CommonCode;

namespace SwarmScreenSaver
{
	/// <summary>
	/// This ScreenSaverForm class is a form that is responsible for going full screen with a black background and<br>
	/// then starting the paint event which is where You should put calls to your own screensaver calls.<br>
	/// This form monitors keyboard and mouse to determine when to quit. There is an instance of this form for <br>
	/// all active monitors.<br>
	/// ScreenNumber is a variable that identifys the monitor this form is working with, and is use to get the bounds <br>
	/// aka size of the monitor resolution. It now works with multiple monitors on the same system!. <br>
	/// I know of no other screensaver in source or out of source that has succeded in this.<br>
	/// the Insects class is the workhorse, and does all the graphics routines called in the paint event.
	/// </summary>
	public class ScreenSaverForm : System.Windows.Forms.Form
	{

		// System.Diagnostics.DebuggableAttribute  MyDebuggable = new System.Diagnostics.DebuggableAttribute(false, false);

		/// <summary>
		/// -4 Screen is shutting down!
		/// -3 Error in painting.<br></br>
		/// -2 Unwanted paint done.<br></br>
		/// -1 No Activity.<br></br>
		///  0 Paint pending.<br></br>
		///  1 Our paint is in Paint event.<br></br>
		///  2 Ready to repaint.
		/// </summary>
		private int paintStatus = PaintStates.NoActivity; 

		/// <summary>
		/// Seems we needed more than one status variable <br></br>
		/// for painting. One for tracking paint status and another to indicate <br></br>
		/// that my drawing code in paint  still needs to be executed.
		/// </summary>
		private bool paintMeCalled = false;
		
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Contains the mouse position. If it moves, all instances os ScreenSaverFor are closed and the app is ended.
		/// </summary>
		private Point MouseXY; 

		private CommonCode.CommonFunctions cf = new CommonCode.CommonFunctions();

		/// <summary>
		/// A pair of delegates this form uses to send signals accross threads to the callers.
		/// </summary>
		DonePaintingDelegate DonePaintingDel;
		/// <summary>
		/// Inform caller that the program needs to shut down.
		/// </summary>
		ShutDownDelegate ShuttingDownDel;
	
		/// <summary>
		/// An index into the system array of active screens on the system: Screens.AllScreens[]
		/// so that this screen can capture the monitor size as a bounds object.
		/// </summary>
		private int ScreenNumber; 
		/// <summary>
		/// stopNow starts out as false, when it becomes true due to mouse or key actions, the form closes.
		/// </summary>
		private bool stopNow = false; 

		/// <summary>
		/// Number of swarms per screen to show.
		/// </summary>
		private int SwarmCount;  
		/// <summary>
		/// If true, use built-in control's double buffering. If you get InvalidOperationExceptions turn this off.
		/// </summary>
		private bool useStyleDoubleBuffer;

		/// <summary>
		/// An array of drawing class instances. Each instance handles one swarm and one wasp.<br>
		/// but there may be multiple swarms per screen.
		/// </summary>
		clsInsects[] insects;
		/// <summary>
		/// Memory resident bitmap for manual double bufferiing.
		/// </summary>
		private Bitmap memBitmap;
		/// <summary>
		/// A Grapics object for the the memory bitmap.
		/// </summary>
		private Graphics memGraphic;
		
		/// <summary>
		/// Constructor for this form which is the central point for full screen drawing.
		/// </summary>
		/// <param name="scrn"> Screen number this form instance will handle.</param>
		/// <param name="baseVelocity"> Higher the number the faster swarms travel.</param>
		/// <param name="beeCount"> Count of bees per swarm.</param>
		/// <param name="colorCycleSeconds"> Seconds between swarmcolor change.</param>
		/// <param name="glitterOn"> If true, each bee changes color on each drawing, over-riding colorCycleSeconds.</param>
		/// <param name="swarmCount"> Number of swarms per screen.</param>
		/// <param name="useStyleDoubleBufferArg"> Use built in control's double buffing if true.</param>
		public ScreenSaverForm(int scrn, DonePaintingDelegate donePaintingDel, ShutDownDelegate shutDownDel, int baseVelocity, int beeCount, int colorCycleSeconds, bool glitterOn, int swarmCount, bool useStyleDoubleBufferArg)
		{
			InitializeComponent();
			ScreenNumber = scrn; // Set the Screen Number.
			Bounds = Screen.AllScreens[ScreenNumber].Bounds; 
			SwarmCount = swarmCount;
			useStyleDoubleBuffer = useStyleDoubleBufferArg;
			DonePaintingDel = donePaintingDel;
			ShuttingDownDel = shutDownDel;
			insects = new clsInsects [SwarmCount]; // An array of insect classes to draw a swarm.
			// Initialize the array of insect classes.
			for (int i = 0; i < SwarmCount; i++)
			{
				insects[i] = new clsInsects(baseVelocity, beeCount, colorCycleSeconds, glitterOn);
			}
			SetStyle(ControlStyles.AllPaintingInWmPaint,true); // Name explains it.
			SetStyle(ControlStyles.Opaque,true); // Background is taken care of by DrawImage.
			SetStyle(ControlStyles.UserPaint,true); // if Allpainting is on, so must this.
			SetStyle(ControlStyles.DoubleBuffer, useStyleDoubleBuffer);
			UpdateStyles();
		}
		// -********************************** End Constructor ****************************-

		/// <summary>
		/// Dispose of any componants, dispose any of my dynamic objects, call base.Dispose.
		/// </summary>
		/// <param name="disposing"> I don't have any componants TO dispose but it's there just in case.</param>
		protected override void Dispose( bool disposing )
		{
			if(disposing)
			{
				if (components != null) // There are componants to dispose of.
				{
					components.Dispose();
				}
			}
			if (useStyleDoubleBuffer == false)
			{
				memGraphic.Dispose();
				memBitmap.Dispose();
			}
			base.Dispose( disposing );
		}
		
		/// <summary>
		/// Sets the size of this form to the size of the screen.<br>
		/// then it hides the cursor. <br> 
		/// sets this form to topmost. <br>
		/// Informs the Insects instances of the size of the drawing area for this screen.<br>
		/// Initializes the Insects instances. We need to do this separately. I could not do it in the constructor.<br>
		/// The drawing objects are initialized in this load event, but are actually called in the paint event.
		/// </summary>
		/// <param name="sender"> Unused. </param>
		/// <param name="e"> Unused.</param>
		//LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD 
		//LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD 
		//LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD LOAD 
		// Because it always takes me almost a minute to find this in front of my face!
		private void ScreenSaverForm_Load(object sender, System.EventArgs e)
		{
			if (useStyleDoubleBuffer == false)
			{
				memBitmap = new Bitmap(Bounds.Width,Bounds.Height);
				memGraphic = Graphics.FromImage(memBitmap);
				memGraphic.Clear(Color.Black);
			}
			Cursor.Hide(); // we don't want a mouse over the screensaver.
			TopMost = false; // We want it in front.
			for (int i = 0; i < SwarmCount; i++)
			{
				// useStyleDoubleBuffer is here because generally we will want to erase if this is false.
				insects[i].initSwarm(50,Bounds.Width,Bounds.Height, !useStyleDoubleBuffer); 
			}
			// The Main method uses a while loop to cause this form to paint the graphics and then
		}   // the loop calls Application.DoEvents which causes the screens to show on the monitors.
		// -************************************** End Load Event **********************************

		/// <summary>
		///  This is a centralized area, where the closing of this<br>
		///  form can be initiated from EntryPoints's Main method while<br>
		///  loop, and from inside this form itself.<br>
		///  It sets stopNow = true, and tells all insects instances<br>
		///  to stop drawing now.
		/// </summary>
		public void CloseMe()
		{
			stopNow = true;
			paintStatus = PaintStates.ShuttingDown;
			for (int i = 0; i < SwarmCount; i++)
			{
				insects[i].StopNow = true;
			}
			Close();
		}
		// -************************************************************-
		/// <summary>
		/// OnMouseEvent. Has user moved or clicked the mouse?<br>
		/// if so, set stopNow = true and close the form.<br>
		/// Paint event will stop drawinig when stopNow = true<br>
		/// and inform all insect instances to stop drawing.<br>
		/// Initialize your drawing routines in the load event or earlier.<br>
		/// Your drawing routines should go into the paint event.
		/// </summary>
		/// <param name="sender"> Unused</param>
		/// <param name="e"> Has a count of mouse clicks and mouse X, Y location.</param>
		private void OnMouseEvent(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (!MouseXY.IsEmpty) // Do nothing if mouse location has not been initialized.
			{
				if ((MouseXY != new Point(e.X, e.Y)) || (e.Clicks > 0)) 
					ShuttingDownDel(); // Close this form if the user moves or clicks the mouse.
			}
			MouseXY = new Point(e.X, e.Y); // Get current position of mouse.
		}
		// -**********************************************************************************-

		/// <summary>
		/// ScreenSaverForm_Keydown event. If use presses a key, close this form.
		/// </summary>
		/// <param name="sender"> Unused</param>
		/// <param name="e"> Unused</param>
		private void ScreenSaverForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			ShuttingDownDel(); // Call Entrypoint delegate to shut down all other forms.
		}
		// -***************************************************************************-
		public void PaintMe()
		{
			// Don't paint while we are shutting dow, that would be absurd.
			if (stopNow == false)
			{
				paintMeCalled = true;
				paintStatus = PaintStates.OurPaintPending; // Signal that a paint event has been called.
				Refresh();
			}
			else
			{
				if (paintStatus != PaintStates.ShuttingDown) 
					paintStatus = PaintStates.ShuttingDown;
			}
		}
		// -*************************************************************************-
		/// <summary>
		/// -4 Shutting down.
		/// -3 Error in painting!.<br></br>
		/// -2 Unwanted paint found.<br></br>
		/// -1 No paint activity.<br></br>
		///  0 Paint event is pending.<br></br>
		///  1 A valid paint is in Paint event.<br></br>
		///  2 Ready to repaint.
		/// </summary>
		public int PaintStatus
		{
			get {return paintStatus;}
		}
		// -*************************** End Property ***********************
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// ScreenSaverForm
			// 
			this.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.Black;
			this.CausesValidation = false;
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.ControlBox = false;
			this.Enabled = false;
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ScreenSaverForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ScreenSaver Form";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ScreenSaverForm_KeyDown);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseEvent);
			this.Load += new System.EventHandler(this.ScreenSaverForm_Load);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.ScreenSaverForm_Paint);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseEvent);

		}
		#endregion
		/// <summary>
		/// Here is where you actually do the drawing stuff.<br> 
		/// In this case, EntryPoint's Main method's while loop<br>
		/// calls Refresh to get the screen to draw.<br>
		/// The code takes the Graphics object in the paint argument <br>
		/// and passes it to insects, my drawing class, which draws<br>
		/// and the while loop calls Application.DoEvents to cause<br>
		/// the painting that was done to show on the screens.
		/// </summary>
		/// <param name="e"> Contains a graphics object to paint on.</param>
		private void ScreenSaverForm_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			// IF we are stopping, set the status to stopping and return;
			if (stopNow == true)
			{
				if (paintStatus != PaintStates.ShuttingDown) 
				{
					paintStatus = PaintStates.ShuttingDown; // We are stopping, set paintStatus = shuttingDown.
				}
				return; // We must stop now and return immediately.
			} 

			// Allow no foreign sized paints into my drawing code. 
			// It is preset to expect the size of the screen.
			// Dissallow calls resulting from outside processes.
			// Refresh is the only way to invoke a paint on multiple
			// forms, but causes multiple calls to paint aside from 
			// the correct rectangle size. We only wish to do the one.
			// When there are multiple screens, even in duel mode, the e.ClipRectangle is strange, so just compare the height and width.
			if (e.ClipRectangle.Width != this.Bounds.Width || e.ClipRectangle.Height != this.Bounds.Height || (paintMeCalled == false)) 
			{
				paintStatus = PaintStates.OtherPaint; // Unwanted paint found.
				DonePaintingDel(ScreenNumber); 
				return; 
			} // End ClipRectangle filter.
			// Don't paint if we need to exit.
			
			// Hey! Don't repaint while I'm painting!
			if (paintStatus == PaintStates.PaintInProgress) 
			{
				return;
			}

			paintStatus = PaintStates.PaintInProgress;
			// Now we are ready to paint.
			try 
			{	
				// Draw each swarm in turn while stopNow == false.
				for (int i = 0;( i < SwarmCount) && (stopNow == false); i++)
				{  
					// Pass in a graphic depending on the value of useStyleDoubleBuffer.
					if (useStyleDoubleBuffer)
					{	// Draw straight to the buffer for this graphic.
						SuspendLayout();
						insects[i].DrawWaspThenSwarm(e.Graphics);
						ResumeLayout();
					}
					else	// We don't need to flush when using manual double buffer.
					{		// Draw on the bitmap for manual double buffering.
						insects[i].DrawWaspThenSwarm(memGraphic);
					} 
				} // End of for loop for creating the graphicsPaths for both kinds of buffering.
				// ------------------------------------------------------------------------------
				// Special handling to control the auto-double buffer bug by trying to force a draw.
				if (useStyleDoubleBuffer == false) 
				{					
					// Copy the non-built in buffer to screen.
					e.Graphics.DrawImageUnscaled(memBitmap,0,0);

				}

				// Paint has been successfully done.
				paintStatus = PaintStates.PaintSuccessful;
				paintMeCalled = false;
				DonePaintingDel(ScreenNumber); 
			} // End try
			catch (Exception ex)
			{
				Cursor.Show();
				throw ex;
			} // End catch.
		} // End of Paint event handler.
		// -****************************************-

		/// <summary>
		/// Don't allow background to paint.
		/// </summary>
		/// <param name="pevent"></param>
		protected override void OnPaintBackground(PaintEventArgs pevent) 
		{ 
			//Don't allow the background to paint.
		} 

		/// <summary>
		/// Don't allow resizing.
		/// </summary>
		/// <param name="e"> Unused.</param>
		protected override void OnSizeChanged(EventArgs e) 
		{ 
			// Don't allow resize.
		} 

		protected override void OnForeColorChanged(EventArgs e)
		{
			// Don't let fore change.
		}
		// -**************************************-
	}		// End of class definition.
} // End of namespace SwarmScreenSaver. 

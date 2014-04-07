using System; 
using CommonCode; // Useful commonly used functions such as API calls.
using System.Windows.Forms; // So we can show a messagebox.
using System.Drawing; 
using Insects;
using System.Threading; // To slow the preview mode to normal speed.

namespace SwarmScreenSaver
{
	/// <summary>
	/// This class draws in the Display Properties Screensaver preview window exclusively.
	/// </summary>
	public class MiniPreview
	{
		/// <summary>
		/// Do the mini preview until the mini-preview window vanishes.
		/// </summary>
		/// <param name="argHandle"> a 10 based handle to the Display Properties window.</param>
		/// <param name="baseVelocity"> Starting velocity. User configureable. higher the faster.</param>
		/// <param name="beeCount"> Number of bees per swarm.</param>
		/// <param name="colorCycleSeconds"> Number of seconds before a swarm switches color.</param>
		/// <param name="glitterOn"> If true overrides colorCycleDelaySeconds and each bee has different colors.</param>
		public void DoMiniPreview(int argHandle, int baseVelocity, int beeCount, int colorCycleSeconds, bool glitterOn, bool useStyleDoubleBuffer)
		{
			// Pointer to windows Display Properties window.
			IntPtr ParentWindowHandle = new IntPtr(0); 
			ParentWindowHandle = (IntPtr) argHandle; // Get the pointer to Windows Display Properties dialog. 
			RECT rect = new RECT(); 
			// The Using construct is to make sure all resources used here are cleared in case of unhandled exceptions.
			using(Graphics PreviewGraphic = Graphics.FromHwnd(ParentWindowHandle)) // This is the mini-preview window from the OS. 
			{
				CommonFunctions cf = new CommonFunctions();
				cf.GetClientRectApi(ParentWindowHandle, ref rect); // Get the dimensions and location of the preview window.
			{ // create a local scope to ensure that dt30SEconds goes out of scope as soon as unneeded.
				// The parent window is never ready when preview is chosen, before we try to get the visible state. 
				// So we must wait. I limit my waiting to 30 seconds. 
				DateTime dt30Seconds = DateTime.Now.AddSeconds(30);
				while (cf.IsWindowVisibleApi(ParentWindowHandle) == false)
				{
					if (DateTime.Now > dt30Seconds) return; // If time runs out, exit program.
					Application.DoEvents(); // We don't want to ignore windows, or we might be sorry! :) Respond to events.
				}
			} // After this point the dt30Seconds goes away.
				// Create a bitmap for double buffering.
				Bitmap OffScreenBitmap = new Bitmap(rect.right - rect.left, rect.bottom - rect.top, PreviewGraphic); 
				Graphics OffScreenBitmapGraphic = Graphics.FromImage(OffScreenBitmap); // Create a Graphics object
				OffScreenBitmapGraphic.Clear(Color.Black);
				// Insects is a class that contains an instance of a Wasp, a Swarm and routines for drawing to the 
				// output graphic.
				clsInsects insects = new clsInsects(baseVelocity, beeCount, colorCycleSeconds, glitterOn); 
				// Do some intialization, that was not allowed to happen in the constructor.
				// The 3 means the wasp can't get closer to the edge than 3 pixels without reversing direction.
				insects.initSwarm(3, rect.right - rect.left, rect.bottom - rect.top,true);
				while (cf.IsWindowVisibleApi(ParentWindowHandle) == true) // Now that the window is visible ...
				{
					if (useStyleDoubleBuffer) // Will black out if we are using built in double-buffering.
						OffScreenBitmapGraphic.Clear(Color.Black); 
					insects.DrawWaspThenSwarm(OffScreenBitmapGraphic); // Execute an iteration of drawing the wasp and a swarm of bees.
					Thread.Sleep(50); // Slow down the mini-preview.
					try
					{	// Draw the image created by insects.DrawWaspThenSwrm(OffscreenBitmapGraphic).
						PreviewGraphic.DrawImage(OffScreenBitmap,0,0,OffScreenBitmap.Width,OffScreenBitmap.Height); 

					}
					catch // the most likely reason we get an exception here is because
					{     // the user hits cancel button while drawing to mini-preview.
						break; // Either way we must get out of the program.
					}
					// NOTE: We don't need to clear the bitmap, because windows refreshing does this automatically.
					Application.DoEvents(); // Yield up some idle time and allow events to be processed.
				} // End of while.
				insects.StopNow = true;
				cf = null;
				OffScreenBitmap.Dispose();
				OffScreenBitmapGraphic.Dispose();
				PreviewGraphic.Dispose(); // We are done, trash this.
			} // End of using statement.
		}
	}
}

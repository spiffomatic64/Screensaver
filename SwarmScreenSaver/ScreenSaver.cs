using System;
using System.Windows.Forms;
using System.Diagnostics; // For logging to error log.
using System.Threading;
using CommonCode;

namespace SwarmScreenSaver
{
	/// <summary>
	/// Declare the DonePaitingDelegate signature.
	/// </summary>
	public delegate void DonePaintingDelegate(int screenNumber);
	/// <summary>
	/// Declare the ShutdownDelegate signature.
	/// </summary>
	public delegate void ShutDownDelegate();

	/// <summary>
	/// This class handles the running of the screensaver in fullscreen mode.
	/// </summary>
	/// 
	public class ScreenSaver
	{
		/// <summary>
		/// A variable used to pass the name of the method that informs this class<br>
		/// that the form is finished painting to a ScreenSaverForm as a delagate.
		/// </summary>
		public DonePaintingDelegate DonePaintingDel; 
		/// <summary>
		/// A variable used to pass the name of the method that informs this class<br>
		/// that the app should shut down to a ScreenSaverForm as a delagate.
		/// </summary>
		public ShutDownDelegate ShutDownDel;     

		/// <summary>
		/// A count of the open screens.
		/// </summary>
		int screenCount;
		/// <summary>
		/// False if not shutting down.
		/// </summary>
		bool shuttingDown = false;

		/// <summary>
		/// An array of ScreenSaverForms. To be initialized later.
		/// </summary>
		ScreenSaverForm [] sf;

		bool[] ScreenDonePainting = new bool[Screen.AllScreens.Length];
	
		/// <summary>
		/// This is used to stop the main thread from eating up CPU cycles waiting for an event.
		/// I use a call to WaitHandle.WaitForAny(manualEvents); to wait for the event.
		/// </summary>
		ManualResetEvent[] manualEvents;

		/// <summary>
		/// Constructor creates a manualEvent for each screen and one more for shutting down.
		/// then it initializes the event to untriggered. The delegates below signal the event
		/// as happened, so that the main thread's while loop continues when an event happens.
		/// Additionally, I have utilized ManualEvents to cause my while loop to stop looping
		/// and wait for an event to be fired instead of looping uselessly, the wait stops the
		/// thread until an event fires to respond to. This dramatically lessens the load on the
		/// CPU, that was wasted in looping over and over while waiting for an event. This was 
		/// causing the InvalidOperationEvent error that was taking place because it was tying up
		/// the OS looping all the time. I could not put in an Application.DoEvent() call because
		/// it was interrupting the drawing and causing bad flicker. Instead, now that the main 
		/// thread goes to sleep, I have accomplished the same thing, by freeing up the load the 
		/// screensaver was putting on the computer cpu just looping.
		/// </summary>
		public ScreenSaver()
		{
			manualEvents = new ManualResetEvent[Screen.AllScreens.Length+1];
			for (int i = 0; i <= Screen.AllScreens.Length; i++)
			{
				manualEvents[i] = new ManualResetEvent(false);
			}
		}

		private void CloseAllScreens()
		{
			
			if (screenCount > 0)
			{
				screenCount = 0; 
				for (int i = 0; i < Screen.AllScreens.Length; i++)
				{
					try
					{
						if (sf[i].Created == true && (sf[i].Visible == true)) 
						{
							sf[i].CloseMe(); // Calls on the form to close itself.
							Application.DoEvents(); // Just to make sure the forms do close.
						}
					}
					catch(Exception ex)
					{
						if (ex.Message != "LocalDataStoreSlot storage has been freed.")
							throw ex;
					}
				}
			}
		}
			

		/// <summary>
		/// Just to make sure all screens are shut down.
		/// </summary>
		~ScreenSaver()
		{
			CloseAllScreens();
		}
		
		/// <summary>
		/// This is passed to ScreenSaverForm as a delegate so the form<br>
		/// can call this to let this class know that the form is done painting.
		/// </summary>
		/// <param name="screenNumber"> An index into the Screen.AllScreens array.</param>
		public void DonePainting(int screenNumber)
		{
			lock(ScreenDonePainting)
			{
				ScreenDonePainting[screenNumber] = true;
				// Signal main thread to go.
				manualEvents[screenNumber].Set(); 
			}
		}

		/// <summary>
		/// Sometimes we get foreign paints, we need to reset the screen
		/// as unpainted if this happenes and reset our event handler to
		/// wait.
		/// </summary>
		/// <param name="screenNumber"></param>
		private void ifPaintDoneResetStatuses(int screenNumber)
		{
			if (ScreenDonePainting[screenNumber] == true)	
			{
				ScreenDonePainting[screenNumber] = false;
				manualEvents[screenNumber].Reset();
			}
		}

		/// <summary>
		/// This is passed to ScreenSaverForms as a delegate to be called when the app<br>
		/// should shut down.
		/// </summary>
		public void ShutDown()
		{
			lock(manualEvents)
			{
				shuttingDown = true;
				// Last element is for ShutDown event, signal the shut down.
				manualEvents[Screen.AllScreens.Length].Set(); 
			}
		}

		/// <summary>
		/// This method handles the actual screensaving until it's time to shut down.
		/// </summary>
		/// <param name="baseVelocity"> Speed of wasp and swarm travel.</param>
		/// <param name="beeCount"> Count of bees per swarm.</param>
		/// <param name="colorCycleSeconds"> How many seconds between swarm color changes.</param>
		/// <param name="glitterOn"> If true, bees have their own color. Overrides above parameter.</param>
		/// <param name="swarmsPerScreen"></param>
		public void RunMeTillShutdown(int baseVelocity, int beeCount, int colorCycleSeconds, bool glitterOn, int swarmsPerScreen, bool useStyleDoubleBuffer)
		{
			DonePaintingDel = new DonePaintingDelegate(DonePainting);
			ShutDownDel = new ShutDownDelegate(ShutDown);
			screenCount = Screen.AllScreens.Length; // Get count of screens on system.
			// Use an array of forms for each screen found just to get the array to create instances. 
			sf = new ScreenSaverForm[screenCount]; // We need to create actual instances to fill out the array.
			int i = 0; // Use this for a loop through the array assigning to the sf array, the actual screen saver form instances.
			for (i = 0; i < screenCount;i++)
			{	// Toss the old forms in the array and replace with the Real ScreenSaverForm.
				// Send the screenNo and config values to the ScreenSaverForm constructor.
				sf[i] = new ScreenSaverForm(i,DonePaintingDel, ShutDownDel, baseVelocity, beeCount, colorCycleSeconds, glitterOn, swarmsPerScreen, useStyleDoubleBuffer); 
				sf[i].Show(); 
				sf[i].PaintMe();
			} 
			while (screenCount > 0)
			{
				// Don't waste CPU cycles, looping, instead go to sleep 
				// until an event happen, then process it. No drawing takes 
				// seven seconds so exit because something is wrong.

				WaitHandle.WaitAny(manualEvents, new TimeSpan(0,0,0,2),false);
				if (shuttingDown == true) // If a screen is shutting down, shut down the rest.
				{   // Note that if we are shutting down, we don't need to reset manualEvents.
					CloseAllScreens();
					continue; // Just loop up and exit.
				} // End of if shutting down.
				try
				{	// For each screen check to see if it is done painting and if so, display it by
					// calling Application.DoEvents() then invoke another paint event.
					for (i = 0; i < Screen.AllScreens.Length; i++)
					{	
						switch(sf[i].PaintStatus)
						{
							case PaintStates.ShuttingDown:
								// Application.DoEvents();
								continue;
								break;

								// Error in paint!! Call it again!
							case PaintStates.PaintError: 
								ifPaintDoneResetStatuses(i);
								Application.DoEvents();
								sf[i].PaintMe();
								break;

								// Outside paint.
							case PaintStates.OtherPaint:
								ifPaintDoneResetStatuses(i);
								break;
								
							case PaintStates.NoActivity: 
							break;
							// Paint called but not in paint yet.
							case PaintStates.OurPaintPending: 
							case PaintStates.PaintInProgress: 
							break; // Do nothing.

							case PaintStates.PaintSuccessful: // Ready to paint.
								ifPaintDoneResetStatuses(i);
								Application.DoEvents(); // Show the graphics.
								sf[i].PaintMe(); // Invoke a paint event.
								break;
						} // End switch.
					} // End for.
				} // end try.
				catch // All errors are accounted as irrecovereable due to GDI+ committing hari-kari on errors.
				{
					Cursor.Show();
					throw;
				}
			} // End of While.
		} // End of RunTillShutdown.
	} // End of class ScreenSaver definition.
} // End of namespace.

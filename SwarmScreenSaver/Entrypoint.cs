using System;
using System.Windows.Forms; // This is so we can put up a messagebox.
using CommonCode.XmlConfig;
// using System.Diagnostics;
//using System.Reflection;

namespace SwarmScreenSaver
{

	

/// <summary>
/// Entrypoint.cs is a class file that is the main entry point for the entire program.<br></br>
/// It handles Configuration requests, creates and runs instances of Mini-preview and ScreenSaver<br></br>
/// which does the actual Screensaving. To install this program, run setup to a folder <br></br>
/// of your making and then rename Swarm.exe to swarm.scr and move it to your System32<br></br>
/// subdirectory and then right-click on the desktop and choose properties and<br></br>
/// select swarm and hit OK. Enjoy your new Screensaver. If you have source code included<br></br>
/// you should see a zip file or folder called CodeCommentReport. Inside is a web page version <br></br>
/// of the documentation for the source code and info on use of the source as a template for<br></br>
/// your own screensaver projects. Written in Visual C# from Visual Studio 2003 Universal.
/// </summary>

	public class EntryPoint 
	{

//		public static void OnExcept(Object sender, UnhandledExceptionEventArgs args) 
//		{
//			Application.Exit();
//		}

		private static void ParseArgsToPrefixAndArgInt( string[] args, out string argPrefix, out int argHandle)
		{
			string curArg;
			char[] SpacesOrColons =  {' ', ':'};

			switch(args.Length)
			{
				case 0: // Nothing on command line, so just start the screensaver.
					argPrefix = "/s";
					argHandle = 0;
					break;
				case 1:
					curArg = args[0];
					argPrefix = curArg.Substring(0,2);
					curArg = curArg.Replace(argPrefix,""); // Drop the slash /? part.
					curArg = curArg.Trim(SpacesOrColons); // Remove colons and spaces.
					argHandle = curArg == "" ? 0 : int.Parse(curArg); // if empty return zero. else get handle.
					break;
				case 2:
					argPrefix = args[0].Substring(0,2);
					argHandle = int.Parse(args[1].ToString());
					break;
				default:
					argHandle = 0;
					argPrefix = "";
					break;

			}
		}

	/// <summary>
	/// Main is the entry point.
	/// </summary>
	/// <param name="args">
	/// <description>
	/// A string array that contains the arguments passed in on the command line. For more info see the source code.
	/// </description>
	/// </param>
		//[STAThread] 
		static void Main(string[] args) 
		{
			//AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnExcept);
			// These are just some variables to old config and argument data.
			int baseVelocity;
			int beeCount;
			int colorCycleSeconds;
		    bool glitterOn;
			int swarmsPerScreen;
			bool useStyleDoubleBuffer;
			string argPrefix;
			int argHandle; // Will be 0 if no number found.
			XmlConfigSaver xm = new CommonCode.XmlConfig.XmlConfigSaver();
			// read xml config ini file, if missing, create and return default values.
			xm.readConfigXml(out baseVelocity, out beeCount, out colorCycleSeconds, out glitterOn, out swarmsPerScreen, out useStyleDoubleBuffer);
			xm = null; // We don't need to get the xml config class for now.
			if (args.Length > 2) 
			{
				MessageBox.Show("Too many arguments on the command line.");
				return;
			}
			ParseArgsToPrefixAndArgInt(args, out argPrefix, out argHandle);

			try
			{
				switch (argPrefix)
				{
					case "/a": // password dialog desired. I'll exit program instead of risking locking someone out due to a bug
						break; // in my code, or a change in the OS code.

					case "/c": // Show configuration screen.
						System.Windows.Forms.Application.Run(new ConfigForm(argHandle));						
						break; // Break goes to a point where we exit the program.
						// Start the screensaver in mini-preview or full screen mode depending on the number of args passsed in.

					case "/p": 
						if (argHandle == 0) goto case "/s"; // No handle found, do a fullscreen screensaver.
						else
						{
							MiniPreview mpTemp = new MiniPreview();
							mpTemp.DoMiniPreview(argHandle, baseVelocity, beeCount, colorCycleSeconds, glitterOn, useStyleDoubleBuffer);
							mpTemp = null; // This does not execute until preview is closed.
						}
						break; // Exit out of the main loop and the program closes.

					case "/s": // Start screensaver.
						ScreenSaver screenSaver = new ScreenSaver();
						screenSaver.RunMeTillShutdown( baseVelocity, beeCount, colorCycleSeconds, glitterOn, swarmsPerScreen, useStyleDoubleBuffer);
						screenSaver = null; // This doesn't execute until shutdown is done.	
						break;
					default:   // If something strange is passed in, just ignore and exit.
						break;
				} // Switch end.
			} // End of try statement.
			
			catch
			{	
				// Just exit because this is a screensaver for unattended operation. We don't want a message
				// and we want to exit if anything goes wrong.
			}
		} // Main End.

	} // Class End.
} // Namespace end.

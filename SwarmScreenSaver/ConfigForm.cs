using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using CommonCode;
using CommonCode.XmlConfig;


namespace SwarmScreenSaver
{
	
	/// <summary>
	/// This is the configuration form. Remember to save all changes<br>
	/// or they will not take. Use the X to close the window.
	/// </summary>
	public class ConfigForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		/// <summary>
		/// Used to display the count of bees.
		/// </summary>
		private System.Windows.Forms.TextBox BeecountText;
		private System.Windows.Forms.Label label2;
		/// <summary>
		/// Displays the Base velocity of the wasp and swarm. Wasp velocity is 100 percent of this value.<br>
		/// Bees is 92 percent of Wasp Velocity.
		/// </summary>
		private System.Windows.Forms.TextBox BaseVelocityText;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label VelDefaultlabel;
		private System.Windows.Forms.Label ColorCycleSecondsDefaultLabel;
		private System.Windows.Forms.TextBox ColorCycleSecondsText;
		private System.Windows.Forms.CheckBox GlitterModeWantedChk;
		private System.Windows.Forms.Label BeeCountDefaultLabel;
		/// <summary>
		/// This is the configuraton form where you can set general Velocity, Number of Bees,<br>
		/// how many seconds before swarm color is changed and if you want GlitterMode or not.
		/// </summary>
		
		// -- Begin the API declarations. They seem to be needed in each class they are used. :-(

		/// <summary>
		/// This is an InPtr that takes the handle to the window when the user hits settings.<br> 
		/// I pass the results of this to an api call that ensures that the window is open.<br> 
		/// When it closes, this program also closes down.
		/// </summary>
		private IntPtr cParentWindowHandle = new IntPtr(0);
		
		// Holds the user configureable values for validation and sending on the user saving the changes.
		/// <summary>
		/// Holds the speed of movement.
		/// </summary>
		int CurrentVelocity;
		/// <summary>
		/// The count of bees that is displayed. These values may or may not be active/saved.
		/// </summary>
		int CurrentBees;
		/// <summary>
		/// Number of seconds before the swarm color changes.
		/// </summary>
		int CurrentColorCycleSeconds;
		/// <summary>
		/// Contains True if user wants GlitterMode else false. Default false. Glitter mode is random colors per bee.<br>
		/// I'll get a headache trying. :)
		/// </summary>
		bool CurrentGlitterModeWanted;
		/// <summary>
		/// Is the number of swarms per screen.
		/// </summary>
		int CurrentSwarmsPerScreen;
		
		bool CurrentUseStyleDoubleBuffer;

		/// <summary>
		/// This class contains a routine to check if the Display Properties
		/// window is still up.
		/// </summary>
		private CommonCode.CommonFunctions cf = new CommonFunctions();
		/// <summary>
		/// This class reads and writes the xml configuration file.
		/// </summary>
		private XmlConfigSaver xmlConfig = new XmlConfigSaver();

		private System.Windows.Forms.Button saveButton;
		private System.Windows.Forms.Button GetDefaultsButton;
		private System.Windows.Forms.Button GetLastSavedConfigButton;
		private System.Windows.Forms.Timer CheckToCloseTimer;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox SwarmsPerScreenText;
		private System.Windows.Forms.CheckBox UseStyleDoubleBufferChk;
		private System.ComponentModel.IContainer components;
		
		/// <summary>
		/// Constructor that does several things ... <br>
		/// Intitializes any componants.<br>
		/// parses out the number that is the handle to the windows screensaver dialog box. This is not<br>
		/// the same as THIS form, but is the form that bring up this form by sending a /c:##### to this program<br>
		/// on the command line. ###### represents a 10 base number that is a handle to the dialog box that started this app.<br>
		/// Put this parsed number into cParentWindowHandle.<br>
		/// Start the timer to check for the window being visible. See source code for more headaches, er I mean details.
		/// </summary>
		/// <param name="args"> An array of strings that contains the handle to the configuration window<br>
		/// that we need to check using the timer event for visibility. When it's not visible we close the form.</param>
		public ConfigForm(int IntArg)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			TopMost = true; // this might be behind a window if a user did swarm /c.
		
			// In configure mode from windows a handle starts at the 3rd character. Example: /c:345678
			if ( IntArg != 0)	// does a parent window handle exist?.
			{
				cParentWindowHandle = (IntPtr)  IntArg;
				
				// start the timer to checking if the parent window has closed, and if so, close this app.
				this.CheckToCloseTimer.Enabled = true;
			}
		}

		/// <summary>
		/// Clean up any resources being used by calling an override of dispose then call base.Dispose().
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.BeecountText = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.BaseVelocityText = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.ColorCycleSecondsText = new System.Windows.Forms.TextBox();
			this.VelDefaultlabel = new System.Windows.Forms.Label();
			this.BeeCountDefaultLabel = new System.Windows.Forms.Label();
			this.ColorCycleSecondsDefaultLabel = new System.Windows.Forms.Label();
			this.saveButton = new System.Windows.Forms.Button();
			this.GetDefaultsButton = new System.Windows.Forms.Button();
			this.GetLastSavedConfigButton = new System.Windows.Forms.Button();
			this.CheckToCloseTimer = new System.Windows.Forms.Timer(this.components);
			this.GlitterModeWantedChk = new System.Windows.Forms.CheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.SwarmsPerScreenText = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.UseStyleDoubleBufferChk = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// BeecountText
			// 
			this.BeecountText.Location = new System.Drawing.Point(124, 30);
			this.BeecountText.Name = "BeecountText";
			this.BeecountText.Size = new System.Drawing.Size(96, 20);
			this.BeecountText.TabIndex = 1;
			this.BeecountText.Text = "";
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(4, 30);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(124, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "# of bees(50 min):";
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(7, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(105, 16);
			this.label2.TabIndex = 2;
			this.label2.Text = "Velocity. Min 2.";
			// 
			// BaseVelocityText
			// 
			this.BaseVelocityText.Location = new System.Drawing.Point(109, 8);
			this.BaseVelocityText.Name = "BaseVelocityText";
			this.BaseVelocityText.Size = new System.Drawing.Size(96, 20);
			this.BaseVelocityText.TabIndex = 0;
			this.BaseVelocityText.Text = "";
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.Location = new System.Drawing.Point(3, 55);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(131, 16);
			this.label3.TabIndex = 3;
			this.label3.Text = "ColorCycleSeconds:";
			// 
			// ColorCycleSecondsText
			// 
			this.ColorCycleSecondsText.Location = new System.Drawing.Point(136, 53);
			this.ColorCycleSecondsText.Name = "ColorCycleSecondsText";
			this.ColorCycleSecondsText.Size = new System.Drawing.Size(96, 20);
			this.ColorCycleSecondsText.TabIndex = 4;
			this.ColorCycleSecondsText.Text = "";
			// 
			// VelDefaultlabel
			// 
			this.VelDefaultlabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.VelDefaultlabel.Location = new System.Drawing.Point(207, 11);
			this.VelDefaultlabel.Name = "VelDefaultlabel";
			this.VelDefaultlabel.Size = new System.Drawing.Size(68, 16);
			this.VelDefaultlabel.TabIndex = 5;
			this.VelDefaultlabel.Text = "Default: 12.";
			// 
			// BeeCountDefaultLabel
			// 
			this.BeeCountDefaultLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.BeeCountDefaultLabel.Location = new System.Drawing.Point(222, 33);
			this.BeeCountDefaultLabel.Name = "BeeCountDefaultLabel";
			this.BeeCountDefaultLabel.Size = new System.Drawing.Size(76, 16);
			this.BeeCountDefaultLabel.TabIndex = 6;
			this.BeeCountDefaultLabel.Text = "Default: 144.";
			// 
			// ColorCycleSecondsDefaultLabel
			// 
			this.ColorCycleSecondsDefaultLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.ColorCycleSecondsDefaultLabel.Location = new System.Drawing.Point(235, 56);
			this.ColorCycleSecondsDefaultLabel.Name = "ColorCycleSecondsDefaultLabel";
			this.ColorCycleSecondsDefaultLabel.Size = new System.Drawing.Size(60, 16);
			this.ColorCycleSecondsDefaultLabel.TabIndex = 7;
			this.ColorCycleSecondsDefaultLabel.Text = "Default: 1";
			// 
			// saveButton
			// 
			this.saveButton.Location = new System.Drawing.Point(224, 129);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(72, 24);
			this.saveButton.TabIndex = 8;
			this.saveButton.Text = "Save These";
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// GetDefaultsButton
			// 
			this.GetDefaultsButton.Location = new System.Drawing.Point(8, 129);
			this.GetDefaultsButton.Name = "GetDefaultsButton";
			this.GetDefaultsButton.Size = new System.Drawing.Size(80, 24);
			this.GetDefaultsButton.TabIndex = 9;
			this.GetDefaultsButton.Text = "Get Defaults";
			this.GetDefaultsButton.Click += new System.EventHandler(this.GetDefaultsButton_Click);
			// 
			// GetLastSavedConfigButton
			// 
			this.GetLastSavedConfigButton.Location = new System.Drawing.Point(88, 129);
			this.GetLastSavedConfigButton.Name = "GetLastSavedConfigButton";
			this.GetLastSavedConfigButton.Size = new System.Drawing.Size(136, 24);
			this.GetLastSavedConfigButton.TabIndex = 10;
			this.GetLastSavedConfigButton.Text = "Get Last Saved Config";
			this.GetLastSavedConfigButton.Click += new System.EventHandler(this.GetLastSavedConfigButton_Click);
			// 
			// CheckToCloseTimer
			// 
			this.CheckToCloseTimer.Tick += new System.EventHandler(this.CheckToCloseTimer_Tick);
			// 
			// GlitterModeWantedChk
			// 
			this.GlitterModeWantedChk.CausesValidation = false;
			this.GlitterModeWantedChk.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.GlitterModeWantedChk.Location = new System.Drawing.Point(167, 104);
			this.GlitterModeWantedChk.Name = "GlitterModeWantedChk";
			this.GlitterModeWantedChk.Size = new System.Drawing.Size(128, 16);
			this.GlitterModeWantedChk.TabIndex = 11;
			this.GlitterModeWantedChk.Text = "GlitterMode Wanted";
			// 
			// label4
			// 
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label4.Location = new System.Drawing.Point(3, 76);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(112, 16);
			this.label4.TabIndex = 12;
			this.label4.Text = "Swarms/Screen";
			// 
			// SwarmsPerScreenText
			// 
			this.SwarmsPerScreenText.Location = new System.Drawing.Point(120, 76);
			this.SwarmsPerScreenText.Name = "SwarmsPerScreenText";
			this.SwarmsPerScreenText.Size = new System.Drawing.Size(48, 20);
			this.SwarmsPerScreenText.TabIndex = 13;
			this.SwarmsPerScreenText.Text = "";
			// 
			// label5
			// 
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label5.Location = new System.Drawing.Point(176, 80);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(112, 16);
			this.label5.TabIndex = 14;
			this.label5.Text = "Default: 1 (1 .. 100)";
			// 
			// UseStyleDoubleBufferChk
			// 
			this.UseStyleDoubleBufferChk.Checked = true;
			this.UseStyleDoubleBufferChk.CheckState = System.Windows.Forms.CheckState.Checked;
			this.UseStyleDoubleBufferChk.Location = new System.Drawing.Point(8, 104);
			this.UseStyleDoubleBufferChk.Name = "UseStyleDoubleBufferChk";
			this.UseStyleDoubleBufferChk.Size = new System.Drawing.Size(144, 16);
			this.UseStyleDoubleBufferChk.TabIndex = 15;
			this.UseStyleDoubleBufferChk.Text = "Use style double buffer.";
			// 
			// ConfigForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(304, 160);
			this.Controls.Add(this.UseStyleDoubleBufferChk);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.SwarmsPerScreenText);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.GlitterModeWantedChk);
			this.Controls.Add(this.BeecountText);
			this.Controls.Add(this.GetLastSavedConfigButton);
			this.Controls.Add(this.GetDefaultsButton);
			this.Controls.Add(this.saveButton);
			this.Controls.Add(this.ColorCycleSecondsText);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.BaseVelocityText);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.VelDefaultlabel);
			this.Controls.Add(this.BeeCountDefaultLabel);
			this.Controls.Add(this.ColorCycleSecondsDefaultLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ConfigForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Swarmscreensaver configuration";
			this.Load += new System.EventHandler(this.ConfigForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Get the current Configuration.<br>
		/// Display the configuration.
		/// </summary>
		/// <param name="sender"> Unused</param>
		/// <param name="e"> Unused</param>
		private void ConfigForm_Load(object sender, System.EventArgs e)
		{
			// We get the configures. On return, the variables contain the configuration that is running.
			xmlConfig.readConfigXml(out CurrentVelocity, out CurrentBees, out CurrentColorCycleSeconds, out CurrentGlitterModeWanted,out CurrentSwarmsPerScreen, out CurrentUseStyleDoubleBuffer);
			// Set the text boxes up.
			SetTextBoxesFromVariables(); // These would be the local variables
		}
		// ----------------------------------------------------------------
		/// <summary>
		/// Copy the values in the variables into the text boxes and the checkbox.
		/// </summary>
		private void SetTextBoxesFromVariables() 
		{
			BaseVelocityText.Text = CurrentVelocity.ToString();
			BeecountText.Text = CurrentBees.ToString();
			ColorCycleSecondsText.Text = CurrentColorCycleSeconds.ToString();
			GlitterModeWantedChk.Checked = CurrentGlitterModeWanted;
			SwarmsPerScreenText.Text = CurrentSwarmsPerScreen.ToString();
			UseStyleDoubleBufferChk.Checked = CurrentUseStyleDoubleBuffer;
		}
		/// <summary>
		/// Validate the data to be saved. 
		/// </summary>
		/// <returns>true if valid data, else false.</returns>
		private bool ValidateData() // Validate the proposed data when the user hits the save these button.
		{
			bool r = true;
			try
			{
				CurrentVelocity = Convert.ToInt32(BaseVelocityText.Text,10);
				CurrentBees = Convert.ToInt32(BeecountText.Text,10);
				CurrentColorCycleSeconds = Convert.ToInt32(ColorCycleSecondsText.Text,10);
				CurrentGlitterModeWanted = GlitterModeWantedChk.Checked;
				CurrentSwarmsPerScreen = Convert.ToInt32(SwarmsPerScreenText.Text,10);
				CurrentUseStyleDoubleBuffer = UseStyleDoubleBufferChk.Checked;
				// Even tho this is an int, we don't want to go more than the max of a short.
				if (CurrentVelocity < 1 || CurrentVelocity > short.MaxValue) 
				{
					MessageBox.Show("Current Velocity must be between 1 and " + short.MaxValue.ToString());
					r = false;
				}
				if (this.CurrentBees < 25) 
				{
					MessageBox.Show("Bee count must be 25 or more.");
					r = false;
				}
				if (CurrentColorCycleSeconds < 1) // Must wait at least one second before changing colors.
				{
					MessageBox.Show("ColorCycleSeconds must be no less than 1!");
					r = false;
				}
				
				if (CurrentSwarmsPerScreen < 1 || CurrentSwarmsPerScreen > 100)
				{
					MessageBox.Show("CurrentSwarmsPerScreen must be between 1 and 100");
					r = false;
				}

				return r;
			} // End try block.
			catch
			{
				MessageBox.Show("Invalid configuration data. Try again or close screen.");
				r = false;
			}
			return r;
		}

		/// <summary>
		/// If the data is valid, call cf.SetConfig passing in the values to be saved into the registry.<br>
		/// exiting this form after saving restarts the app with the saved values.
		/// </summary>
		/// <param name="sender"> Unused</param>
		/// <param name="e"> Unused</param>
		private void saveButton_Click(object sender, System.EventArgs e)
		{
			if (ValidateData() == true)
			{
				xmlConfig.writeConfigXml( CurrentVelocity, CurrentBees, CurrentColorCycleSeconds, CurrentGlitterModeWanted,CurrentSwarmsPerScreen, CurrentUseStyleDoubleBuffer);
				MessageBox.Show("Configuration Saved. To close form, hit the X");
			}

		}
		
		/// <summary>
		/// Sets the text boxes to the default values. You must save these and close form for them to take effect.
		/// </summary>
		/// <param name="sender"> Unused</param>
		/// <param name="e"> Unused</param>
		private void GetDefaultsButton_Click(object sender, System.EventArgs e)
		{
			BaseVelocityText.Text = "12";
			BeecountText.Text = "144";
			ColorCycleSecondsText.Text = "1";
			GlitterModeWantedChk.Checked = false;
			CurrentSwarmsPerScreen = 1;
			CurrentUseStyleDoubleBuffer = true;
		}

		// 
		/// <summary>
		/// Refetch the values from the registry and display them.
		/// </summary>
		/// <param name="sender"> Unused.</param>
		/// <param name="e"> Unused.</param>
		private void GetLastSavedConfigButton_Click(object sender, System.EventArgs e)
		{
			xmlConfig.readConfigXml(out CurrentVelocity, out CurrentBees, out CurrentColorCycleSeconds, out CurrentGlitterModeWanted, out CurrentSwarmsPerScreen, out CurrentUseStyleDoubleBuffer);
			SetTextBoxesFromVariables();		
		}

		
		/// <summary>
		/// If the Windows dialog box vanishes, close this window. User could have closed it. This is a timer event handler.
		/// </summary>
		/// <param name="sender"> Unused.</param>
		/// <param name="e"> Unused.</param>
		private void CheckToCloseTimer_Tick(object sender, System.EventArgs e)
		{
			if (cf.IsWindowVisibleApi(cParentWindowHandle) == false) 
			{
				CheckToCloseTimer.Enabled = false; // Turn off the timer.
				Close(); // Close the form.
			} // End if.
		} // end timer eventhandler.
	} // End class definition.
} // end namespace.

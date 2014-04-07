using System;
using System.Windows.Forms; // So I can get at the application object to get a path.
using System.IO;
using System.Data;

namespace CommonCode.XmlConfig
{
	/// <summary>
	/// A class
	/// </summary>
	public class XmlConfigSaver
	{
		/// <summary>
		/// Appends to the end of the file to create the xml config file name.
		/// Swarm.scr creates Swarm.scr.config.xml
		/// </summary>
		private const string xmlExtention = ".config.xml";
		private int ExceptionRetries = 0;


		private int baseVelocity = 12;
		private int beeCount = 144;
		private int colorCycleSeconds = 1;
		private bool glitterOn = false;
		private int swarmsPerScreen = 1;
		// Use built in double buffering.
		private bool useStyleDoubleBuffer = true; 
		// --- End Property Variables ---

		/// <summary>
		/// I use a dataset to do the writing of the information.
		/// </summary>
		DataSet dsConfigDataSet;
		DataTable dtConfigDataTable;
		// ------- End variables --------
	

		// ------ Start property Definitions ---
		/// <summary>
		/// Some properties incase i need them.
		/// </summary>
		public int BaseVelocity
		{
			get { return baseVelocity; }
		}
		//
		public int BeeCount
		{
			get { return beeCount; }
		}
		//
		public int ColorCycleSeconds
		{
			get { return colorCycleSeconds; }
		}
		//
		public bool GlitterOn
		{
			get { return glitterOn; }
		} 

		public int SwarmsPerScreen
		{
			get { return swarmsPerScreen; }
		}

		public bool UseStyleDoubleBuffer
		{
			get { return useStyleDoubleBuffer; }
		}

		// --- End Prperty Definitions------

		/// <summary>
		/// In the event that the file is missing, we need to recreate the 
		/// schema of the xml preparatory to writing the data.
		/// </summary>
		private void BuildSchema()
		{
			dtConfigDataTable = new DataTable("ConfigDataTable");
			dtConfigDataTable.Columns.Add("baseVelocity", typeof(int));
			dtConfigDataTable.Columns.Add("beeCount",typeof(int));
			dtConfigDataTable.Columns.Add("colorCycleSeconds",typeof(int));
			dtConfigDataTable.Columns.Add("glitterOn", typeof(bool));
			dtConfigDataTable.Columns.Add("swarmsPerScreen", typeof(int));
			dtConfigDataTable.Columns.Add("useStyleDoubleBuffer", typeof(bool));
			dsConfigDataSet.Tables.Add(this.dtConfigDataTable); 
		}

		/// <summary>
		/// Used to write to the config xml.
		/// </summary>
		/// <param name="baseVelocityArg"> Base program velocity, higher the number the faster.</param>
		/// <param name="beeCountArg"> Bees per swarm.</param>
		/// <param name="colorCycleSecondsArg"> Seconds between change of color, overridden by glitterOnArg</param>
		/// <param name="glitterOnArg"> Does every bee have a different color every time it's drawn?.</param>
		/// <param name="swarmsPerScreenArg"> Number of swarms per screen.</param>
		/// <param name="useStyleDoubleBufferArg"> Default is true; Use the runtime's built in double buffering.</param>
		public void writeConfigXml(int baseVelocityArg, int beeCountArg, int colorCycleSecondsArg, bool glitterOnArg, int swarmsPerScreenArg, bool useStyleDoubleBufferArg) 
		{
			dsConfigDataSet = new DataSet();
			BuildSchema();
			DataRow r = dsConfigDataSet.Tables[0].NewRow();
			r["baseVelocity"] = baseVelocity = baseVelocityArg;
			r["beeCount"] = beeCount = beeCountArg;
			r["colorCycleSeconds"] = colorCycleSeconds = colorCycleSecondsArg;
			r["glitterOn"] = glitterOn = glitterOnArg;
			r["swarmsPerScreen"] = swarmsPerScreen = swarmsPerScreenArg;
			r["useStyleDoubleBuffer"] = useStyleDoubleBuffer = useStyleDoubleBufferArg;
			dsConfigDataSet.Tables["ConfigDataTable"].Rows.Add(r);
			dsConfigDataSet.WriteXml(Application.ExecutablePath + xmlExtention);
			r = null;
			dtConfigDataTable.Dispose();
			dsConfigDataSet.Dispose();
		}
	
		/// <summary>
		/// If this routine finds the file missing, it re-creates it.
		/// and then returns default values.
		/// </summary>
		/// <param name="baseVelocityArg"></param>
		/// <param name="beeCountArg"></param>
		/// <param name="colorCycleSecondsArg"></param>
		/// <param name="glitterOnArg"></param>
		/// <paramref name=" swarmsPerScreenArg"/> Number of swarms per screen.</param>
		/// <param name="useStyleDoubleBufferArg"> Default true. Use the runtime's built in double buffering.</param>
		public void readConfigXml(out int baseVelocityArg, out int beeCountArg, out int colorCycleSecondsArg, out bool glitterOnArg, out int swarmsPerScreenArg, out bool useStyleDoubleBufferArg) 
		{	
			// If it doesn't exist, create it.
			if (File.Exists(Application.ExecutablePath + xmlExtention) == false)
			{
				writeConfigXml(12,144,1,false,1,true);
				baseVelocityArg = 12;
				beeCountArg = 144;
				colorCycleSecondsArg = 1;
				glitterOnArg = false;
				swarmsPerScreenArg = 1;
				useStyleDoubleBufferArg = true;
			}
			else
			{
				try
				{
					dsConfigDataSet = new DataSet();
					dsConfigDataSet.ReadXml(Application.ExecutablePath + xmlExtention);
					DataRow r = dsConfigDataSet.Tables[0].Rows[0]; // Always only one row.
					baseVelocity = int.Parse(r["baseVelocity"].ToString());
					beeCount = int.Parse(r["beeCount"].ToString());
					colorCycleSeconds = int.Parse(r["colorCycleSeconds"].ToString());
					glitterOn = bool.Parse(r["glitterOn"].ToString());
					swarmsPerScreen = int.Parse(r["swarmsPerScreen"].ToString());
					useStyleDoubleBuffer = bool.Parse(r["useStyleDoubleBuffer"].ToString());
					dsConfigDataSet.Dispose();
					baseVelocityArg = baseVelocity;
					beeCountArg = beeCount;
					colorCycleSecondsArg = colorCycleSeconds;
					glitterOnArg = glitterOn;
					swarmsPerScreenArg = swarmsPerScreen;
					useStyleDoubleBufferArg = useStyleDoubleBuffer;
				} // End try.
				catch(Exception ex)
				{
					if (ExceptionRetries++ < 1) // Don't try more than once before throwing.
					{
						// Likely, there was a new config field added so call the write.
						writeConfigXml(12,144,1,false,1,true); 
						baseVelocityArg = 12;
						beeCountArg = 144;
						colorCycleSecondsArg = 1;
						glitterOnArg = false;
						swarmsPerScreenArg = 1;
						useStyleDoubleBufferArg = true;
					}
					else 
						throw ex;
				} // End catch.
			} // End else branch of ReadConfigXml.
		} // End of readConfigXml.
	} // End class.
} // End namespace.

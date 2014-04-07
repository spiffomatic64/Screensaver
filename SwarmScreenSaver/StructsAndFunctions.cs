using System;
using System.IO;
using System.Runtime.InteropServices; // So API functions can be called.
using Microsoft.Win32;
//using System.Diagnostics; // Uncomment out for debugging.
using System.Drawing;


namespace CommonCode
{
	/// <summary>
	/// Summary description for StructsAndFunctions.cs which is actually a class called CommonFunctions.<br></br>
	/// Actually this is under the CommonCode namespace. This namespace contains structures commonly used and the<br></br>
	/// class CommonFunctions contains functions that are commonly needed, such as various random number related<br></br>
	/// and color adjustment functions. I would have liked to use private for most of these structures but the<br></br>
	/// C# namespace is evil, it does not allow me to make private structure even tho I don't need any but the<br></br>
	/// RECT struct outside the file. Bah!
	/// </summary>

	/// <summary>
	/// Various paint states ....<br></br>
	/// ShuttingDown,<br></br>
	/// PaintError,<br></br>
	/// OtherPaint, Means another process did the paint.<br></br>
	/// NoActivity, Paint not called.<br></br>
	/// OurpaintPending, Paint event invoked but not yet in progress.<br></br>
	/// PaintInProgress, Our drawing code executing.<br></br>
	/// PaintSuccessful.
	/// </summary>
	public struct PaintStates
	{
		public const int 
		ShuttingDown = -4,
		PaintError = -3,
		OtherPaint = -2,
		NoActivity = -1,
		OurPaintPending = 0,
		PaintInProgress = 1,
		PaintSuccessful = 2;
	}
	
	/// <summary>
	/// An instance of this struct is passed GetClientRect API to get the size of the preview window.<br></br>
	/// A struct of ints that contains the location and size info for a screen region.<br></br>
	/// It is passed to GetClientRect API call as a ref variable so the members can be filled in.
	/// </summary>
	public struct RECT
	{
		public int left; 
		public int top; 
		public int right;
		public int bottom;

		public RECT(int l, int t, int r, int b)
		{
			left = l;
			top = t;
			right = r;
			bottom = b;
		}
	}
	/// <summary>
	/// A struct to represent a line via 4 ints. x1, y1 for the starting point and x2, y2 for the ending point.<br></br>
	/// this represents bees and the wasp too.
	/// </summary>
	public struct Segment 
	{
		public int x1; // Starting X and Y point for line
		public int y1;
		public int x2; // Ending X and Y point for line.
		public int y2;
	} 

	/// <summary>
	/// A structure to represent all the bees in a swarm. segs and old_segs (unused) refer to bee lines at xy start and xy end points.<br></br>
	/// x and y are 2 dimentional int arrays to represent the starting and ending points of bees to be erased or drawn.
	/// </summary>
	public struct SwarmStruct // This struct holds data for all the bees in the swarm.
	{
		public Segment[]	segs;		// bee lines 
		public Segment[]	old_segs;	// old bee lines // I"m nost terribly sure why the programmer kept so many segment copies.
		public int[,] 		x;			// Bee X positions. y[TIME][bee#] Time is a constant of 4. I don't know why four copies are kept. 
		public int[,]		y;			// Bee Y positions. y[TIME][bee#] I imagine it's just to keep them ready for drawing ahead of time as a kind of cache.
		public int[]		xv;			// Bee horizontal velocity 
		public int[]		yv;			// Bee virtical velocity xv[bee#] 
	} // End of swarmstruct class.

	/// <summary>
	/// A class of commonly used functions: Custom random number generators and a random RGB color method.<br></br>
	/// This class needs no constructor.
	/// </summary>
	public class CommonFunctions 
	{
//		[DllImport("gdi32.dll")]
//		private static extern bool Rectangle(
//			IntPtr hdc,
//			int ulCornerX, int ulCornerY,
//			int lrCornerX, int lrCornerY);
//		public bool RectangleApi(IntPtr hdc, int X1, int Y1, int X2, int Y2)
//		{
//			return Rectangle(hdc,X1,Y1, X2, Y2);
//		}

//		public static int RGB(int R,int G,int B)
//		{
//			return (R |(G<<8)|(B<<16));         
//		}
//
//		[DllImport("gdi32.dll")] // -*** CreatePen cr = color. Stupid programmers!
//		private static extern IntPtr CreatePen(int iStyle, int iWidth, int cr);
//
//		[DllImport("gdi32.dll")] // -*** MoveToEx use 0 for just plain MoveTo.
//		private static extern bool MoveToEx(IntPtr hdc, int x, int y, IntPtr pt);
//
//		[DllImport("gdi32.dll")] // -*** LineTo
//		private static extern bool LineTo(IntPtr hdc, int x, int y);		

		// ---------------------------------------------
//		[DllImport("user32.dll")]
//		private static extern IntPtr GetDC(IntPtr hWnd);
//		public IntPtr GetDcApi(IntPtr hWnd)
//		{
//			return GetDC(hWnd);
//		}

//		[DllImport("gdi32.dll")]
//		private static extern IntPtr CreateCompatibleBitmap(IntPtr HDC, int X, int Y);
//		public IntPtr CreateCompatibleBitmapApi(IntPtr HDC, int X, int Y)
//		{
//			return CreateCompatibleBitmap(HDC, X, Y);
//		}

//		[DllImport("gdi32.dll")] // -*** CreateCompatibleDC Used to turn the htBitmap from a graphic to a real memory pointer we need.
//		private static extern IntPtr CreateCompatibleDC(IntPtr HDC);
//		public IntPtr CreateCompatibleDcApi(IntPtr HDC)
//		{
//			return CreateCompatibleDC(HDC);
//		}
		// ----------------------------------------------------------------------------
//		[DllImport("gdi32.dll")] // -*** BitBlt BitBltApi
//		private static extern bool BitBlt(IntPtr HDC, int Top, int Left, 
//			int Width, int Height, IntPtr SourceHDC, 
//			int X, int Y, int ROP);
//		public bool BitBltApi(IntPtr HDC, int Top, int Left, int Width, int Height, IntPtr SourceHDC, int X, int Y)
//		{	
//			// this value for ROP copies the buffer.
//            bool b = BitBlt(HDC, Top, Left, Width, Height, SourceHDC, X, Y, 0x00CC0020); 
//			return b;
//		}
//		// -----------------------------------------------------------------------
//        [DllImport("gdi32.dll")]// -*** SelectObject SelectObjectApi 
//		private static extern IntPtr SelectObject(IntPtr hMemDC, IntPtr hObject);
//		public IntPtr SelectObjectApi(IntPtr hMemDC, IntPtr hObject)
//		{
//			return SelectObject(hMemDC, hObject);
//		}
//		// ----------------------------------------------------
//		[DllImport("gdi32.dll")] // -*** DeleteDC DeleteDcApi
//		private static extern bool DeleteDC(IntPtr hMemDC);
//		public bool DeleteDcApi(IntPtr hMemDC)
//		{
//			return DeleteDC(hMemDC);
//		}
//		// --------------------------------------------------------
//		[DllImport("gdi32.dll")]
//		private static extern bool DeleteObject(IntPtr hpen);
//		public bool DeleteObjectApi(IntPtr hObj)
//		{
//			return DeleteObject(hObj);
//		}
		// --------------------------------------------------------
//		[System.Runtime.InteropServices.DllImport("user32.dll")]
//		private static extern int GetSystemMetrics(int nMetrics);
//		public int GetSystemMetricsApi(int nMetrics)
//		{
//			return GetSystemMetrics(nMetrics);
//		}
		// ---------------------------------------------
//		[DllImport("user32.dll")] 
//		private static extern int MessageBeep(uint n); 
// 
//		public void MessageBeepApi() 
//		{ 
//			MessageBeep(0x0);  
//		} 
 

		/// Declares an API call to determine if a window is visible.
		/// /summary>
		/// param name="hWnd"> Handle to the window being checked for visibility.</param>
		/// returns> Returns true if the window is visible, false if not.</returns>
		[DllImport("user32.DLL",EntryPoint="IsWindowVisible")]
		private static extern bool IsWindowVisible(IntPtr hWnd);

		/// <summary>
		/// Declare an external API function call. A wrapper function calls this.
		/// </summary>
		/// <param name="hWnd"> Handle to the window being addressed.</param>
		/// <param name="rect"> Struct containing Size and Location to be filled with the client area size and loc.</param>
		/// <returns> true is success, otherwise false.</returns>
		[DllImport("user32.dll")]
		private static extern bool GetClientRect(IntPtr hWnd, ref RECT rect);

		/// <summary>
		/// Wrapper to call the IsWindowVisible api call.
		/// </summary>
		/// <param name="hWnd">Handle to the Desktop Properties window.</param>
		/// <returns></returns>
		public bool IsWindowVisibleApi(IntPtr hWnd)
		{
			return IsWindowVisible(hWnd);
		}
		/// <summary>
		/// Wrapper to call GetClientRect API functions in user32.dll to get size of the client area of a window.
		/// </summary>
		/// <param name="hWnd"> This is a handle to the desktop properties dialog box</param>
		/// <param name="rect"> This is an instance of a RECT struct to be filled with the location & size of the client area.
		/// </param>
		/// <returns>true if successful, false if uncuccessful</returns>
		public bool GetClientRectApi(IntPtr hWnd, ref RECT rect)
		{
			return GetClientRect(hWnd, ref rect);
		}

//		public void CopyMemoryGraphicToScreen(Graphics screenGr, Graphics memGr, Rectangle rct)
//		{
//			IntPtr hdc = screenGr.GetHdc();
//			IntPtr memHdc = memGr.GetHdc(); 
//			IntPtr hMemdc = CreateCompatibleDC(memHdc);
//			BitBltApi(hdc, rct.Top, rct.Left, rct.Width, rct.Height,hMemdc,rct.X,rct.Y);
//			screenGr.ReleaseHdc(hdc);
//			DeleteDcApi(hMemdc);
//			memGr.ReleaseHdc(memHdc);
//		}
		/// <summary>
		/// A private instance of a random number generator for use by all functions in this class.
		/// </summary>
		private Random RandIntGenerator = new Random(); // Create an instance of a random number generator.
		/// <summary>
		/// Returns a non-negative integer between 0 and 32767 inclusive.
		/// </summary>
		/// <returns>a int positive random number between 0 and 32767 inclusive.</returns>
		public int rand() 
		{
			return RandIntGenerator.Next(32768); // Used 32767 Because it returns 0 to Parameter - 1;
		} 

		/// <summary>
		/// Returns an Int that seems to be in the range of approximately -(v/2)+1 to (v/2)-1 
		/// </summary>
		/// <param name="v"></param>
		/// <returns>an Int that seems to be in the range of approximately -(v/2)+1 to (v/2)-1.</returns>
		public int RAND(int v)
		{
			return ((RandIntGenerator.Next(32768)%(v))-((v)/2)); // WE use 32768 because we want this to work with a random in between 0 to 32767 inclusive.
		}
		// ----------------------------------------------------
		/// <summary>
		/// Returns an integer in the range of MinVal to MaxVal inclusive.
		/// </summary>
		/// <param name="MinVal"></param>
		/// <param name="MaxVal"></param>
		/// <returns>A random int between MinVal and MaxVal inclusive</returns>
		public int randInRange(int MinVal, int MaxVal)
		{
			return RandIntGenerator.Next(MinVal,MaxVal+1);
		}
		// ----------------------------------------------------
		// This does not seem to yield accurate results, but very close. 
		/// <summary>
		/// Convert an HSB to RGB color. Thanks to George Shepherd's site..
		/// http://www.syncfusion.com/FAQ/WindowsForms/FAQ_c85c.aspx#q982q
		/// </summary>
		/// <param name="h"></param>
		/// <param name="s"></param>
		/// <param name="v"></param>
		/// <param name="r"></param>
		/// <param name="g"></param>
		/// <param name="b"></param>
		private void ConvertHSBToRGB(float h, float s, float v, out float r, out float g, out float b) 
		{ 
			if (s == 0f) 
 			{ 
 			// if s = 0 then h is undefined 
 				r = v; 
 				g = v; 
 				b = v; 
 			} 
 			else 
 			{ 
 				float hue = (float)h; 
 				if (h == 360.0f) 
 				{ 
 					hue = 0.0f; 
 				} 
 				hue /= 60.0f; 
 				int i = (int)Math.Floor((double)hue); 
 				float f = hue - i; 
 				float p = v * (1.0f - s); 
 				float q = v * (1.0f - (s * f)); 
 				float t = v * (1.0f - (s * (1 - f))); 
 				switch(i) 
 				{ 
 					case 0: r = v; g = t; b = p; break; 
 					case 1: r = q; g = v; b = p; break; 
 					case 2: r = p; g = v; b = t; break; 
 					case 3: r = p; g = q; b = v; break; 
 					case 4: r = t; g = p; b = v; break; 
 					case 5: r = v; g = p; b = q; break; 
 					default: r = 0.0f; g = 0.0f; b = 0.0f; break; 
				} 
			} 
		} 
		// ----------------------------------------------------
		/// 
		/// Adjusts the specified Fore Color's brightness based on the specified back color and preferred contrast. <br>
		/// Thanks to George Shepherd's site..<br>
		/// http://www.syncfusion.com/FAQ/WindowsForms/FAQ_c85c.aspx#q982q<br>
		/// The fore Color to adjust. <br>
		/// The back Color for reference.<br> 
		/// Preferred contrast level. <br>
		/// This method checks if the current contrast in brightness between the 2 colors is <br>
		/// less than the specified contrast level. If so, it brightens or darkens the fore color appropriately. 
		/// 
	public void AdjustForeColorBrightnessForBackColor(ref Color foreColor, Color backColor, float prefContrastLevel) 
 	{ 
		float fBrightness = foreColor.GetBrightness(); 
 		float bBrightness = backColor.GetBrightness(); 
 		float curContrast = fBrightness - bBrightness; 
 		float delta = prefContrastLevel - (float)Math.Abs(curContrast); 
		if((float)Math.Abs(curContrast) < prefContrastLevel) 
		{ 
			if(bBrightness < 0.5f) 
			{ 
				fBrightness = bBrightness + prefContrastLevel; 
				if(fBrightness > 1.0f) 
					fBrightness = 1.0f; 
			} 
			else 
			{ 
				fBrightness = bBrightness - prefContrastLevel; 
				if(fBrightness < 0.0f) 
					fBrightness = 0.0f; 
			} 
			float newr, newg, newb; 
			ConvertHSBToRGB(foreColor.GetHue(), foreColor.GetSaturation(), fBrightness, out newr, out newg, out newb); 
			foreColor = Color.FromArgb(foreColor.A, (int)Math.Floor(newr * 255f), (int)Math.Floor(newg * 255f), (int)Math.Floor(newb * 255f)); 
		}
	} 
 
		// <summary>
		// Fills a ref byte array with random numbers 0 to 255 inclusive per element. 
		// </summary>
		// <param name="rgb">R for Red, G for Green, B for Blue. The higher the number, the more intense the color.</param>
//		public void RandRGB(byte[] rgb) // No longer in use.
//		{
//			RandIntGenerator.NextBytes(rgb);
//			rgb[0] |= 64;
//			rgb[1] |= 64;
//			rgb[2] |= 64;
//		}
		// ------------------------------------------- End methods. ----------------------------------------------

	} // Class End
}

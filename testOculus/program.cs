using System;

using OpenTK;

using Oculus;

using ovrBool = System.Byte;
using ovrSession = System.IntPtr;
using ovrTextureSwapChain = System.IntPtr;
using ovrMirrorTexture = System.IntPtr;

namespace TestOculus
{
   class Program
   {

      static void Main(string[] args)
      {
			Result result;
			ovrSession session = IntPtr.Zero;
			GraphicsLuid luid = new GraphicsLuid();

			InitParams initParams = new InitParams()
			{
				Flags = InitFlags.None,
				RequestedMinorVersion = Types.OVR_MINOR_VERSION, 
				LogCallback = null,
				UserData = IntPtr.Zero,
				ConnectionTimeoutMS = 0
			};

			result = OvrDLL.ovr_Initialize(ref initParams);
			if (result < 0)
			{
				Console.WriteLine("Failed to initialize OVR");
			}

			result = OvrDLL.ovr_Create(ref session, ref luid);
			if (result < 0)
			{
				Console.WriteLine("Failed to create OVR session");
			}

			HmdDesc desc = OvrDLL.ovr_GetHmdDesc(session);
			
			Console.WriteLine("HMD Type: {0}", desc.Type);
			Console.WriteLine("HMD Product Name: {0}", desc.ProductName);
			Console.WriteLine("HMD Manufacturer: {0}", desc.Manufacturer);
			Console.WriteLine("HMD Serial Number: {0}", desc.SerialNumber);
			Console.WriteLine("HMD Resolution {0}x{1}", desc.Resolution.Width, desc.Resolution.Height);


			for(int i=0; i < 200; i++)
			{
				TrackingState ts = OvrDLL.ovr_GetTrackingState(session, OvrDLL.ovr_GetTimeInSeconds(), true);
				if(ts.StatusFlags.HasFlag(StatusBits.OrientationTracked | StatusBits.PositionTracked))
				{
					Console.WriteLine("Head Position: {0}", ts.HeadPose.ThePose.Position);
					Console.WriteLine("Head Orientation: {0}", ts.HeadPose.ThePose.Orientation);
				}
			}

			OvrDLL.ovr_Shutdown();
		}
	}
}
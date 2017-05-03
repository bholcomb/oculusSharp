using System;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;

using Oculus;

using ovrBool = System.Byte;
using ovrSession = System.IntPtr;
using ovrTextureSwapChain = System.IntPtr;
using ovrMirrorTexture = System.IntPtr;

namespace TestOculus
{
	class Program
	{
		public class TestOculus : GameWindow
		{
			public static int theWidth = 1280;
			public static int theHeigth = 800;

			Result result;
			ErrorInfo error;
			ovrSession session = IntPtr.Zero;
			GraphicsLuid luid = new GraphicsLuid();
			HmdDesc hmdDesc;
			EyeRenderDesc[] eyeDesc = new EyeRenderDesc[2];
			ovrTextureSwapChain swapChain = IntPtr.Zero;
			ovrMirrorTexture mirror = IntPtr.Zero;

			public TestOculus() : base(theWidth, theHeigth, new GraphicsMode(32, 24, 0, 0), "Test FBO", GameWindowFlags.Default, DisplayDevice.Default, 4, 5,
#if DEBUG
         GraphicsContextFlags.Debug)
#else
         GraphicsContextFlags.Default)
#endif
			{
				Keyboard.KeyUp += new EventHandler<KeyboardKeyEventArgs>(handleKeyboardUp);
			}

			public void handleKeyboardUp(object sender, KeyboardKeyEventArgs e)
			{
				if (e.Key == Key.Escape)
				{
					Exit();
				}
			}

			protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
			{
				base.OnClosing(e);

				OvrDLL.ovr_DestroyMirrorTexture(session, mirror);
				OvrDLL.ovr_DestroyTextureSwapChain(session, swapChain);
				OvrDLL.ovr_Destroy(session);
				OvrDLL.ovr_Shutdown();
			}

			protected override void OnResize(EventArgs e)
			{
				base.OnResize(e);
			}

			protected override void OnUpdateFrame(FrameEventArgs e)
			{
				base.OnUpdateFrame(e);

				double displayMidpoint = OvrDLL.ovr_GetPredictedDisplayTime(session, 0);
				TrackingState ts = OvrDLL.ovr_GetTrackingState(session, displayMidpoint, true);
				if (ts.StatusFlags.HasFlag(StatusBits.OrientationTracked | StatusBits.PositionTracked))
				{
					Console.WriteLine("Head Position: {0}", ts.HeadPose.ThePose.Position);
					Console.WriteLine("Head Orientation: {0}", ts.HeadPose.ThePose.Orientation);

					Vector3[] eyeOffsets = new Vector3[2] {eyeDesc[0].HmdToEyeOffset, eyeDesc[1].HmdToEyeOffset };
					Posef[] eyePoses = new Posef[2];
					OvrDLL.ovr_CalcEyePoses(ts.HeadPose.ThePose, eyeOffsets, eyePoses);

					Console.WriteLine("HMD Eye Offsets: {0} | {1}", eyeOffsets[0], eyeOffsets[1]);
					Console.WriteLine("HMD Eye Poses: {0}-{1} | {2}-{3}", eyePoses[0].Position, eyePoses[0].Orientation, eyePoses[1].Position, eyePoses[1].Orientation);
				}


			}

			protected override void OnLoad(EventArgs e)
			{
				base.OnLoad(e);

				//initialize the library
				InitParams initParams = new InitParams()
				{
					Flags = InitFlags.None,
					RequestedMinorVersion = Constants.OVR_MINOR_VERSION,
					LogCallback = null,
					UserData = IntPtr.Zero,
					ConnectionTimeoutMS = 0
				};

				result = OvrDLL.ovr_Initialize(ref initParams);
				if (result < 0)
				{
					Console.WriteLine("Failed to initialize OVR");
				}

				//create the session
				result = OvrDLL.ovr_Create(ref session, ref luid);
				if (result < 0)
				{
					Console.WriteLine("Failed to create OVR session");
				}

				//get device description and capabilities
				hmdDesc = OvrDLL.ovr_GetHmdDesc(session);

				Console.WriteLine("HMD Type: {0}", hmdDesc.Type);
				Console.WriteLine("HMD Product Name: {0}", hmdDesc.ProductName);
				Console.WriteLine("HMD Manufacturer: {0}", hmdDesc.Manufacturer);
				Console.WriteLine("HMD Serial Number: {0}", hmdDesc.SerialNumber);
				Console.WriteLine("HMD Resolution {0}x{1}", hmdDesc.Resolution.Width, hmdDesc.Resolution.Height);
				Console.WriteLine("Version String: {0}", OvrDLL.ovr_GetVersionString());

				//get the eye descriptions
				eyeDesc[0] = OvrDLL.ovr_GetRenderDesc(session, EyeType.Left, hmdDesc.LeftDefaultEyeFov);
				eyeDesc[1] = OvrDLL.ovr_GetRenderDesc(session, EyeType.Right, hmdDesc.RightDefaultEyeFov);

				//get the tracker (the first one) description and then recenter it
				TrackerDesc trackDesc = OvrDLL.ovr_GetTrackerDesc(session, 0);
				Console.WriteLine("Tracker 0 description: FOV H: {0} V: {1} Near: {2} Far: {3}", trackDesc.FrustumHFovInRadians, trackDesc.FrustumVFovInRadians, trackDesc.FrustumNearZInMeters, trackDesc.FrustumFarZInMeters);
				OvrDLL.ovr_RecenterTrackingOrigin(session);
				Console.WriteLine("Tracker origin recentered");
				
				//create the texture swap chain
				TextureSwapChainDesc swapDesc = new TextureSwapChainDesc()
				{
					Type = TextureType.Texture2D,
					ArraySize = 1,
					Width = hmdDesc.Resolution.Width,
					Height = hmdDesc.Resolution.Height,
					MipLevels = 1,
					Format = TextureFormat.R8G8B8A8_UNORM_SRGB,
					SampleCount = 1,
					StaticImage = 0
				};
				
				result = OvrDLL.ovr_CreateTextureSwapChainGL(session, ref swapDesc, ref swapChain);
				if (result < 0)
				{
					Console.WriteLine("Error creating swap chain");
					OvrDLL.ovr_GetLastErrorInfo(ref error);
					Console.WriteLine("Last Error Info: {0}-{1}", error.result, error.ErrorString);
				}

				int swapChainLength = 0;
				OvrDLL.ovr_GetTextureSwapChainLength(session, swapChain, ref swapChainLength);
				Console.WriteLine("Swapchain length: {0}", swapChainLength);
				for (int i = 0; i < swapChainLength; i++)
				{
					UInt32 chainTexId = 0;
					OvrDLL.ovr_GetTextureSwapChainBufferGL(session, swapChain, i, ref chainTexId);
					Console.WriteLine("OpenGL Swap Texture ID: {0}", chainTexId);
				}
				int currentIndex = 0;
				OvrDLL.ovr_GetTextureSwapChainCurrentIndex(session, swapChain, ref currentIndex);
				Console.WriteLine("Swapchain current index: {0}", currentIndex);


				//create the mirror texture
				MirrorTextureDesc mirrorDesc = new MirrorTextureDesc()
				{
					Format = TextureFormat.R8G8B8A8_UNORM_SRGB,
					Width = 800,
					Height = 600
				};
				
				result = OvrDLL.ovr_CreateMirrorTextureGL(session, ref mirrorDesc, ref mirror);
				if (result < 0)
				{
					Console.WriteLine("Error creating mirror texture");
					OvrDLL.ovr_GetLastErrorInfo(ref error);
					Console.WriteLine("Last Error Info: {0}-{1}", error.result, error.ErrorString);
				}
				UInt32 mirrorTexId = 0;
				OvrDLL.ovr_GetMirrorTextureBufferGL(session, mirror, ref mirrorTexId);
				Console.WriteLine("OpenGL Mirror texture ID: {0}", mirrorTexId);
			}

			protected override void OnRenderFrame(FrameEventArgs e)
			{
				base.OnRenderFrame(e);

				//render each eye
				for(int i=0; i< 2; i++)
				{

				}

				OvrDLL.ovr_CommitTextureSwapChain(session, swapChain);

				LayerHeader[] layers = new LayerHeader[1];
				ViewScaleDesc viewScale = new ViewScaleDesc();
				result = OvrDLL.ovr_SubmitFrame(session, 0, ref viewScale, ref layers, 1);
				if (result < 0)
				{
					Console.WriteLine("Error creating mirror texture");
					OvrDLL.ovr_GetLastErrorInfo(ref error);
					Console.WriteLine("Last Error Info: {0}-{1}", error.result, error.ErrorString);
				}

				//blit mirror to fbo

				SwapBuffers();
			}
		}

		static void Main(string[] args)
		{
			using (TestOculus example = new TestOculus())
			{
				example.Title = "Test Oculus";
				example.Run(60.0);
			}
		}
	}
}
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.IO;
using System.ComponentModel;

using OpenTK;

using ovrBool = System.Byte;
using ovrSession = System.IntPtr;
using ovrTextureSwapChain = System.IntPtr;
using ovrMirrorTexture = System.IntPtr;

namespace Oculus
{
	#region DLL Loading

	public class DllLoader
	{
		IntPtr myDllPtr { get; set; }

		public DllLoader(String dllName)
		{
			loadDll(dllName);
		}

		/// <summary>
		/// Loads a dll into process memory.
		/// </summary>
		/// <param name="lpFileName">Filename to load.</param>
		/// <returns>Pointer to the loaded library.</returns>
		/// <remarks>
		/// This method is used to load a dll into memory, before calling any of it's DllImported methods.
		/// 
		/// This is done to allow loading an x86 version of a dllfor an x86 process, or an x64 version of it
		/// for an x64 process.
		/// </remarks>
		[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
		public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]String lpFileName);

		/// <summary>
		/// Frees a previously loaded dll, from process memory.
		/// </summary>
		/// <param name="hModule">Pointer to the previously loaded library (This pointer comes from a call to LoadLibrary).</param>
		/// <returns>Returns true if the library was successfully freed.</returns>
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool FreeLibrary(IntPtr hModule);

		/// <summary>
		/// This method is used to load the dll into memory, before calling any of it's DllImported methods.
		/// 
		/// This is done to allow loading an x86 or x64 version of the dll depending on the process
		/// </summary>
		private void loadDll(String dllName)
		{
			if (myDllPtr == IntPtr.Zero)
			{
				// Retrieve the folder of the OculusWrap.dll.
				string executingAssemblyFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

				string subfolder;

				if (Environment.Is64BitProcess)
					subfolder = "x64";
				else
					subfolder = "x32";

				string filename = Path.Combine(executingAssemblyFolder, subfolder, dllName);

				// Check that the dll file exists.
				bool exists = File.Exists(filename);
				if (!exists)
					throw new DllNotFoundException("Unable to load the file \"" + filename + "\", the file wasn't found.");

				myDllPtr = LoadLibrary(filename);
				if (myDllPtr == IntPtr.Zero)
				{
					int win32Error = Marshal.GetLastWin32Error();
					throw new Win32Exception(win32Error, "Unable to load the file \"" + filename + "\", LoadLibrary reported error code: " + win32Error + ".");
				}
			}
		}

		/// <summary>
		/// Frees previously loaded dll, from process memory.
		/// </summary>
		private void UnloadDll()
		{
			if (myDllPtr != IntPtr.Zero)
			{
				bool success = FreeLibrary(myDllPtr);
				if (success)
					myDllPtr = IntPtr.Zero;
			}
		}
	};

	#endregion

	public static class OvrDLL
	{
		private const string OVR_DLL = "libOVR.dll";

		static DllLoader theOvrDll;

		static OvrDLL()
		{
			theOvrDll = new DllLoader(OVR_DLL);
		}

		#region Initalization/Shutdown
		/* 
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_Initialize(const ovrInitParams* params);
		OVR_PUBLIC_FUNCTION(void) ovr_Shutdown();
		OVR_PUBLIC_FUNCTION(void) ovr_GetLastErrorInfo(ovrErrorInfo* errorInfo);
		OVR_PUBLIC_FUNCTION(const char*) ovr_GetVersionString();

		OVR_PUBLIC_FUNCTION(int) ovr_TraceMessage(int level, const char* message);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_IdentifyClient(const char* identity);
		*/
		[DllImport(OVR_DLL, EntryPoint = "ovr_Initialize", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_Initialize(ref InitParams param);

		[DllImport(OVR_DLL, EntryPoint = "ovr_Shutdown", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern void ovr_Shutdown();

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetLastErrorInfo", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern void ovr_GetLastErrorInfo(ref ErrorInfo errorInfo);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetVersionString", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		static extern IntPtr _ovr_GetVersionString();
		public static string ovr_GetVersionString()
		{
			IntPtr ret = _ovr_GetVersionString();
			return Marshal.PtrToStringAnsi(ret);
		}

		[DllImport(OVR_DLL, EntryPoint = "ovr_TraceMessage", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern int ovr_TraceMessage(int level, string message);

		[DllImport(OVR_DLL, EntryPoint = "ovr_IdentifyClient", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_IdentifyClient(string identity);

		#endregion

		#region HMD Management
		/*  
		OVR_PUBLIC_FUNCTION(ovrHmdDesc) ovr_GetHmdDesc(ovrSession session);
		OVR_PUBLIC_FUNCTION(unsigned int) ovr_GetTrackerCount(ovrSession session);
		OVR_PUBLIC_FUNCTION(ovrTrackerDesc) ovr_GetTrackerDesc(ovrSession session, unsigned int trackerDescIndex);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_Create(ovrSession* pSession, ovrGraphicsLuid* pLuid);
		OVR_PUBLIC_FUNCTION(void) ovr_Destroy(ovrSession session);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_GetSessionStatus(ovrSession session, ovrSessionStatus* sessionStatus);
		*/
		[DllImport(OVR_DLL, EntryPoint = "ovr_GetHmdDesc", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern HmdDesc ovr_GetHmdDesc(ovrSession session);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetTrackerCount", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern UInt32 ovr_GetTrackerCount(ovrSession session);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetTrackerDesc", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern TrackerDesc ovr_GetTrackerDesc(ovrSession session, UInt32 trackerDescIndex);

		[DllImport(OVR_DLL, EntryPoint = "ovr_Create", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_Create(ref ovrSession session, ref GraphicsLuid luid);

		[DllImport(OVR_DLL, EntryPoint = "ovr_Destroy", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern void ovr_Destroy(ovrSession session);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetSessionStatus", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_GetSessionStatus(ovrSession session, ref SessionStatus sessionStatus);
		#endregion

		#region Tracking API
		/*   
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_SetTrackingOriginType(ovrSession session, ovrTrackingOrigin origin);
		OVR_PUBLIC_FUNCTION(ovrTrackingOrigin) ovr_GetTrackingOriginType(ovrSession session);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_RecenterTrackingOrigin(ovrSession session);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_SpecifyTrackingOrigin(ovrSession session, ovrPosef originPose);
		OVR_PUBLIC_FUNCTION(void) ovr_ClearShouldRecenterFlag(ovrSession session);
		OVR_PUBLIC_FUNCTION(ovrTrackingState) ovr_GetTrackingState(ovrSession session, double absTime, ovrBool latencyMarker);
		OVR_PUBLIC_FUNCTION(ovrTrackerPose) ovr_GetTrackerPose(ovrSession session, unsigned int trackerPoseIndex);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_GetInputState(ovrSession session, ovrControllerType controllerType, ovrInputState* inputState);
		OVR_PUBLIC_FUNCTION(unsigned int) ovr_GetConnectedControllerTypes(ovrSession session);
		OVR_PUBLIC_FUNCTION(ovrTouchHapticsDesc) ovr_GetTouchHapticsDesc(ovrSession session, ovrControllerType controllerType);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_SetControllerVibration(ovrSession session, ovrControllerType controllerType, float frequency, float amplitude);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_SubmitControllerVibration(ovrSession session, ovrControllerType controllerType, const ovrHapticsBuffer* buffer);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_GetControllerVibrationState(ovrSession session, ovrControllerType controllerType, ovrHapticsPlaybackState* outState);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_TestBoundary(ovrSession session, ovrTrackedDeviceType deviceBitmask, 
                                                ovrBoundaryType boundaryType, ovrBoundaryTestResult* outTestResult);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_TestBoundaryPoint(ovrSession session, const ovrVector3f* point, 
                                                     ovrBoundaryType singleBoundaryType, ovrBoundaryTestResult* outTestResult);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_SetBoundaryLookAndFeel(ovrSession session, const ovrBoundaryLookAndFeel* lookAndFeel);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_ResetBoundaryLookAndFeel(ovrSession session);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_GetBoundaryGeometry(ovrSession session, ovrBoundaryType boundaryType, ovrVector3f* outFloorPoints, int* outFloorPointsCount);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_GetBoundaryDimensions(ovrSession session, ovrBoundaryType boundaryType, ovrVector3f* outDimensions);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_GetBoundaryVisible(ovrSession session, ovrBool* outIsVisible); 

		OVR_PUBLIC_FUNCTION(ovrResult) ovr_RequestBoundaryVisible(ovrSession session, ovrBool visible);
		*/

		[DllImport(OVR_DLL, EntryPoint = "ovr_SetTrackingOriginType", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_SetTrackingOriginType(ovrSession session, TrackingOrigin origin);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetTrackingOriginType", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern TrackingOrigin ovr_GetTrackingOriginType(ovrSession session);

		[DllImport(OVR_DLL, EntryPoint = "ovr_RecenterTrackingOrigin", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_RecenterTrackingOrigin(ovrSession session);

		[DllImport(OVR_DLL, EntryPoint = "ovr_SpecifyTrackingOrigin", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_SpecifyTrackingOrigin(ovrSession session, Posef orignPose);

		[DllImport(OVR_DLL, EntryPoint = "ovr_ClearShouldRecenterFlag", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern void ovr_ClearShouldRecenterFlag(ovrSession session);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetTrackingState", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		static extern TrackingState ovr_GetTrackingState(ovrSession session, double absTime, ovrBool latencyMarker);
		public static TrackingState ovr_GetTrackingState(ovrSession session, double absTime, bool latencyMarker)
		{
			return ovr_GetTrackingState(session, absTime, latencyMarker == true ? (byte)1 : (byte)0);
		}

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetTrackerPose", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern TrackerPose ovr_GetTrackerPose(ovrSession session, UInt32 trackerPoseIndex);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetInputState", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_GetInputState(ovrSession session, ControllerType controllerType, ref InputState inputState);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetConnectedControllerTypes", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern UInt32 ovr_GetConnectedControllerTypes(ovrSession session);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetTouchHapticsDesc", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern TouchHapticsDesc ovr_GetTouchHapticsDesc(ovrSession session, ControllerType controllerType);

		[DllImport(OVR_DLL, EntryPoint = "ovr_SetControllerVibration", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_SetControllerVibration(ovrSession session, ControllerType controllerType, float frequency, float amplitude);

		[DllImport(OVR_DLL, EntryPoint = "ovr_SubmitControllerVibration", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_SubmitControllerVibration(ovrSession session, ControllerType controllerType, ref HapticsBuffer buffer);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetControllerVibrationState", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_GetControllerVibrationState(ovrSession session, ControllerType controllerType, ref HapticsPlaybackState outState);

		[DllImport(OVR_DLL, EntryPoint = "ovr_TestBoundary", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_TestBoundary(ovrSession session, TrackedDeviceType deviceBitmask, BoundaryType boundaryType, ref BoundaryTestResult outTestResult);

		[DllImport(OVR_DLL, EntryPoint = "ovr_TestBoundaryPoint", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_TestBoundaryPoint(ovrSession session, ref Vector3 point, BoundaryType singleboundaryType, ref BoundaryTestResult outTestResult);

		[DllImport(OVR_DLL, EntryPoint = "ovr_SetBoundaryLookAndFeel", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_SetBoundaryLookAndFeel(ovrSession session, ref BoundaryLookAndFeel lookAndFeel);

		[DllImport(OVR_DLL, EntryPoint = "ovr_ResetBoundaryLookAndFeel", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_ResetBoundaryLookAndFeel(ovrSession session);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetBoundaryGeometry", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_GetBoundaryGeometry(ovrSession session, BoundaryType boundaryType, ref Vector3 outFloorPoints, ref int outFloorPointsCount);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetBoundaryDimensions", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_GetBoundaryDimensions(ovrSession session, BoundaryType boundaryType, ref Vector3 outDimensions);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetBoundaryVisible", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_GetBoundaryVisible(ovrSession session, ref ovrBool outIsVisible);

		[DllImport(OVR_DLL, EntryPoint = "ovr_RequestBoundaryVisible", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_RequestBoundaryVisible(ovrSession session, ref ovrBool isVisible);
		#endregion

		#region SDK Distortion Rendering
		/* 
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_GetTextureSwapChainLength(ovrSession session, ovrTextureSwapChain chain, int* out_Length);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_GetTextureSwapChainCurrentIndex(ovrSession session, ovrTextureSwapChain chain, int* out_Index);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_GetTextureSwapChainDesc(ovrSession session, ovrTextureSwapChain chain, ovrTextureSwapChainDesc* out_Desc);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_CommitTextureSwapChain(ovrSession session, ovrTextureSwapChain chain);
		OVR_PUBLIC_FUNCTION(void) ovr_DestroyTextureSwapChain(ovrSession session, ovrTextureSwapChain chain);
		OVR_PUBLIC_FUNCTION(void) ovr_DestroyMirrorTexture(ovrSession session, ovrMirrorTexture mirrorTexture);
		OVR_PUBLIC_FUNCTION(ovrSizei) ovr_GetFovTextureSize(ovrSession session, ovrEyeType eye, ovrFovPort fov, float pixelsPerDisplayPixel);
		OVR_PUBLIC_FUNCTION(ovrEyeRenderDesc) ovr_GetRenderDesc(ovrSession session, ovrEyeType eyeType, ovrFovPort fov);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_SubmitFrame(ovrSession session, long long frameIndex,
                                                  const ovrViewScaleDesc* viewScaleDesc,
                                                  ovrLayerHeader const * const * layerPtrList, unsigned int layerCount);
		*/
		[DllImport(OVR_DLL, EntryPoint = "ovr_GetTextureSwapChainLength", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_GetTextureSwapChainLength(ovrSession session, ovrTextureSwapChain chain, ref int outLength);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetTextureSwapChainCurrentIndex", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_GetTextureSwapChainCurrentIndex(ovrSession session, ovrTextureSwapChain chain, ref int outIndex);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetTextureSwapChainDesc", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_GetTextureSwapChainDesc(ovrSession session, ovrTextureSwapChain chain, ref TextureSwapChainDesc outDesc);

		[DllImport(OVR_DLL, EntryPoint = "ovr_CommitTextureSwapChain", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_CommitTextureSwapChain(ovrSession session, ovrTextureSwapChain chain);

		[DllImport(OVR_DLL, EntryPoint = "ovr_DestroyTextureSwapChain", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern void ovr_DestroyTextureSwapChain(ovrSession session, ovrTextureSwapChain chain);

		[DllImport(OVR_DLL, EntryPoint = "ovr_DestroyMirrorTexture", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern void ovr_DestroyMirrorTexture(ovrSession session, ovrMirrorTexture mirrorTexture);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetFovTextureSize", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Sizei ovr_GetFovTextureSize(ovrSession session, EyeType eye, FovPort vof, float pixelsPerDisplayPixel);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetRenderDesc", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern EyeRenderDesc ovr_GetRenderDesc(ovrSession session, EyeType eye, FovPort fov);

		[DllImport(OVR_DLL, EntryPoint = "ovr_SubmitFrame", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_SubmitFrame(ovrSession session, Int64 frameIndex, ref ViewScaleDesc viewScaleDesc, IntPtr layerPtrList, UInt32 layerCount);
		#endregion

		#region Frame Timing
		/* 
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_GetPerfStats(ovrSession session, ovrPerfStats* outStats);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_ResetPerfStats(ovrSession session);
		OVR_PUBLIC_FUNCTION(double) ovr_GetPredictedDisplayTime(ovrSession session, long long frameIndex);
		OVR_PUBLIC_FUNCTION(double) ovr_GetTimeInSeconds();
		*/
		[DllImport(OVR_DLL, EntryPoint = "ovr_GetPerfStats", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_GetPerfStats(ovrSession session, ref PerfStats outStats);

		[DllImport(OVR_DLL, EntryPoint = "ovr_ResetPerfStats", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_ResetPerfStats(ovrSession session);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetPredictedDisplayTime", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern double ovr_GetPredictedDisplayTime(ovrSession session, Int64 frameIndex);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetTimeInSeconds", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern double ovr_GetTimeInSeconds();
		#endregion

		#region Mixed reality support
		/*
		 OVR_PUBLIC_FUNCTION(ovrResult) ovr_GetExternalCameras( ovrSession session, ovrExternalCamera* cameras, unsigned int* inoutCameraCount);
		 OVR_PUBLIC_FUNCTION(ovrResult) ovr_SetExternalCameraProperties( ovrSession session, const char* name, const ovrCameraIntrinsics* const intrinsics, const ovrCameraExtrinsics* const extrinsics);
		*/

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetExternalCameras", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		static extern Result ovr_GetExternalCameras(ovrSession session, IntPtr cameras, ref UInt32 inoutCameraCount);
		public unsafe static Result ovr_GetExternalCameras(ovrSession session, ExternalCamera[] cameras, ref UInt32 inoutCameraCount)
		{
			fixed (ExternalCamera* ptr = cameras)
			{
				return ovr_GetExternalCameras(session, (IntPtr)ptr, ref inoutCameraCount);
			}
		}

		[DllImport(OVR_DLL, EntryPoint = "ovr_SetExternalCameraProperties", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_SetExternalCameraProperties(ovrSession session, String name, ref CameraIntrinsics intrinsics, ref CameraExtrinsics extrinsics);

		#endregion

		#region Property Access
		/* 
		OVR_PUBLIC_FUNCTION(ovrBool) ovr_GetBool(ovrSession session, const char* propertyName, ovrBool defaultVal);
		OVR_PUBLIC_FUNCTION(ovrBool) ovr_SetBool(ovrSession session, const char* propertyName, ovrBool value);
		OVR_PUBLIC_FUNCTION(int) ovr_GetInt(ovrSession session, const char* propertyName, int defaultVal);
		OVR_PUBLIC_FUNCTION(ovrBool) ovr_SetInt(ovrSession session, const char* propertyName, int value);
		OVR_PUBLIC_FUNCTION(float) ovr_GetFloat(ovrSession session, const char* propertyName, float defaultVal);
		OVR_PUBLIC_FUNCTION(ovrBool) ovr_SetFloat(ovrSession session, const char* propertyName, float value);
		OVR_PUBLIC_FUNCTION(unsigned int) ovr_GetFloatArray(ovrSession session, const char* propertyName,
                                                       float values[], unsigned int valuesCapacity);
		OVR_PUBLIC_FUNCTION(ovrBool) ovr_SetFloatArray(ovrSession session, const char* propertyName,
                                                  const float values[], unsigned int valuesSize);
		OVR_PUBLIC_FUNCTION(const char*) ovr_GetString(ovrSession session, const char* propertyName,
                                                  const char* defaultVal);
		OVR_PUBLIC_FUNCTION(ovrBool) ovr_SetString(ovrSession session, const char* propertyName,
                                              const char* value);
		*/
		[DllImport(OVR_DLL, EntryPoint = "ovr_GetBool", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern ovrBool ovr_GetBool(ovrSession session, String propertyName, ovrBool defaultValue);

      [DllImport(OVR_DLL, EntryPoint = "ovr_SetBool", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern ovrBool ovr_SetBool(ovrSession session, String propertyName, ovrBool value);

      [DllImport(OVR_DLL, EntryPoint = "ovr_GetInt", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern int ovr_GetInt(ovrSession session, String propertyName, int defaultValue);

      [DllImport(OVR_DLL, EntryPoint = "ovr_SetInt", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern ovrBool ovr_SetInt(ovrSession session, String propertyName, int value);

      [DllImport(OVR_DLL, EntryPoint = "ovr_GetFloat", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern float ovr_GetFloat(ovrSession session, String propertyName, float defaultValue);

      [DllImport(OVR_DLL, EntryPoint = "ovr_SetFloat", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern ovrBool ovr_SetFloat(ovrSession session, String propertyName, float value);

      [DllImport(OVR_DLL, EntryPoint = "ovr_GetFloatArray", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      static extern UInt32 ovr_GetFloatArray(ovrSession session, String propertyName, IntPtr values, UInt32 valueSize);
		public unsafe static UInt32 ovr_GetFloatArray(ovrSession session, String propertyName, float[] values, UInt32 valueSize)
		{
			fixed(float* ptr = values)
			{
				return ovr_GetFloatArray(session, propertyName, (IntPtr)ptr, valueSize);
			}
		}

		[DllImport(OVR_DLL, EntryPoint = "ovr_SetFloatArray", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      static extern ovrBool ovr_SetFloatArray(ovrSession session, String propertyName, IntPtr values, UInt32 valueSize);
		public unsafe static ovrBool ovr_SetFloatArray(ovrSession session, String propertyName, float[] values, UInt32 valueSize)
		{
			fixed(float* ptr = values)
			{
				return ovr_SetFloatArray(session, propertyName, (IntPtr)ptr, valueSize);
			}
		}

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetString", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity]
      public static extern string ovr_GetString(ovrSession session, String propertyName, string defaultValue);

      [DllImport(OVR_DLL, EntryPoint = "ovr_SetString", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi), SuppressUnmanagedCodeSecurity]
      public static extern ovrBool ovr_SetString(ovrSession session, String propertyName, string defaultValue);

      #endregion

      #region Util API
      /*
		OVR_PUBLIC_FUNCTION(ovrDetectResult) ovr_Detect(int timeoutMilliseconds);
		OVR_PUBLIC_FUNCTION(ovrMatrix4f) ovrMatrix4f_Projection(ovrFovPort fov, float znear, float zfar, unsigned int projectionModFlags);
		OVR_PUBLIC_FUNCTION(ovrTimewarpProjectionDesc) ovrTimewarpProjectionDesc_FromProjection(ovrMatrix4f projection, unsigned int projectionModFlags);
		OVR_PUBLIC_FUNCTION(ovrMatrix4f) ovrMatrix4f_OrthoSubProjection(ovrMatrix4f projection, ovrVector2f orthoScale,
                                                                float orthoDistance, float HmdToEyeOffsetX);
		OVR_PUBLIC_FUNCTION(void) ovr_CalcEyePoses(ovrPosef headPose,
                                           const ovrVector3f hmdToEyeOffset[2],
                                           ovrPosef outEyePoses[2]);

		OVR_PUBLIC_FUNCTION(void) ovr_GetEyePoses(ovrSession session, long long frameIndex, ovrBool latencyMarker,
                                             const ovrVector3f hmdToEyeOffset[2],
                                             ovrPosef outEyePoses[2],
                                             double* outSensorSampleTime);
		OVR_PUBLIC_FUNCTION(void) ovrPosef_FlipHandedness(const ovrPosef* inPose, ovrPosef* outPose);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_ReadWavFromBuffer(ovrAudioChannelData* outAudioChannel, const void* inputData, int dataSizeInBytes, int stereoChannelToUse);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_GenHapticsFromAudioData(ovrHapticsClip* outHapticsClip, const ovrAudioChannelData* audioChannel, ovrHapticsGenMode genMode);
		OVR_PUBLIC_FUNCTION(void) ovr_ReleaseAudioChannelData(ovrAudioChannelData* audioChannel);
		OVR_PUBLIC_FUNCTION(void) ovr_ReleaseHapticsClip(ovrHapticsClip* hapticsClip);
		*/
      [DllImport(OVR_DLL, EntryPoint = "ovr_Detect", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern Result ovr_Detect(int timeoutMilliseconds);

      [DllImport(OVR_DLL, EntryPoint = "ovrMatrix4f_Projection", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      static extern Matrix4 _ovrMatrix4f_Projection(FovPort vof, float znear, float zfar, ProjectionModifier projectionModFlags);
		public static Matrix4 ovrMatrix4f_Projection(FovPort vof, float znear, float zfar, ProjectionModifier projectionModFlags)
		{
			Matrix4 ret = _ovrMatrix4f_Projection(vof, znear, zfar, projectionModFlags);
			//the matrix layout of ovr is row major, but openGL wants column major
			ret.Transpose();

			return ret;
		}

		[DllImport(OVR_DLL, EntryPoint = "ovrTimewarpProjectionDesc_FromProjection", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern TimewarpProjectionDesc ovrTimewarpProjectionDesc_FromProjection(Matrix4 projection, ProjectionModifier projectionModFlags);

      [DllImport(OVR_DLL, EntryPoint = "ovrMatrix4f_OrthoSubProjection", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern Matrix4 ovrMatrix4f_OrthoSubProjection(Matrix4 projection, Vector2 orthoSale, float orthoDistance, float hmdToEyeOffsetx);

      [DllImport(OVR_DLL, EntryPoint = "ovr_CalcEyePoses", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern void ovr_CalcEyePoses(Posef headPose, Vector3[] hmdToEyeOffset, [Out]Posef[] outEyePoses);

      [DllImport(OVR_DLL, EntryPoint = "ovr_GetEyePoses", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern ovrBool ovr_GetEyePoses(ovrSession session, Int64 frameIndex, ovrBool latencyMarker, Vector3[] hmdToEyeOffset, Posef[] outEyePosese, ref double outSensorSampleTime);

      [DllImport(OVR_DLL, EntryPoint = "ovrPosef_FlipHandedness", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern void ovrPosef_FlipHandedness(ref Posef inPose, ref Posef outPose);

      //[DllImport(OVR_DLL, EntryPoint = "ovr_ReadWavFromBuffer", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]////
      //public static extern Result ovr_ReadWavFromBuffer(ref AudioCahnnelData outAudioChannel, IntPtr inputData, int dataSizeInByts, int stereoChannelToUse);

      //[DllImport(OVR_DLL, EntryPoint = "ovr_GenHapticsFromAudioData", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      //public static extern Result ovr_GenHapticsFromAudioData(ref HapticsClip outHapticsClip, ref AudioChannelData audioChannel, HapticsGenMode genMode);

      //[DllImport(OVR_DLL, EntryPoint = "ovr_ReleaseAudioChannelData", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      //public static extern void ovr_ReleaseAudioChannelData(ref AudioChannelData audioChannel);

      [DllImport(OVR_DLL, EntryPoint = "ovr_ReleaseHapticsClip", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern void ovr_ReleaseHapticsClip(HapticsClip hapticsClip);

      #endregion

      #region OpenGL API
      /*
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_CreateTextureSwapChainGL(ovrSession session,
                                                            const ovrTextureSwapChainDesc* desc,
                                                            ovrTextureSwapChain* out_TextureSwapChain);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_GetTextureSwapChainBufferGL(ovrSession session,
                                                               ovrTextureSwapChain chain,
                                                               int index,
                                                               unsigned int* out_TexId);
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_CreateMirrorTextureGL(ovrSession session,
                                                         const ovrMirrorTextureDesc* desc,
                                                         ovrMirrorTexture* out_MirrorTexture);

		OVR_PUBLIC_FUNCTION(ovrResult) ovr_GetMirrorTextureBufferGL(ovrSession session,
                                                            ovrMirrorTexture mirrorTexture,
                                                            unsigned int* out_TexId);
		*/
      [DllImport(OVR_DLL, EntryPoint = "ovr_CreateTextureSwapChainGL", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern Result ovr_CreateTextureSwapChainGL(ovrSession session, ref TextureSwapChainDesc desc, ref ovrTextureSwapChain outTextureSwapChain);

      [DllImport(OVR_DLL, EntryPoint = "ovr_GetTextureSwapChainBufferGL", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern Result ovr_GetTextureSwapChainBufferGL(ovrSession session, ovrTextureSwapChain chain, int index, ref UInt32 outTexId);

      [DllImport(OVR_DLL, EntryPoint = "ovr_CreateMirrorTextureGL", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern Result ovr_CreateMirrorTextureGL(ovrSession session, ref MirrorTextureDesc desc, ref ovrMirrorTexture outMirrorTexture);

      [DllImport(OVR_DLL, EntryPoint = "ovr_GetMirrorTextureBufferGL", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern Result ovr_GetMirrorTextureBufferGL(ovrSession session, ovrMirrorTexture mirroTexture, ref UInt32 outTexId);
      #endregion

      #region DirectX API
      /*
      OVR_PUBLIC_FUNCTION(ovrResult) ovr_CreateTextureSwapChainDX(ovrSession session,
                                                            IUnknown* d3dPtr,
                                                            const ovrTextureSwapChainDesc* desc,
                                                            ovrTextureSwapChain* out_TextureSwapChain);
      
      OVR_PUBLIC_FUNCTION(ovrResult) ovr_GetTextureSwapChainBufferDX(ovrSession session,
                                                               ovrTextureSwapChain chain,
                                                               int index,
                                                               IID iid,
                                                               void** out_Buffer);

      OVR_PUBLIC_FUNCTION(ovrResult) ovr_CreateMirrorTextureDX(ovrSession session,
                                                         IUnknown* d3dPtr,
                                                         const ovrMirrorTextureDesc* desc,
                                                         ovrMirrorTexture* out_MirrorTexture);

      OVR_PUBLIC_FUNCTION(ovrResult) ovr_GetMirrorTextureBufferDX(ovrSession session,
                                                            ovrMirrorTexture mirrorTexture,
                                                            IID iid,
                                                            void** out_Buffer);

      */
      [DllImport(OVR_DLL, EntryPoint = "ovr_CreateTextureSwapChainDX", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern Result ovr_CreateTextureSwapChainDX(ovrSession session, IntPtr d3dPtr, ref TextureSwapChainDesc desc, ref ovrTextureSwapChain outTextureSwapChain);

      [DllImport(OVR_DLL, EntryPoint = "ovr_GetTextureSwapChainBufferDX", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern Result ovr_GetTextureSwapChainBufferDX(ovrSession session, ovrTextureSwapChain chain, int index, IID iid, IntPtr outBuffer);
       
      [DllImport(OVR_DLL, EntryPoint = "ovr_CreateMirrorTextureDX", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern Result ovr_CreateMirrorTextureDX(ovrSession session, IntPtr d3dPtr, ref MirrorTextureDesc desc, ref ovrMirrorTexture outMirrorTexture);

      [DllImport(OVR_DLL, EntryPoint = "ovr_GetMirrorTextureBufferDX", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
      public static extern Result ovr_GetMirrorTextureBufferDX(ovrSession session, ovrMirrorTexture mirroTexture, IID iid, IntPtr outBufer);

		#endregion

		#region Vulkan API
		/*
		OVR_PUBLIC_FUNCTION(ovrResult) ovr_GetSessionPhysicalDeviceVk( ovrSession session,
					 ovrGraphicsLuid luid,
					 VkInstance instance,
					 VkPhysicalDevice* out_physicalDevice);

		OVR_PUBLIC_FUNCTION(ovrResult) ovr_SetSynchonizationQueueVk(ovrSession session, VkQueue queue);

		OVR_PUBLIC_FUNCTION(ovrResult) ovr_CreateTextureSwapChainVk( ovrSession session,
			 VkDevice device,
			 const ovrTextureSwapChainDesc* desc,
			 ovrTextureSwapChain* out_TextureSwapChain);

		OVR_PUBLIC_FUNCTION(ovrResult) ovr_GetTextureSwapChainBufferVk( ovrSession session,
			 ovrTextureSwapChain chain,
			 int index,
			 VkImage* out_Image);

		OVR_PUBLIC_FUNCTION(ovrResult) ovr_CreateMirrorTextureWithOptionsVk( ovrSession session,
			 VkDevice device,
			 const ovrMirrorTextureDesc* desc,
			 ovrMirrorTexture* out_MirrorTexture);

		OVR_PUBLIC_FUNCTION(ovrResult) ovr_GetMirrorTextureBufferVk( ovrSession session,
			 ovrMirrorTexture mirrorTexture,
			 VkImage* out_Image);
		*/

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetSessionPhysicalDeviceVk", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_GetSessionPhysicalDeviceVk(ovrSession session, GraphicsLuid lluid, IntPtr instance, ref IntPtr out_physicalDevice);

		[DllImport(OVR_DLL, EntryPoint = "ovr_SetSynchonizationQueueVk", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_SetSynchonizationQueueVk(ovrSession session, UInt64 queue);

		[DllImport(OVR_DLL, EntryPoint = "ovr_CreateTextureSwapChainVk", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_CreateTextureSwapChainVk(ovrSession session, IntPtr device, ref TextureSwapChainDesc desc, out ovrTextureSwapChain out_TextureSwapChain);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetTextureSwapChainBufferVk", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_GetTextureSwapchainbufferVk(ovrSession session, ovrTextureSwapChain chain, int index, out UInt64 out_Image);

		[DllImport(OVR_DLL, EntryPoint = "ovr_CreateMirrorTextureWithOptionsVk", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_CreateMirrorTextureWithOptionsVk(ovrSession session, IntPtr device, ref MirrorTextureDesc desc, out ovrMirrorTexture out_MirrorTexture);

		[DllImport(OVR_DLL, EntryPoint = "ovr_GetMirrorTextureBufferVk", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		public static extern Result ovr_GetMirrorTextureBufferVk(ovrSession session, ovrMirrorTexture mirrorTexture, out UInt64 out_Image);

		#endregion
	}
}
using System;
using System.Runtime.InteropServices;
using ovrBool = System.Byte;
using ovrTextureSwapChain = System.IntPtr;

using OpenTK;
using OpenTK.Graphics;

namespace Oculus
{
	#region typedefs
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate void LogFunction(IntPtr userData, int level, String message);

	#endregion

	#region constants
	public static class Types
	{
		#region Definitions found in OVR_CAPI_Keys_h
		public const string OVR_KEY_USER = "User";              // string
		public const string OVR_KEY_NAME = "Name";              // string
		public const string OVR_KEY_GENDER = "Gender";            // string "Male", "Female", or "Unknown"
		public const string OVR_DEFAULT_GENDER = "Unknown";
		public const string OVR_KEY_PLAYER_HEIGHT = "PlayerHeight";      // float meters
		public const float OVR_DEFAULT_PLAYER_HEIGHT = 1.778f;
		public const string OVR_KEY_EYE_HEIGHT = "EyeHeight";         // float meters
		public const float OVR_DEFAULT_EYE_HEIGHT = 1.675f;
		public const string OVR_KEY_NECK_TO_EYE_DISTANCE = "NeckEyeDistance";   // float[2] meters
		public const float OVR_DEFAULT_NECK_TO_EYE_HORIZONTAL = 0.0805f;
		public const float OVR_DEFAULT_NECK_TO_EYE_VERTICAL = 0.075f;
		public const string OVR_KEY_EYE_TO_NOSE_DISTANCE = "EyeToNoseDist";     // float[2] meters

		public const string OVR_PERF_HUD_MODE = "PerfHudMode";                       // int, allowed values are defined in enum ovrPerfHudMode
		public const string OVR_LAYER_HUD_MODE = "LayerHudMode";                      // int, allowed values are defined in enum ovrLayerHudMode
		public const string OVR_LAYER_HUD_CURRENT_LAYER = "LayerHudCurrentLayer";              // int, The layer to show 
		public const string OVR_LAYER_HUD_SHOW_ALL_LAYERS = "LayerHudShowAll";                   // bool, Hide other layers when the hud is enabled
		public const string OVR_DEBUG_HUD_STEREO_MODE = "DebugHudStereoMode";                // allowed values are defined in enum DebugHudStereoMode
		public const string OVR_DEBUG_HUD_STEREO_GUIDE_INFO_ENABLE = "DebugHudStereoGuideInfoEnable";     // bool
		public const string OVR_DEBUG_HUD_STEREO_GUIDE_SIZE = "DebugHudStereoGuideSize2f";         // float[2]
		public const string OVR_DEBUG_HUD_STEREO_GUIDE_POSITION = "DebugHudStereoGuidePosition3f";     // float[3]
		public const string OVR_DEBUG_HUD_STEREO_GUIDE_YAWPITCHROLL = "DebugHudStereoGuideYawPitchRoll3f"; // float[3]
		public const string OVR_DEBUG_HUD_STEREO_GUIDE_COLOR = "DebugHudStereoGuideColor4f";        // float[4]
		#endregion

		#region Definitions found in OVR_VERSION_h

		/// <summary>
		/// Product version doesn't participate in semantic versioning.
		/// </summary>
		public const int OVR_PRODUCT_VERSION = 1;

		/// <summary>
		/// If you change these values then you need to also make sure to change LibOVR/Projects/Windows/LibOVR.props in parallel.
		/// </summary>
		public const int OVR_MAJOR_VERSION = 1;
		public const int OVR_MINOR_VERSION = 11;
		public const int OVR_PATCH_VERSION = 0;
		public const int OVR_BUILD_NUMBER = 0;

		/// <summary>
		/// This is the ((product * 100) + major) version of the service that the DLL is compatible with.
		/// When we backport changes to old versions of the DLL we update the old DLLs
		/// to move this version number up to the latest version.
		/// The DLL is responsible for checking that the service is the version it supports
		/// and returning an appropriate error message if it has not been made compatible.
		/// </summary>
		public const int OVR_DLL_COMPATIBLE_MAJOR_VERSION = 101;

		public const int OVR_FEATURE_VERSION = 0;

		public static readonly string OVR_VERSION_STRING = OVR_MAJOR_VERSION + "." + OVR_MINOR_VERSION + "." + OVR_PATCH_VERSION;

		public static readonly string OVR_DETAILED_VERSION_STRING = OVR_MAJOR_VERSION + "." + OVR_MINOR_VERSION + "." + OVR_PATCH_VERSION + "." + OVR_BUILD_NUMBER;
		#endregion

		#region Definitions found in OVR_CAPI_Audio_h
		public const int OVR_AUDIO_MAX_DEVICE_STR_SIZE = 128;
		#endregion

		/// <summary>
		/// Specifies the maximum number of layers supported by ovr_SubmitFrame.
		/// </summary>
		/// <see cref="OVRBase.SubmitFrame"/>
		public const int MaxLayerCount = 16;

		/// Maximum number of frames of performance stats provided back to the caller of ovr_GetPerfStats
		public const int MaxProvidedFrameStats = 5;
	}
	#endregion

	#region Structs
	/// <summary>
	/// A 2D size with integer components.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct Sizei
	{
		public Sizei(int width, int height)
		{
			this.Width = width;
			this.Height = height;
		}

		public int Width,
					Height;
	}

	/// <summary>
	/// A 2D rectangle with a position and size.
	/// All components are integers.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct Recti
	{
		public Recti(Vector2i position, Sizei size)
		{
			Position = position;
			Size = size;
		}

		public Vector2i Position;
		public Sizei Size;
	}

	/// <summary>
	/// Position and orientation together.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct Posef
	{
		[MarshalAs(UnmanagedType.Struct)]
		public Quaternion Orientation;

		[MarshalAs(UnmanagedType.Struct)]
		public Vector3 Position;
	}

	/// <summary>
	/// A full pose (rigid body) configuration with first and second derivatives.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct PoseStatef
	{
		/// <summary>
		/// Position and orientation.
		/// </summary>
		public Posef ThePose;

		/// <summary>
		/// Angular velocity in radians per second.
		/// </summary>
		public Vector3 AngularVelocity;

		/// <summary>
		/// Velocity in meters per second.
		/// </summary>
		public Vector3 LinearVelocity;

		/// <summary>
		/// Angular acceleration in radians per second per second.
		/// </summary>
		public Vector3 AngularAcceleration;

		/// <summary>
		/// Acceleration in meters per second per second.
		/// </summary>
		public Vector3 LinearAcceleration;



		/// <summary>
		/// Absolute time that this pose refers to.
		/// </summary>
		/// <see cref="OVRBase.GetTimeInSeconds"/>
		public double TimeInSeconds;
	}

	/// <summary>
	/// Field Of View (FOV) in tangent of the angle units.
	/// As an example, for a standard 90 degree vertical FOV, we would 
	/// have: { UpTan = tan(90 degrees / 2), DownTan = tan(90 degrees / 2) }.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct FovPort
	{
		/// The tangent of the angle between the viewing vector and the top edge of the field of view.
		public float UpTan;

		/// The tangent of the angle between the viewing vector and the bottom edge of the field of view.
		public float DownTan;

		/// The tangent of the angle between the viewing vector and the left edge of the field of view.
		public float LeftTan;

		/// The tangent of the angle between the viewing vector and the right edge of the field of view.
		public float RightTan;
	}


	[StructLayout(LayoutKind.Sequential)]
	public struct GraphicsLuid
	{
		/// <summary>
		/// Public definition reserves space for graphics API-specific implementation 
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		public byte[] Reserved;
	}

	/// <summary>
	/// This is a complete descriptor of the HMD.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct HmdDesc
	{
		/// <summary>
		/// Copy constructor used to convert an HmdDesc64 to an HmdDesc.
		/// </summary>
		/// <param name="source">HmdDesc64 to copy from.</param>
		public HmdDesc(HmdDesc64 source)
		{
			Type = source.Type;
			ProductName = source.ProductName;
			Manufacturer = source.Manufacturer;
			VendorId = source.VendorId;
			ProductId = source.ProductId;
			SerialNumber = source.SerialNumber;
			FirmwareMajor = source.FirmwareMajor;
			FirmwareMinor = source.FirmwareMinor;
			AvailableHmdCaps = source.AvailableHmdCaps;
			DefaultHmdCaps = source.DefaultHmdCaps;
			AvailableTrackingCaps = source.AvailableTrackingCaps;
			DefaultTrackingCaps = source.DefaultTrackingCaps;
			DefaultEyeFov = source.DefaultEyeFov;
			MaxEyeFov = source.MaxEyeFov;
			Resolution = source.Resolution;
			DisplayRefreshRate = source.DisplayRefreshRate;
		}

		/// <summary>
		/// The type of HMD.
		/// </summary>
		public HmdType Type;

		/// <summary>
		/// Product identification string (e.g. "Oculus Rift DK1").
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public byte[] ProductName;

		/// <summary>
		/// HMD manufacturer identification string.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public byte[] Manufacturer;

		/// <summary>
		/// HID (USB) vendor identifier of the device.
		/// </summary>
		public short VendorId;

		/// <summary>
		/// HID (USB) product identifier of the device.
		/// </summary>
		public short ProductId;

		/// <summary>
		/// HMD serial number.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
		public byte[] SerialNumber;

		/// <summary>
		/// HMD firmware major version.
		/// </summary>
		public short FirmwareMajor;

		/// <summary>
		/// HMD firmware minor version.
		/// </summary>
		public short FirmwareMinor;

		/// <summary>
		/// Capability bits described by HmdCaps which the HMD currently supports.
		/// </summary>
		public HmdCaps AvailableHmdCaps;

		/// <summary>
		/// Capability bits described by HmdCaps which are default for the current Hmd.
		/// </summary>
		public HmdCaps DefaultHmdCaps;

		/// <summary>
		/// Capability bits described by TrackingCaps which the system currently supports.
		/// </summary>
		public TrackingCaps AvailableTrackingCaps;

		/// <summary>
		/// Capability bits described by ovrTrackingCaps which are default for the current system.
		/// </summary>
		public TrackingCaps DefaultTrackingCaps;

		/// <summary>
		/// Defines the recommended FOVs for the HMD.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public FovPort[] DefaultEyeFov;

		/// <summary>
		/// Defines the maximum FOVs for the HMD.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public FovPort[] MaxEyeFov;

		/// <summary>
		/// Resolution of the full HMD screen (both eyes) in pixels.
		/// </summary>
		public Sizei Resolution;

		/// <summary>
		/// Nominal refresh rate of the display in cycles per second at the time of HMD creation.
		/// </summary>
		public float DisplayRefreshRate;
	}

	/// <summary>
	/// 64 bit version of the HmdDesc.
	/// </summary>
	/// <remarks>
	/// This class is needed because the Oculus SDK defines padding fields on the 64 bit version of the Oculus SDK.
	/// </remarks>
	/// <see cref="HmdDesc"/>
	[StructLayout(LayoutKind.Sequential)]
	public struct HmdDesc64
	{
		/// <summary>
		/// The type of HMD.
		/// </summary>
		public HmdType Type;

		/// <summary>
		/// Internal struct paddding.
		/// </summary>
		private int Pad0;

		/// <summary>
		/// Product identification string (e.g. "Oculus Rift DK1").
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public byte[] ProductName;

		/// <summary>
		/// HMD manufacturer identification string.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
		public byte[] Manufacturer;

		/// <summary>
		/// HID (USB) vendor identifier of the device.
		/// </summary>
		public short VendorId;

		/// <summary>
		/// HID (USB) product identifier of the device.
		/// </summary>
		public short ProductId;

		/// <summary>
		/// Sensor (and display) serial number.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
		public byte[] SerialNumber;

		/// <summary>
		/// Sensor firmware major version.
		/// </summary>
		public short FirmwareMajor;

		/// <summary>
		/// Sensor firmware minor version.
		/// </summary>
		public short FirmwareMinor;

		/// <summary>
		/// Capability bits described by HmdCaps which the HMD currently supports.
		/// </summary>
		public HmdCaps AvailableHmdCaps;

		/// <summary>
		/// Capability bits described by HmdCaps which are default for the current Hmd.
		/// </summary>
		public HmdCaps DefaultHmdCaps;

		/// <summary>
		/// Capability bits described by TrackingCaps which the system currently supports.
		/// </summary>
		public TrackingCaps AvailableTrackingCaps;

		/// <summary>
		/// Capability bits described by ovrTrackingCaps which are default for the current system.
		/// </summary>
		public TrackingCaps DefaultTrackingCaps;

		/// <summary>
		/// Defines the recommended FOVs for the HMD.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public FovPort[] DefaultEyeFov;

		/// <summary>
		/// Defines the maximum FOVs for the HMD.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public FovPort[] MaxEyeFov;

		/// <summary>
		/// Resolution of the full HMD screen (both eyes) in pixels.
		/// </summary>
		public Sizei Resolution;

		/// <summary>
		/// Nominal refresh rate of the display in cycles per second at the time of HMD creation.
		/// </summary>
		public float DisplayRefreshRate;

		/// <summary>
		/// Internal struct paddding.
		/// </summary>
		private int Pad1;
	}

	/// <summary>
	/// Specifies the description of a single sensor.
	/// </summary>
	/// <see cref="OVRBase.GetTrackerDesc"/>
	[StructLayout(LayoutKind.Sequential)]
	public struct TrackerDesc
	{
		/// <summary>
		/// Sensor frustum horizontal field-of-view (if present).
		/// </summary>
		public float FrustumHFovInRadians;

		/// <summary>
		/// Sensor frustum vertical field-of-view (if present).
		/// </summary>
		public float FrustumVFovInRadians;

		/// <summary>
		/// Sensor frustum near Z (if present).
		/// </summary>
		public float FrustumNearZInMeters;

		/// <summary>
		/// Sensor frustum far Z (if present).
		/// </summary>
		public float FrustumFarZInMeters;
	}
	/// <summary>
	/// Specifies the pose for a single sensor.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct TrackerPose
	{
		/// <summary>
		/// TrackerFlags
		/// </summary>
		public TrackerFlags trackerFlags;

		/// <summary>
		/// The sensor's pose. This pose includes sensor tilt (roll and pitch). 
		/// For a leveled coordinate system use LeveledPose.
		/// </summary>
		public Posef Pose;

		/// <summary>
		/// The sensor's leveled pose, aligned with gravity. 
		/// This value includes position and yaw of the sensor, but not roll and pitch. It can be used as a reference point to render real-world objects in the correct location.
		/// </summary>
		public Posef LeveledPose;
	}

	/// <summary>
	/// Tracking state at a given absolute time (describes predicted HMD pose etc).
	/// Returned by ovr_GetTrackingState.
	/// <see cref="OVRBase.GetTrackingState"/>
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct TrackingState
	{
		/// <summary>
		/// Predicted head pose (and derivatives) at the requested absolute time.
		/// </summary>
		public PoseStatef HeadPose;

		/// <summary>
		/// HeadPose tracking status described by StatusBits.
		/// </summary>
		public StatusBits StatusFlags;

		/// <summary>
		/// The most recent calculated pose for each hand when hand controller tracking is present.
		/// HandPoses[ovrHand_Left] refers to the left hand and HandPoses[ovrHand_Right] to the right hand.
		/// These values can be combined with ovrInputState for complete hand controller information.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public PoseStatef[] HandPoses;

		/// <summary>
		/// HandPoses status flags described by StatusBits.
		/// Only OrientationTracked and PositionTracked are reported.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public StatusBits[] HandStatusFlags;

		/// <summary>
		/// The pose of the origin captured during calibration.
		/// 
		/// Like all other poses here, this is expressed in the space set by ovr_RecenterTrackingOrigin,
		/// and so will change every time that is called. This pose can be used to calculate
		/// where the calibrated origin lands in the new recentered space.
		/// 
		/// If an application never calls ovr_RecenterTrackingOrigin, expect this value to be the identity
		/// pose and as such will point respective origin based on TrackingOrigin requested when
		/// calling ovr_GetTrackingState.
		/// </summary>
		public Posef CalibratedOrigin;
	}

	/// <summary>
	/// Rendering information for each eye. Computed by ovr_GetRenderDesc() based on the
	/// specified FOV. Note that the rendering viewport is not included
	/// here as it can be specified separately and modified per frame by
	/// passing different Viewport values in the layer structure.
	/// </summary>
	/// <see cref="OVRBase.GetRenderDesc"/>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct EyeRenderDesc
	{
		/// <summary>
		/// The eye index to which this instance corresponds.
		/// </summary>
		public EyeType Eye;

		/// <summary>
		/// The field of view.
		/// </summary>
		public FovPort Fov;

		/// <summary>
		/// Distortion viewport.
		/// </summary>
		public Recti DistortedViewport;

		/// <summary>
		/// How many display pixels will fit in tan(angle) = 1.
		/// </summary>
		public Vector2 PixelsPerTanAngleAtCenter;

		/// <summary>
		/// Translation of each eye, in meters.
		/// </summary>
		public Vector3 HmdToEyeOffset;
	}

	/// <summary>
	/// Projection information for LayerEyeFovDepth.
	/// 
	/// Use the utility function ovrTimewarpProjectionDesc_FromProjection to
	/// generate this structure from the application's projection matrix.
	/// </summary>
	/// <see cref="OVRBase.TimewarpProjectionDesc_FromProjection"/>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct TimewarpProjectionDesc
	{
		/// <summary>
		/// Projection matrix element [2][2].
		/// </summary>
		public float Projection22;

		/// <summary>
		/// Projection matrix element [2][3].
		/// </summary>
		public float Projection23;

		/// <summary>
		/// Projection matrix element [3][2].
		/// </summary>
		public float Projection32;
	}

	/// <summary>
	/// Contains the data necessary to properly calculate position info for various layer types.
	/// </summary>
	/// <see cref="EyeRenderDesc"/>
	/// <see cref="OVRBase.SubmitFrame"/>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct ViewScaleDesc
	{
		/// <summary>
		/// Translation of each eye.
		/// 
		/// The same value pair provided in EyeRenderDesc.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public Vector3[] HmdToEyeOffset;

		/// <summary>
		/// Ratio of viewer units to meter units.
		/// 
		/// Used to scale player motion into in-application units.
		/// In other words, it is how big an in-application unit is in the player's physical meters.
		/// For example, if the application uses inches as its units then HmdSpaceToWorldScaleInMeters would be 0.0254.
		/// Note that if you are scaling the player in size, this must also scale. So if your application
		/// units are inches, but you're shrinking the player to half their normal size, then
		/// HmdSpaceToWorldScaleInMeters would be 0.0254*2.0.
		/// </summary>
		public float HmdSpaceToWorldScaleInMeters;
	}

	/// <summary>
	/// Description used to create a texture swap chain.
	/// </summary>
	/// <see cref="OVRBase.CreateTextureSwapChainDX"/>
	/// <see cref="OVRBase.CreateTextureSwapChainGL"/>
	[StructLayout(LayoutKind.Sequential)]
	public struct TextureSwapChainDesc
	{
		public TextureType Type;
		public TextureFormat Format;

		/// <summary>
		/// Only supported with ovrTexture_2D. 
		/// Not supported on PC at this time.
		/// </summary>
		public int ArraySize;

		public int Width;

		public int Height;

		public int MipLevels;

		/// <summary>
		/// Current only supported on depth textures
		/// </summary>
		public int SampleCount;

		/// <summary>
		/// Not buffered in a chain. For images that don't change
		/// </summary>
		public ovrBool StaticImage;

		/// <summary>
		/// ovrTextureMiscFlags
		/// </summary>
		public TextureMiscFlags MiscFlags;

		/// <summary>
		/// ovrTextureBindFlags. Not used for GL.
		/// </summary>
		public TextureBindFlags BindFlags;
	}

	/// <summary>
	/// Description used to create a mirror texture.
	/// </summary>
	/// <see cref="OVRBase.CreateMirrorTextureDX"/>
	/// <see cref="OVRBase.CreateMirrorTextureGL"/>
	[StructLayout(LayoutKind.Sequential)]
	public struct MirrorTextureDesc
	{
		public TextureFormat Format;
		public int Width;
		public int Height;
		public TextureMiscFlags MiscFlags;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct TouchHapticsDesc
	{
		// Haptics engine frequency/sample-rate, sample time in seconds equals 1.0/sampleRateHz
		public int SampleRateHz;
		// Size of each Haptics sample, sample value range is [0, 2^(Bytes*8)-1]
		public int SampleSizeInBytes;

		// Queue size that would guarantee Haptics engine would not starve for data
		// Make sure size doesn't drop below it for best results
		public int QueueMinSizeToAvoidStarvation;

		// Minimum, Maximum and Optimal number of samples that can be sent to Haptics through ovr_SubmitControllerVibration
		public int SubmitMinSamples;
		public int SubmitMaxSamples;
		public int SubmitOptimalSamples;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct HapticsBuffer
	{
		/// Samples stored in opaque format
		public  IntPtr Samples;
		/// Number of samples
		public int SamplesCount;
		/// How samples are submitted to the hardware
		public HapticsBufferSubmitMode SubmitMode;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct HapticsPlaybackState
	{
		// Remaining space available to queue more samples
		public int RemainingQueueSpace;

		// Number of samples currently queued
		public int SamplesQueued;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct BoundaryTestResult
	{
		// True if the boundary system is being triggered. Note that due to fade in/out effects this may not exactly match visibility.
		ovrBool IsTriggering;

		// Distance to the closest play area or outer boundary surface.
		float ClosestDistance;

		// Closest point on the boundary surface.
		Vector3 ClosestPoint;

		// Unit surface normal of the closest boundary surface.
		Vector3 ClosestPointNormal;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct BoundaryLookAndFeel
	{
		public Color4 Color;
	}

	/// <summary>
	/// InputState describes the complete controller input state, including Oculus Touch,
	/// and XBox gamepad. If multiple inputs are connected and used at the same time,
	/// their inputs are combined.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct InputState
	{
		/// <summary>
		/// System type when the controller state was last updated.
		/// </summary>
		public double TimeInSeconds;

		/// <summary>
		/// Values for buttons described by ovrButton.
		/// </summary>
		public uint Buttons;

		/// <summary>
		/// Touch values for buttons and sensors as described by ovrTouch.
		/// </summary>
		public uint Touches;

		/// <summary>
		/// Left and right finger trigger values (Hand.Left and Hand.Right), in the range 0.0 to 1.0f.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public float[] IndexTrigger;

		/// <summary>
		/// Left and right hand trigger values (Hand.Left and Hand.Right), in the range 0.0 to 1.0f.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public float[] HandTrigger;

		/// <summary>
		/// Horizontal and vertical thumbstick axis values (Hand.Left and Hand.Right), in the range -1.0f to 1.0f.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public Vector2[] Thumbstick;

		/// <summary>
		/// The type of the controller this state is for.
		/// </summary>
		public ControllerType ControllerType;

		/// Left and right finger trigger values (ovrHand_Left and ovrHand_Right), in the range 0.0 to 1.0f.
		/// Does not apply a deadzone.  Only touch applies a filter.
		/// This has been formally named simply "Trigger". We retain the name IndexTrigger for backwards code compatibility.
		/// User-facing documentation should refer to it as the Trigger.
		/// Added in 1.7
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public float[] IndexTriggerNoDeadzone;

		/// Left and right hand trigger values (ovrHand_Left and ovrHand_Right), in the range 0.0 to 1.0f.
		/// Does not apply a deadzone. Only touch applies a filter.
		/// This has been formally named "Grip Button". We retain the name HandTrigger for backwards code compatibility.
		/// User-facing documentation should refer to it as the Grip Button or simply Grip.
		/// Added in 1.7
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public float[] HandTriggerNoDeadzone;

		/// Horizontal and vertical thumbstick axis values (ovrHand_Left and ovrHand_Right), in the range -1.0f to 1.0f
		/// Does not apply a deadzone or filter.
		/// Added in 1.7
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public Vector2[] ThumbstickNoDeadzone;

		/// Left and right finger trigger values (ovrHand_Left and ovrHand_Right), in the range 0.0 to 1.0f.
		/// No deadzone or filter
		/// This has been formally named "Grip Button". We retain the name HandTrigger for backwards code compatibility.
		/// User-facing documentation should refer to it as the Grip Button or simply Grip.
		/// Added in 1.11
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public float[] IndexTriggerRaw;

		/// Left and right hand trigger values (ovrHand_Left and ovrHand_Right), in the range 0.0 to 1.0f.
		/// No deadzone or filter
		/// This has been formally named "Grip Button". We retain the name HandTrigger for backwards code compatibility.
		/// User-facing documentation should refer to it as the Grip Button or simply Grip.
		/// Added in 1.11
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public float[] HandTriggerRaw;

		/// Horizontal and vertical thumbstick axis values (ovrHand_Left and ovrHand_Right), in the range -1.0f to 1.0f
		/// No deadzone or filter
		/// Added in 1.11
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public Vector2[] ThumbstickRaw;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct InitParams
	{
		/// Flags from ovrInitFlags to override default behavior.
		/// Use 0 for the defaults.
		public InitFlags Flags;

		/// Requests a specific minor version of the LibOVR runtime.
		/// Flags must include ovrInit_RequestVersion or this will be ignored and OVR_MINOR_VERSION 
		/// will be used. If you are directly calling the LibOVRRT version of ovr_Initialize
		/// in the LibOVRRT DLL then this must be valid and include ovrInit_RequestVersion.
		public UInt32 RequestedMinorVersion;

		/// User-supplied log callback function, which may be called at any time
		/// asynchronously from multiple threads until ovr_Shutdown completes.
		/// Use NULL to specify no log callback.
		public LogFunction LogCallback;

		/// User-supplied data which is passed as-is to LogCallback. Typically this
		/// is used to store an application-specific pointer which is read in the
		/// callback function.
		public IntPtr UserData;

		/// Relative number of milliseconds to wait for a connection to the server
		/// before failing. Use 0 for the default timeout.
		public UInt32 ConnectionTimeoutMS;
	}

	/// <summary>
	/// Return values for ovr_Detect.
	/// </summary>
	/// <see cref="OVRBase.Detect"/>
	[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 8)]
	public struct DetectResult
	{
		/// <summary>
		/// Is ovrFalse when the Oculus Service is not running.
		///   This means that the Oculus Service is either uninstalled or stopped.
		///   IsOculusHMDConnected will be ovrFalse in this case.
		/// Is ovrTrue when the Oculus Service is running.
		///   This means that the Oculus Service is installed and running.
		///   IsOculusHMDConnected will reflect the state of the HMD.
		/// </summary>
		public ovrBool IsOculusServiceRunning;

		/// <summary>
		/// Is ovrFalse when an Oculus HMD is not detected.
		///   If the Oculus Service is not running, this will be ovrFalse.
		/// Is ovrTrue when an Oculus HMD is detected.
		///   This implies that the Oculus Service is also installed and running.
		/// </summary>
		public ovrBool IsOculusHMDConnected;
	}


	/// <summary>
	/// Specifies status information for the current session.
	/// </summary>
	/// <see cref="OVRBase.GetSessionStatus"/>
	[StructLayout(LayoutKind.Sequential)]
	public struct SessionStatus
	{
		/// <summary>
		/// True if the process has VR focus and thus is visible in the HMD.
		/// </summary>
		public ovrBool IsVisible;

		/// <summary>
		/// True if an HMD is present.
		/// </summary>
		public ovrBool HmdPresent;

		/// <summary>
		/// True if the HMD is on the user's head.
		/// </summary>
		public ovrBool HmdMounted;

		/// <summary>
		/// True if the session is in a display-lost state. See ovr_SubmitFrame.
		/// </summary>
		public ovrBool DisplayLost;

		/// <summary>
		/// True if the application should initiate shutdown.    
		/// </summary>
		public ovrBool ShouldQuit;

		/// <summary>
		/// True if UX has requested re-centering. 
		/// Must call ovr_ClearShouldRecenterFlag or ovr_RecenterTrackingOrigin.
		/// </summary>
		public ovrBool ShouldRecenter;
	}


	/// <summary>
	/// Defines properties shared by all ovrLayer structs, such as LayerEyeFov.
	///
	/// LayerHeader is used as a base member in these larger structs.
	/// This struct cannot be used by itself except for the case that Type is LayerType_Disabled.
	/// </summary>
	/// <see cref="LayerType"/>
	/// <see cref="LayerFlags"/>
	[StructLayout(LayoutKind.Sequential)]
	public struct LayerHeader
	{
		/// <summary>
		/// Described by LayerType.
		/// </summary>
		public LayerType Type;

		/// <summary>
		/// Described by LayerFlags.
		/// </summary>
		public LayerFlags Flags;
	}

	/// <summary>
	/// Describes a layer that specifies a monoscopic or stereoscopic view.
	/// 
	/// This is the kind of layer that's typically used as layer 0 to ovr_SubmitFrame,
	/// as it is the kind of layer used to render a 3D stereoscopic view.
	///
	/// Three options exist with respect to mono/stereo texture usage:
	///    - ColorTexture[0] and ColorTexture[1] contain the left and right stereo renderings, respectively. 
	///      Viewport[0] and Viewport[1] refer to ColorTexture[0] and ColorTexture[1], respectively.
	///    - ColorTexture[0] contains both the left and right renderings, ColorTexture[1] is NULL, 
	///      and Viewport[0] and Viewport[1] refer to sub-rects with ColorTexture[0].
	///    - ColorTexture[0] contains a single monoscopic rendering, and Viewport[0] and 
	///      Viewport[1] both refer to that rendering.
	/// </summary>
	/// <see cref="TextureSwapChain"/>
	/// <see cref="OVRBase.SubmitFrame"/>
	[StructLayout(LayoutKind.Sequential)]
	public struct LayerEyeFov
	{
		/// <summary>
		/// Header.Type must be LayerType_EyeFov.
		/// </summary>
		public LayerHeader Header;

		/// <summary>
		/// TextureSwapChains for the left and right eye respectively.
		/// 
		/// The second one of which can be null for cases described above.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public IntPtr[] ColorTexture;

		/// <summary>
		/// Specifies the ColorTexture sub-rect UV coordinates.
		/// 
		/// Both Viewport[0] and Viewport[1] must be valid.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public Recti[] Viewport;

		/// <summary>
		/// The viewport field of view.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public FovPort[] Fov;

		/// <summary>
		/// Specifies the position and orientation of each eye view, with the position specified in meters.
		/// RenderPose will typically be the value returned from ovr_CalcEyePoses,
		/// but can be different in special cases if a different head pose is used for rendering.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public Posef[] RenderPose;

		/// <summary>
		/// Specifies the timestamp when the source ovrPosef (used in calculating RenderPose)
		/// was sampled from the SDK. Typically retrieved by calling ovr_GetTimeInSeconds
		/// around the instant the application calls ovr_GetTrackingState
		/// The main purpose for this is to accurately track app tracking latency.
		/// </summary>
		public double SensorSampleTime;
	}

	/// <summary>
	/// Describes a layer that specifies a monoscopic or stereoscopic view.
	/// This uses a direct 3x4 matrix to map from view space to the UV coordinates.
	/// It is essentially the same thing as ovrLayerEyeFov but using a much
	/// lower level. This is mainly to provide compatibility with specific apps.
	/// Unless the application really requires this flexibility, it is usually better
	/// to use ovrLayerEyeFov.
	///
	/// Three options exist with respect to mono/stereo texture usage:
	///    - ColorTexture[0] and ColorTexture[1] contain the left and right stereo renderings, respectively.
	///      Viewport[0] and Viewport[1] refer to ColorTexture[0] and ColorTexture[1], respectively.
	///    - ColorTexture[0] contains both the left and right renderings, ColorTexture[1] is null,
	///      and Viewport[0] and Viewport[1] refer to sub-rects with ColorTexture[0].
	///    - ColorTexture[0] contains a single monoscopic rendering, and Viewport[0] and
	///      Viewport[1] both refer to that rendering.
	/// </summary>
	/// <see cref="TextureSwapChain"/>
	/// <see cref="OVRBase.SubmitFrame"/>
	[StructLayout(LayoutKind.Sequential)]
	public struct LayerEyeMatrix
	{
		/// <summary>
		/// Header.Type must be ovrLayerType_EyeMatrix.
		/// </summary>
		public LayerHeader Header;

		/// <summary>
		/// TextureSwapChains for the left and right eye respectively.
		/// 
		/// The second one of which can be NULL for cases described above.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public IntPtr[] ColorTexture;

		/// <summary>
		/// Specifies the ColorTexture sub-rect UV coordinates.
		/// Both Viewport[0] and Viewport[1] must be valid.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public Recti[] Viewport;

		/// <summary>
		/// Specifies the position and orientation of each eye view, with the position specified in meters.
		/// RenderPose will typically be the value returned from ovr_CalcEyePoses,
		/// but can be different in special cases if a different head pose is used for rendering.
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public Posef[] RenderPose;

		/// <summary>
		/// Specifies the mapping from a view-space vector
		/// to a UV coordinate on the textures given above.
		/// P = (x,y,z,1)*Matrix
		/// TexU  = P.x/P.z
		/// TexV  = P.y/P.z
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
		public Matrix4[] Matrix;

		/// <summary>
		/// Specifies the timestamp when the source ovrPosef (used in calculating RenderPose)
		/// was sampled from the SDK. Typically retrieved by calling ovr_GetTimeInSeconds
		/// around the instant the application calls ovr_GetTrackingState
		/// The main purpose for this is to accurately track app tracking latency.
		/// </summary>
		public double SensorSampleTime;
	};

	/// <summary>
	/// Describes a layer of Quad type, which is a single quad in world or viewer space.
	/// It is used for ovrLayerType_Quad. This type of layer represents a single
	/// object placed in the world and not a stereo view of the world itself.
	///
	/// A typical use of ovrLayerType_Quad is to draw a television screen in a room
	/// that for some reason is more convenient to draw as a layer than as part of the main
	/// view in layer 0. For example, it could implement a 3D popup GUI that is drawn at a
	/// higher resolution than layer 0 to improve fidelity of the GUI.
	///
	/// Quad layers are visible from both sides; they are not back-face culled.
	/// </summary>
	/// <see cref="TextureSwapChain"/>
	/// <see cref="OVRBase.SubmitFrame"/>
	[StructLayout(LayoutKind.Sequential)]
	public struct LayerQuad
	{
		/// <summary>
		/// Header.Type must be ovrLayerType_Quad.
		/// </summary>
		public LayerHeader Header;

		/// <summary>
		/// Contains a single image, never with any stereo view.
		/// </summary>
		public IntPtr ColorTexture;

		/// <summary>
		/// Specifies the ColorTexture sub-rect UV coordinates.
		/// </summary>
		public Recti Viewport;

		/// <summary>
		/// Specifies the orientation and position of the center point of a Quad layer type.
		/// The supplied direction is the vector perpendicular to the quad.
		/// The position is in real-world meters (not the application's virtual world,
		/// the physical world the user is in) and is relative to the "zero" position
		/// set by ovr_RecenterTrackingOrigin unless the ovrLayerFlag_HeadLocked flag is used.
		/// </summary>
		public Posef QuadPoseCenter;

		/// <summary>
		/// Width and height (respectively) of the quad in meters.
		/// </summary>
		public Vector2 QuadSize;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PerfStatsPerCompositorFrame
	{
		///
		/// Vsync Frame Index - increments with each HMD vertical synchronization signal (i.e. vsync or refresh rate)
		/// If the compositor drops a frame, expect this value to increment more than 1 at a time.
		///
		public int HmdVsyncIndex;

		///
		/// Application stats
		///

		/// Index that increments with each successive ovr_SubmitFrame call
		public int AppFrameIndex;

		/// If the app fails to call ovr_SubmitFrame on time, then expect this value to increment with each missed frame
		public int AppDroppedFrameCount;

		/// Motion-to-photon latency for the application
		/// This value is calculated by either using the SensorSampleTime provided for the ovrLayerEyeFov or if that
		/// is not available, then the call to ovr_GetTrackingState which has latencyMarker set to ovrTrue
		public float AppMotionToPhotonLatency;

		/// Amount of queue-ahead in seconds provided to the app based on performance and overlap of CPU & GPU utilization
		/// A value of 0.0 would mean the CPU & GPU workload is being completed in 1 frame's worth of time, while
		/// 11 ms (on the CV1) of queue ahead would indicate that the app's CPU workload for the next frame is
		/// overlapping the app's GPU workload for the current frame.
		public float AppQueueAheadTime;

		/// Amount of time in seconds spent on the CPU by the app's render-thread that calls ovr_SubmitFrame
		/// Measured as elapsed time between from when app regains control from ovr_SubmitFrame to the next time the app
		/// calls ovr_SubmitFrame.
		public float AppCpuElapsedTime;

		/// Amount of time in seconds spent on the GPU by the app
		/// Measured as elapsed time between each ovr_SubmitFrame call using GPU timing queries.
		public float AppGpuElapsedTime;

		///
		/// SDK Compositor stats
		///

		/// Index that increments each time the SDK compositor completes a distortion and timewarp pass
		/// Since the compositor operates asynchronously, even if the app calls ovr_SubmitFrame too late,
		/// the compositor will kick off for each vsync.
		public int CompositorFrameIndex;

		/// Increments each time the SDK compositor fails to complete in time
		/// This is not tied to the app's performance, but failure to complete can be tied to other factors
		/// such as OS capabilities, overall available hardware cycles to execute the compositor in time
		/// and other factors outside of the app's control.
		public int CompositorDroppedFrameCount;

		/// Motion-to-photon latency of the SDK compositor in seconds
		/// This is the latency of timewarp which corrects the higher app latency as well as dropped app frames.
		public float CompositorLatency;

		/// The amount of time in seconds spent on the CPU by the SDK compositor. Unless the VR app is utilizing
		/// all of the CPU cores at their peak performance, there is a good chance the compositor CPU times
		/// will not affect the app's CPU performance in a major way.
		public float CompositorCpuElapsedTime;

		/// The amount of time in seconds spent on the GPU by the SDK compositor. Any time spent on the compositor
		/// will eat away from the available GPU time for the app.
		public float CompositorGpuElapsedTime;

		/// The amount of time in seconds spent from the point the CPU kicks off the compositor to the point in time
		/// the compositor completes the distortion & timewarp on the GPU. In the event the GPU time is not
		/// available, expect this value to be -1.0f
		public float CompositorCpuStartToGpuEndElapsedTime;

		/// The amount of time in seconds left after the compositor is done on the GPU to the associated V-Sync time.
		/// In the event the GPU time is not available, expect this value to be -1.0f
		public float CompositorGpuEndToVsyncElapsedTime;

		///
		/// Async Spacewarp stats (ASW)
		///

		/// Will be true is ASW is active for the given frame such that the application is being forced into
		/// half the frame-rate while the compositor continues to run at full frame-rate
		public ovrBool AswIsActive;

		/// Accumulates each time ASW it activated where the app was forced in and out of half-rate rendering
		public int AswActivatedToggleCount;

		/// Accumulates the number of frames presented by the compositor which had extrapolated ASW frames presented
		public int AswPresentedFrameCount;

		/// Accumulates the number of frames that the compositor tried to present when ASW is active but failed
		public int AswFailedFrameCount;

	}


	[StructLayout(LayoutKind.Sequential)]
	public struct PerfStats
	{
		/// FrameStatsCount will have a maximum value set by ovrMaxProvidedFrameStats
		/// If the application calls ovr_GetPerfStats at the native refresh rate of the HMD
		/// then FrameStatsCount will be 1. If the app's workload happens to force
		/// ovr_GetPerfStats to be called at a lower rate, then FrameStatsCount will be 2 or more.
		/// If the app does not want to miss any performance data for any frame, it needs to
		/// ensure that it is calling ovr_SubmitFrame and ovr_GetPerfStats at a rate that is at least:
		/// "HMD_refresh_rate / ovrMaxProvidedFrameStats". On the Oculus Rift CV1 HMD, this will
		/// be equal to 18 times per second.
		///
		/// The performance entries will be ordered in reverse chronological order such that the
		/// first entry will be the most recent one.
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
		public PerfStatsPerCompositorFrame[] FrameStats;

		public int FrameStatsCount;

		/// If the app calls ovr_SubmitFrame at a rate less than 18 fps, then when calling
		/// ovr_GetPerfStats, expect AnyFrameStatsDropped to become ovrTrue while FrameStatsCount
		/// is equal to ovrMaxProvidedFrameStats.
		public ovrBool AnyFrameStatsDropped;

		/// AdaptiveGpuPerformanceScale is an edge-filtered value that a caller can use to adjust
		/// the graphics quality of the application to keep the GPU utilization in check. The value
		/// is calculated as: (desired_GPU_utilization / current_GPU_utilization)
		/// As such, when this value is 1.0, the GPU is doing the right amount of work for the app.
		/// Lower values mean the app needs to pull back on the GPU utilization.
		/// If the app is going to directly drive render-target resolution using this value, then
		/// be sure to take the square-root of the value before scaling the resolution with it.
		/// Changing render target resolutions however is one of the many things an app can do
		/// increase or decrease the amount of GPU utilization.
		/// Since AdaptiveGpuPerformanceScale is edge-filtered and does not change rapidly
		/// (i.e. reports non-1.0 values once every couple of seconds) the app can make the
		/// necessary adjustments and then keep watching the value to see if it has been satisfied.
		public float AdaptiveGpuPerformanceScale;

		/// Will be true if Async Spacewarp (ASW) is available for this system which is dependent on
		/// several factors such as choice of GPU, OS and debug overrides
		public ovrBool AswIsAvailable;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ErrorInfo
	{
		public Result result;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
		public byte[] ErrorString;
	}
	#endregion
}
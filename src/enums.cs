using System;
using System.Runtime.InteropServices;
using ovrBool = System.Byte;
using ovrTextureSwapChain = System.IntPtr;

namespace Oculus
{
	public enum Result : Int32
	{
		#region Successful results.
		/// <summary>
		/// This is a general success result. 
		/// </summary>
		Success = 0,

		/// <summary>
		/// Returned from a call to SubmitFrame. The call succeeded, but what the app
		/// rendered will not be visible on the HMD. Ideally the app should continue
		/// calling SubmitFrame, but not do any rendering. When the result becomes
		/// ovrSuccess, rendering should continue as usual.
		/// </summary>
		SuccessNotVisible = 1000,

		SuccessBoundaryInvalid = 1001,  ///< Boundary is invalid due to sensor change or was not setup.
		SuccessDeviceUnavailable = 1002,  ///< Device is not available for the requested operation.
		#endregion

		#region General errors
		/* General errors */
		MemoryAllocationFailure = -1000,   ///< Failure to allocate memory.
		InvalidSession = -1002,   ///< Invalid ovrSession parameter provided.
		Timeout = -1003,   ///< The operation timed out.
		NotInitialized = -1004,   ///< The system or component has not been initialized.
		InvalidParameter = -1005,   ///< Invalid parameter provided. See error info or log for details.
		ServiceError = -1006,   ///< Generic service error. See error info or log for details.
		NoHmd = -1007,   ///< The given HMD doesn't exist.
		Unsupported = -1009,   ///< Function call is not supported on this hardware/software
		DeviceUnavailable = -1010,   ///< Specified device type isn't available.
		InvalidHeadsetOrientation = -1011,   ///< The headset was in an invalid orientation for the requested operation (e.g. vertically oriented during ovr_RecenterPose).
		ClientSkippedDestroy = -1012,   ///< The client failed to call ovr_Destroy on an active session before calling ovr_Shutdown. Or the client crashed.
		ClientSkippedShutdown = -1013,   ///< The client failed to call ovr_Shutdown or the client crashed.
		ServiceDeadlockDetected = -1014,   ///< The service watchdog discovered a deadlock.
		InvalidOperation = -1015,   ///< Function call is invalid for object's current state

		/* Audio error range, reserved for Audio errors. */
		AudioDeviceNotFound = -2001,   ///< Failure to find the specified audio device.
		AudioComError = -2002,   ///< Generic COM error.

		/* Initialization errors. */
		Initialize = -3000,   ///< Generic initialization error.
		LibLoad = -3001,   ///< Couldn't load LibOVRRT.
		LibVersion = -3002,   ///< LibOVRRT version incompatibility.
		ServiceConnection = -3003,   ///< Couldn't connect to the OVR Service.
		ServiceVersion = -3004,   ///< OVR Service version incompatibility.
		IncompatibleOS = -3005,   ///< The operating system version is incompatible.
		DisplayInit = -3006,   ///< Unable to initialize the HMD display.
		ServerStart = -3007,   ///< Unable to start the server. Is it already running?
		Reinitialization = -3008,   ///< Attempting to re-initialize with a different version.
		MismatchedAdapters = -3009,   ///< Chosen rendering adapters between client and service do not match
		LeakingResources = -3010,   ///< Calling application has leaked resources
		ClientVersion = -3011,   ///< Client version too old to connect to service
		OutOfDateOS = -3012,   ///< The operating system is out of date.
		OutOfDateGfxDriver = -3013,   ///< The graphics driver is out of date.
		IncompatibleGPU = -3014,   ///< The graphics hardware is not supported
		NoValidVRDisplaySystem = -3015,   ///< No valid VR display system found.
		Obsolete = -3016,   ///< Feature or API is obsolete and no longer supported.
		DisabledOrDefaultAdapter = -3017,   ///< No supported VR display system found, but disabled or driverless adapter found.
		HybridGraphicsNotSupported = -3018,   ///< The system is using hybrid graphics (Optimus, etc...), which is not support.
		DisplayManagerInit = -3019,   ///< Initialization of the DisplayManager failed.
		TrackerDriverInit = -3020,   ///< Failed to get the interface for an attached tracker
		LibSignCheck = -3021,   ///< LibOVRRT signature check failure.
		LibPath = -3022,   ///< LibOVRRT path failure.
		LibSymbols = -3023,   ///< LibOVRRT symbol resolution failure.
		RemoteSession = -3024,   ///< Failed to connect to the service because remote connections to the service are not allowed.

		/* Rendering errors */
		DisplayLost = -6000,   ///< In the event of a system-wide graphics reset or cable unplug this is returned to the app.
		TextureSwapChainFull = -6001,   ///< ovr_CommitTextureSwapChain was called too many times on a texture swapchain without calling submit to use the chain.
		TextureSwapChainInvalid = -6002,   ///< The ovrTextureSwapChain is in an incomplete or inconsistent state. Ensure ovr_CommitTextureSwapChain was called at least once first.
		GraphicsDeviceReset = -6003,   ///< Graphics device has been reset (TDR, etc...)
		DisplayRemoved = -6004,   ///< HMD removed from the display adapter
		ContentProtectionNotAvailable = -6005,///<Content protection is not available for the display
		ApplicationInvisible = -6006,   ///< Application declared itself as an invisible type and is not allowed to submit frames.
		Disallowed = -6007,   ///< The given request is disallowed under the current conditions.
		DisplayPluggedIncorrectly = -6008,   ///< Display portion of HMD is plugged into an incompatible port (ex: IGP)

		/* Fatal errors */
		RuntimeException = -7000,   ///< A runtime exception occurred. The application is required to shutdown LibOVR and re-initialize it before this error state will be cleared.

		/* Calibration errors */
		NoCalibration = -9000,   ///< Result of a missing calibration block
		OldVersion = -9001,   ///< Result of an old calibration block
		MisformattedBlock = -9002,   ///< Result of a bad calibration block due to lengths

		#endregion
	}


	#region Flags
	/// <summary>
	/// Enumerates all HMD types that we support.
	/// </summary>
	public enum HmdType : UInt32
	{
		None = 0,
		DK1 = 3,
		DKHD = 4,
		DK2 = 6,
		CB = 8,
		Other = 9,
		E3_2015 = 10,
		ES06 = 11,
		ES09 = 12,
		ES11 = 13,
		CV1 = 14,
	}

	/// <summary>
	/// HMD capability bits reported by device.
	/// </summary>
	public enum HmdCaps :UInt32
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,

		// Read only flags

		/// <summary>
		/// Means HMD device is a virtual debug device.
		/// </summary>
		/// <remarks>
		/// (read only) 
		/// </remarks>
		DebugDevice = 0x0010,
	}

	/// <summary>
	/// Tracking capability bits reported by the device.
	/// Used with ovr_GetTrackingCaps.
	/// </summary>
	[Flags]
	public enum TrackingCaps : UInt32
	{
		/// <summary>
		/// Supports orientation tracking (IMU).
		/// </summary>
		Orientation = 0x0010,

		/// <summary>
		/// Supports yaw drift correction via a magnetometer or other means.
		/// </summary>
		MagYawCorrection = 0x0020,

		/// <summary>
		/// Supports positional tracking.
		/// </summary>
		Position = 0x0040,
	}

	/// <summary>
	/// Specifies which eye is being used for rendering.
	/// This type explicitly does not include a third "NoStereo" option, as such is
	/// not required for an HMD-centered API.
	/// </summary>
	public enum EyeType : UInt32
	{
		Left = 0,
		Right = 1,
		Count = 2
	}

	/// <summary>
	/// Specifies the coordinate system TrackingState returns tracking poses in.
	/// Used with ovr_SetTrackingOriginType()
	/// </summary>
	public enum TrackingOrigin :UInt32
	{
		/// <summary>
		/// Tracking system origin reported at eye (HMD) height
		/// 
		/// Prefer using this origin when your application requires
		/// matching user's current physical head pose to a virtual head pose
		/// without any regards to a the height of the floor. Cockpit-based,
		/// or 3rd-person experiences are ideal candidates.
		/// 
		/// When used, all poses in TrackingState are reported as an offset
		/// transform from the profile calibrated or recentered HMD pose.
		/// It is recommended that apps using this origin type call ovr_RecenterTrackingOrigin
		/// prior to starting the VR experience, but notify the user before doing so
		/// to make sure the user is in a comfortable pose, facing a comfortable
		/// direction.
		/// </summary>
		EyeLevel = 0,

		/// <summary>
		/// Tracking system origin reported at floor height
		/// 
		/// Prefer using this origin when your application requires the
		/// physical floor height to match the virtual floor height, such as
		/// standing experiences.
		/// 
		/// When used, all poses in TrackingState are reported as an offset
		/// transform from the profile calibrated floor pose. Calling ovr_RecenterTrackingOrigin
		/// will recenter the X &amp; Z axes as well as yaw, but the Y-axis (i.e. height) will continue
		/// to be reported using the floor height as the origin for all poses.
		/// </summary>
		FloorLevel = 1,

		/// <summary>
		/// Count of enumerated elements.
		/// </summary>
		Count = 2,
	}


	/// <summary>
	/// Bit flags describing the current status of sensor tracking.
	/// The values must be the same as in enum StatusBits
	/// </summary>
	/// <see cref="TrackingState"/>
	[Flags]
	public enum StatusBits : UInt32
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0x0000,

		/// <summary>
		/// Orientation is currently tracked (connected and in use).
		/// </summary>
		OrientationTracked = 0x0001,

		/// <summary>
		/// Position is currently tracked (false if out of range).
		/// </summary>
		PositionTracked = 0x0002,
	}

	/// <summary>
	/// Specifies sensor flags.
	/// </summary>
	/// <see cref="TrackerPose"/>
	[Flags]
	public enum TrackerFlags : UInt32
	{
		/// <summary>
		/// The sensor is present, else the sensor is absent or offline.
		/// </summary>
		Connected = 0x0020,

		/// <summary>
		/// The sensor has a valid pose, else the pose is unavailable. 
		/// This will only be set if TrackerFlags.Connected is set.
		/// </summary>
		PoseTracked = 0x0004,
	}

	/// <summary>
	/// The type of texture resource.
	/// </summary>
	/// <see cref="TextureSwapChainDesc"/>
	public enum TextureType : UInt32
	{
		/// <summary>
		/// 2D textures.
		/// </summary>
		Texture2D,

		/// <summary>
		/// External 2D texture. 
		/// 
		/// Not used on PC.
		/// </summary>
		Texture2DExternal,

		/// <summary>
		/// Cube maps. 
		/// 
		/// Not currently supported on PC.
		/// </summary>
		TextureCube,

		/// <summary>
		/// Undocumented.
		/// </summary>
		TextureCount,
	}

	/// <summary>
	/// The bindings required for texture swap chain.
	///
	/// All texture swap chains are automatically bindable as shader
	/// input resources since the Oculus runtime needs this to read them.
	/// </summary>
	/// <see cref="TextureSwapChainDesc"/>
	[Flags]
	public enum TextureBindFlags : UInt32
	{
		None,

		/// <summary>
		/// The application can write into the chain with pixel shader.
		/// </summary>
		DX_RenderTarget = 0x0001,

		/// <summary>
		/// The application can write to the chain with compute shader.
		/// </summary>
		DX_UnorderedAccess = 0x0002,

		/// <summary>
		/// The chain buffers can be bound as depth and/or stencil buffers.
		/// </summary>
		DX_DepthStencil = 0x0004,
	}

	/// <summary>
	/// The format of a texture.
	/// </summary>
	/// <see cref="TextureSwapChainDesc"/>
	public enum TextureFormat : UInt32
	{
		UNKNOWN = 0,

		/// <summary>
		/// Not currently supported on PC. Would require a DirectX 11.1 device.
		/// </summary>
		B5G6R5_UNORM = 1,

		/// <summary>
		/// Not currently supported on PC. Would require a DirectX 11.1 device.
		/// </summary>
		B5G5R5A1_UNORM = 2,

		/// <summary>
		/// Not currently supported on PC. Would require a DirectX 11.1 device.
		/// </summary>
		B4G4R4A4_UNORM = 3,

		R8G8B8A8_UNORM = 4,
		R8G8B8A8_UNORM_SRGB = 5,
		B8G8R8A8_UNORM = 6,

		/// <summary>
		/// Not supported for OpenGL applications
		/// </summary>
		B8G8R8A8_UNORM_SRGB = 7,

		/// <summary>
		/// Not supported for OpenGL applications
		/// </summary>
		B8G8R8X8_UNORM = 8,

		/// <summary>
		/// Not supported for OpenGL applications
		/// </summary>
		B8G8R8X8_UNORM_SRGB = 9,

		R16G16B16A16_FLOAT = 10,
		R11G11B10_FLOAT = 25,

		// Depth formats
		D16_UNORM = 11,
		D24_UNORM_S8_UINT = 12,
		D32_FLOAT = 13,
		D32_FLOAT_S8X24_UINT =14,

		// Added in 1.5 compressed formats can be used for static layers
		BC1_UNORM = 15,
		BC1_UNORM_SRGB = 16,
		BC2_UNORM = 17,
		BC2_UNROM_SRGB = 18,
		BC3_UNORM = 19,
		BC3_UNORM_SRGB = 20,
		BC6H_UF16 = 21,
		BC6H_SF16 = 22,
		BC7_UNORM = 23,
		BC7_UNORM_SRGB = 24
	}

	/// <summary>
	/// Misc flags overriding particular behaviors of a texture swap chain
	/// </summary>
	/// <see cref="TextureSwapChainDesc"/>
	[Flags]
	public enum TextureMiscFlags : UInt32
	{
		None = 0,

		/// <summary>
		/// DX only: The underlying texture is created with a TYPELESS equivalent of the
		/// format specified in the texture desc. The SDK will still access the
		/// texture using the format specified in the texture desc, but the app can
		/// create views with different formats if this is specified.
		/// </summary>
		DX_Typeless = 0x0001,

		/// <summary>
		/// DX only: Allow generation of the mip chain on the GPU via the GenerateMips
		/// call. This flag requires that RenderTarget binding also be specified.
		/// </summary>
		AllowGenerateMips = 0x0002,

		/// Texture swap chain contains protected content, and requires
		/// HDCP connection in order to display to HMD. Also prevents
		/// mirroring or other redirection of any frame containing this contents
		ProtectedContent = 0x0004
	}

	/// <summary>
	/// Describes button input types.
	/// Button inputs are combined; that is they will be reported as pressed if they are 
	/// pressed on either one of the two devices.
	/// The ovrButton_Up/Down/Left/Right map to both XBox D-Pad and directional buttons.
	/// The ovrButton_Enter and ovrButton_Return map to Start and Back controller buttons, respectively.
	/// </summary>
	[Flags]
	public enum Button : UInt32
	{
		A = 0x00000001,
		B = 0x00000002,
		RThumb = 0x00000004,
		RShoulder = 0x00000008,

		/// <summary>
		/// Bit mask of all buttons on the right Touch controller
		/// </summary>
		RMask = A | B | RThumb | RShoulder,

		X = 0x00000100,
		Y = 0x00000200,
		LThumb = 0x00000400,
		LShoulder = 0x00000800,

		/// <summary>
		/// Bit mask of all buttons on the left Touch controller
		/// </summary>
		LMask = X | Y | LThumb | LShoulder,

		// Navigation through DPad.
		Up = 0x00010000,
		Down = 0x00020000,
		Left = 0x00040000,
		Right = 0x00080000,

		/// <summary>
		/// Start on XBox controller.
		/// </summary>
		Enter = 0x00100000,

		/// <summary>
		/// Back on Xbox controller.
		/// </summary>
		Back = 0x00200000,

		/// <summary>
		/// Only supported by Remote.
		/// </summary>
		VolUp = 0x00400000,

		/// <summary>
		/// Only supported by Remote.
		/// </summary>
		VolDown = 0x00800000,

		Home = 0x01000000,
		Private = VolUp | VolDown | Home,
	}

	/// <summary>
	/// Describes touch input types.
	/// These values map to capacitive touch values reported ovrInputState::Touch.
	/// Some of these values are mapped to button bits for consistency.
	/// </summary>
	[Flags]
	public enum Touch : UInt32
	{
		A = Button.A,
		B = Button.B,
		RThumb = Button.RThumb,
		RIndexTrigger = 0x00000010,

		/// <summary>
		/// Bit mask of all the button touches on the right controller
		/// </summary>
		RButtonMask = A | B | RThumb | RIndexTrigger,

		X = Button.X,
		Y = Button.Y,
		LThumb = Button.LThumb,
		LIndexTrigger = 0x00001000,

		/// <summary>
		/// Bit mask of all the button touches on the left controller
		/// </summary>
		LButtonMask = X | Y | LThumb | LIndexTrigger,

		// Finger pose state 
		// Derived internally based on distance, proximity to sensors and filtering.
		RIndexPointing = 0x00000020,
		RThumbUp = 0x00000040,

		/// <summary>
		/// Bit mask of all right controller poses
		/// </summary>
		RPoseMask = RIndexPointing | RThumbUp,

		LIndexPointing = 0x00002000,
		LThumbUp = 0x00004000,

		/// <summary>
		/// Bit mask of all left controller poses
		/// </summary>
		LPoseMask = LIndexPointing | LThumbUp,
	}

	/// <summary>
	/// Specifies which controller is connected; multiple can be connected at once.
	/// </summary>
	[Flags]
	public enum ControllerType : UInt32
	{
		None = 0x00,
		LTouch = 0x01,
		RTouch = 0x02,
		Touch = LTouch | RTouch,
		Remote = 0x04,

		XBox = 0x10,

		Object0 = 0x0100,
		Object1 = 0x0200,
		Object2 = 0x0400,
		Object3 = 0x0800,


		/// <summary>
		/// Operate on or query whichever controller is active.
		/// </summary>
		Active = 0xffffffff
	}

	/// <summary>
	/// Haptics buffer submit mode
	/// </summary>
	[Flags]
	public enum HapticsBufferSubmitMode : UInt32
	{
		Enqueue
	}

	/// <summary>
	/// Position tracked devices
	/// </summary>
	[Flags]
	public enum TrackedDeviceType : UInt32
	{
		HMD = 0x0001,
		LTouch = 0x0002,
		RTouch = 0x0004,
		Touch = (LTouch | RTouch),

		Object0 = 0x0010,
		Object1 = 0x0020,
		Object2 = 0x0040,
		Object3 = 0x0080,

		All = 0xFFFF,
	}

	/// <summary>
	/// Boundary types that specified while using the boundary system
	/// </summary>
	[Flags]
	public enum BoundaryType : UInt32
	{
		Outer = 0x0001,
		PlayerArea = 0x0100,
	}



	/// <summary>
	/// Provides names for the left and right hand array indexes.
	/// </summary>
	/// <see cref="InputState"/>
	/// <seealso cref="TrackingState"/>
	public enum HandType : UInt32
	{
		Left = 0,
		Right = 1,
		Count = 2,
	}

	[Flags]
	public enum InitFlags : UInt32
	{
		None = 0x0,

		/// When a debug library is requested, a slower debugging version of the library will
		/// run which can be used to help solve problems in the library and debug application code.
		Debug = 0x00000001,

		/// When a version is requested, the LibOVR runtime respects the RequestedMinorVersion
		/// field and verifies that the RequestedMinorVersion is supported. Normally when you 
		/// specify this flag you simply use OVR_MINOR_VERSION for ovrInitParams::RequestedMinorVersion,
		/// though you could use a lower version than OVR_MINOR_VERSION to specify previous 
		/// version behavior.
		RequestVersion = 0x00000004,

		/// This client will not be visible in the HMD.
		/// Typically set by diagnostic or debugging utilities.
		Invisible = 0x00000010,

		/// This client will alternate between VR and 2D rendering.
		/// Typically set by game engine editors and VR-enabled web browsers.
		MixedRendering = 0x00000020,

		/// These bits are writable by user code.
		WritableBits = 0x00ffffff,
	}

	public enum LogLevel : UInt32
	{
		Debug = 0, ///< Debug-level log event.
		Info = 1, ///< Info-level log event.
		Error = 2, ///< Error-level log event.
	}

	/// <summary>
	/// Describes layer types that can be passed to ovr_SubmitFrame.
	/// Each layer type has an associated struct, such as ovrLayerEyeFov.
	/// </summary>
	/// <see cref="LayerHeader"/>
	public enum LayerType : UInt32
	{
		/// <summary>
		/// Layer is disabled.
		/// </summary>
		Disabled = 0,

		/// <summary>
		/// Described by LayerEyeFov.
		/// </summary>
		EyeFov = 1,

		/// <summary>
		/// Described by LayerQuad. 
		/// 
		/// Previously called QuadInWorld.
		/// </summary>
		Quad = 3,

		// enum 4 used to be ovrLayerType_QuadHeadLocked. 
		// Instead, use ovrLayerType_Quad with ovrLayerFlag_HeadLocked.

		/// <summary>
		/// Described by LayerEyeMatrix.
		/// </summary>
		EyeMatrix = 5,
	}

	/// <summary>
	/// Identifies flags used by LayerHeader and which are passed to ovr_SubmitFrame.
	/// </summary>
	/// <see cref="LayerHeader"/>
	[Flags]
	public enum LayerFlags : UInt32
	{
		/// <summary>
		/// HighQuality enables 4x anisotropic sampling during the composition of the layer.
		/// The benefits are mostly visible at the periphery for high-frequency &amp; high-contrast visuals.
		/// For best results consider combining this flag with an ovrTextureSwapChain that has mipmaps and
		/// instead of using arbitrary sized textures, prefer texture sizes that are powers-of-two.
		/// Actual rendered viewport and doesn't necessarily have to fill the whole texture.
		/// </summary>
		HighQuality = 0x01,

		/// <summary>
		/// TextureOriginAtBottomLeft: the opposite is TopLeft.
		/// 
		/// Generally this is false for D3D, true for OpenGL.
		/// </summary>
		TextureOriginAtBottomLeft = 0x02,

		/// <summary>
		/// Mark this surface as "headlocked", which means it is specified
		/// relative to the HMD and moves with it, rather than being specified
		/// relative to sensor/torso space and remaining still while the head moves.
		/// 
		/// What used to be ovrLayerType_QuadHeadLocked is now LayerType.Quad plus this flag.
		/// However the flag can be applied to any layer type to achieve a similar effect.
		/// </summary>
		HeadLocked = 0x04
	}

	/// <summary>
	/// Performance HUD enables the HMD user to see information critical to
	/// the real-time operation of the VR application such as latency timing,
	/// and CPU &amp; GPU performance metrics
	/// </summary>
	/// <example>
	/// App can toggle performance HUD modes as such:
	/// 
	/// PerfHudMode perfHudMode = PerfHudMode.Hud_LatencyTiming;
	/// ovr_SetInt(Hmd, "PerfHudMode", (int) perfHudMode);
	/// </example>
	public enum PerfHudMode : UInt32
	{
		Off = 0,

		/// <summary>
		/// Shows performance summary and headroom
		/// </summary>
		PerfSummary = 1,

		/// <summary>
		/// Shows latency related timing info
		/// </summary>
		LatencyTiming = 2,

		/// <summary>
		/// Shows render timing info for application
		/// </summary>
		AppRenderTiming = 3,

		/// <summary>
		/// Shows render timing info for OVR compositor
		/// </summary>
		CompRenderTiming = 4,

		/// <summary>
		/// Shows SDK &amp; HMD version Info
		/// </summary>
		VersionInfo = 5,

		AswStats = 6,

		/// <summary>
		/// Count of enumerated elements.
		/// </summary>
		Count = 7
	}

	/// <summary>
	/// Layer HUD enables the HMD user to see information about a layer
	/// <example>
	/// <code>
	///     App can toggle layer HUD modes as such:
	///         ovrLayerHudMode LayerHudMode = ovrLayerHud_Info;
	///         ovr_SetInt(Hmd, OVR_LAYER_HUD_MODE, (int)LayerHudMode);
	/// </code>
	/// </example>
	/// </summary>
	public enum ovrLayerHudMode : UInt32
	{
		/// <summary>
		/// Turns off the layer HUD
		/// </summary>
		Off = 0,

		/// <summary>
		/// Shows info about a specific layer
		/// </summary>
		Info = 1,
	}

	/// <summary>
	/// Debug HUD is provided to help developers gauge and debug the fidelity of their app's
	/// stereo rendering characteristics. Using the provided quad and crosshair guides, 
	/// the developer can verify various aspects such as VR tracking units (e.g. meters),
	/// stereo camera-parallax properties (e.g. making sure objects at infinity are rendered
	/// with the proper separation), measuring VR geometry sizes and distances and more.
	///
	///     App can toggle the debug HUD modes as such:
	///     \code{.cpp}
	///     \endcode
	///
	/// The app can modify the visual properties of the stereo guide (i.e. quad, crosshair)
	/// using the ovr_SetFloatArray function. For a list of tweakable properties,
	/// see the OVR_DEBUG_HUD_STEREO_GUIDE_* keys in the OVR_CAPI_Keys.h header file.
	/// </summary>
	/// <example>
	/// ovrDebugHudStereoMode DebugHudMode = ovrDebugHudStereo.QuadWithCrosshair;
	/// ovr_SetInt(Hmd, OVR_DEBUG_HUD_STEREO_MODE, (int)DebugHudMode);
	/// </example>
	public enum DebugHudStereoMode : UInt32
	{
		/// <summary>
		/// Turns off the Stereo Debug HUD
		/// </summary>
		Off = 0,

		/// <summary>
		/// Renders Quad in world for Stereo Debugging
		/// </summary>
		Quad = 1,

		/// <summary>
		/// Renders Quad+crosshair in world for Stereo Debugging
		/// </summary>
		QuadWithCrosshair = 2,

		/// <summary>
		/// Renders screen-space crosshair at infinity for Stereo Debugging
		/// </summary>
		CrosshairAtInfinity = 3,

		/// <summary>
		/// Count of enumerated elements
		/// </summary>
		Count
	}

	/// <summary>
	/// Enumerates modifications to the projection matrix based on the application's needs.
	/// </summary>
	/// <see cref="OVRBase.Matrix_Projection"/>
	public enum ProjectionModifier
	{
		/// <summary>
		/// Use for generating a default projection matrix that is:
		/// * Right-handed.
		/// * Near depth values stored in the depth buffer are smaller than far depth values.
		/// * Both near and far are explicitly defined.
		/// * With a clipping range that is (0 to w).
		/// </summary>
		None = 0x00,

		/// <summary>
		/// Enable if using left-handed transformations in your application.
		/// </summary>
		LeftHanded = 0x01,

		/// <summary>
		/// After the projection transform is applied, far values stored in the depth buffer will be less than closer depth values.
		/// NOTE: Enable only if the application is using a floating-point depth buffer for proper precision.
		/// </summary>
		FarLessThanNear = 0x02,

		/// <summary>
		/// When this flag is used, the zfar value pushed into ovrMatrix_Projection() will be ignored
		/// NOTE: Enable only if ovrProjection_FarLessThanNear is also enabled where the far clipping plane will be pushed to infinity.
		/// </summary>
		FarClipAtInfinity = 0x04,

		/// <summary>
		/// Enable if the application is rendering with OpenGL and expects a projection matrix with a clipping range of (-w to w).
		/// Ignore this flag if your application already handles the conversion from D3D range (0 to w) to OpenGL.
		/// </summary>
		ClipRangeOpenGL = 0x08
	}

	public enum HapticsGenMode
	{
		PointSample,
		Count,
	}
	#endregion
}
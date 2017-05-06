#define RENDER_OCULUS

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Reflection;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Oculus;

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

			class EyeTarget
			{
				//mostly static
				public EyeType eye;
				public EyeRenderDesc desc;
				public Matrix4 proj;
				public int fbo;
				public int depthTexture;
				public ovrTextureSwapChain swapChain;
				public Sizei renderTargetSize;

				//updated every frame
				public Posef pose;
			};

			EyeTarget[] eyes = new EyeTarget[2];

			int myShader;
			int myBlitShader;
			int myVao;
			int myBlitVao;
			int myVbo;
			int myBlitVbo;
			int myIbo;
			int myModelUniform;
			int myViewUniform;
			int myProjectionUniform;
			int myTextureUniform;
			int myCubeTexture;
			UInt32 myMirrorTexture;

			float zoomFactor = 1.0f;

			Result result;
			ErrorInfo error;
			ovrSession session = IntPtr.Zero;
			GraphicsLuid luid = new GraphicsLuid();
			HmdDesc hmdDesc;
			ovrMirrorTexture mirror = IntPtr.Zero;
			Layers layers = new Layers();

			public TestOculus() : base(theWidth, theHeigth, new GraphicsMode(32, 24, 0, 0), "Test FBO", GameWindowFlags.Default, DisplayDevice.Default, 4, 5,
#if DEBUG
			GraphicsContextFlags.Debug)
#else
         GraphicsContextFlags.Default)
#endif
			{
				this.VSync = VSyncMode.Off;

				eyes[0] = new EyeTarget();
				eyes[1] = new EyeTarget();

				Keyboard.KeyUp += new EventHandler<KeyboardKeyEventArgs>(handleKeyboardUp);
			}

			#region boilerplate
			public void handleKeyboardUp(object sender, KeyboardKeyEventArgs e)
			{
				if (e.Key == Key.Escape)
				{
					Exit();
				}

				if(e.Key == Key.Space)
				{
					OvrDLL.ovr_RecenterTrackingOrigin(session);
				}

				if(e.Key == Key.Up)
				{
					zoomFactor *= 2.0f;
					if (zoomFactor > 128.0f)
						zoomFactor = 128.0f;
				}

				if (e.Key == Key.Down)
				{
					zoomFactor /= 2.0f;
					if (zoomFactor < 0.25f)
						zoomFactor = 0.25f;
				}
			}

			protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
			{
				base.OnClosing(e);

				OvrDLL.ovr_DestroyTextureSwapChain(session, eyes[0].swapChain);
				OvrDLL.ovr_DestroyTextureSwapChain(session, eyes[1].swapChain);
				OvrDLL.ovr_DestroyMirrorTexture(session, mirror);
				OvrDLL.ovr_Destroy(session);
				OvrDLL.ovr_Shutdown();
			}

			protected override void OnResize(EventArgs e)
			{
				base.OnResize(e);
			}
			#endregion

			protected override void OnUpdateFrame(FrameEventArgs e)
			{
				base.OnUpdateFrame(e);

				//get the time the frame will be displayed on the oculus
				double displayMidpoint = OvrDLL.ovr_GetPredictedDisplayTime(session, 0);
				//get the predicted position of the device at that time
				TrackingState ts = OvrDLL.ovr_GetTrackingState(session, displayMidpoint, true);
				//calculate eye poses
				Vector3[] eyeOffsets = new Vector3[2] { eyes[0].desc.HmdToEyeOffset, eyes[1].desc.HmdToEyeOffset };
				Posef[] eyePoses = new Posef[2];
				OvrDLL.ovr_CalcEyePoses(ts.HeadPose.ThePose, eyeOffsets, eyePoses);

				//get the orientation of the hmd if it was tracked
				if (ts.StatusFlags.HasFlag(StatusBits.OrientationTracked))
				{
					eyes[0].pose.Orientation = eyePoses[0].Orientation;
					eyes[1].pose.Orientation = eyePoses[1].Orientation;
				}
				else
				{
					eyes[0].pose.Orientation = Quaternion.Identity;
					eyes[1].pose.Orientation = Quaternion.Identity;
				}

				//get the position of the hmd if it was tracked
				if (ts.StatusFlags.HasFlag(StatusBits.PositionTracked))
				{
					eyes[0].pose.Position = eyePoses[0].Position;
					eyes[1].pose.Position = eyePoses[1].Position;
				}
				else
				{
					eyes[0].pose.Position = Vector3.Zero;
					eyes[1].pose.Position = Vector3.Zero;
				}
			}

			protected override void OnLoad(EventArgs e)
			{
				base.OnLoad(e);

				initGLObjects();

				GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
				GL.Enable(EnableCap.CullFace);
				GL.Enable(EnableCap.DepthTest);
				GL.Enable(EnableCap.Blend);

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
				eyes[0].desc = OvrDLL.ovr_GetRenderDesc(session, EyeType.Left, hmdDesc.LeftDefaultEyeFov);
				eyes[1].desc = OvrDLL.ovr_GetRenderDesc(session, EyeType.Right, hmdDesc.RightDefaultEyeFov);

				//get the tracker (the first one) description and then recenter it
				TrackerDesc trackDesc = OvrDLL.ovr_GetTrackerDesc(session, 0);
				Console.WriteLine("Tracker 0 description: FOV H: {0} V: {1} Near: {2} Far: {3}", trackDesc.FrustumHFovInRadians, trackDesc.FrustumVFovInRadians, trackDesc.FrustumNearZInMeters, trackDesc.FrustumFarZInMeters);
				OvrDLL.ovr_RecenterTrackingOrigin(session);
				Console.WriteLine("Tracker origin recentered");

				//create layers
				layers.AddLayerEyeFov(); // in slot 0

				//init each of the eye targets with swap chains/fbo/etc
				for (int i = 0; i < 2; i++)
				{
					initEyeTarget((EyeType)i);
				}

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
				
				OvrDLL.ovr_GetMirrorTextureBufferGL(session, mirror, ref myMirrorTexture);
				Console.WriteLine("OpenGL Mirror texture ID: {0}", myMirrorTexture);
			}

			protected override void OnRenderFrame(FrameEventArgs e)
			{
				base.OnRenderFrame(e);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

				Vector3 camPos = new Vector3(0, 2, 5);
				Quaternion camOri = Quaternion.Identity;


#if RENDER_OCULUS
				for (int i = 0; i < 2; i++)
				{
					//zoom field of view
					var fov = new FovPort
					{
						DownTan = eyes[i].desc.Fov.DownTan / zoomFactor,
						UpTan = eyes[i].desc.Fov.UpTan / zoomFactor,
						LeftTan = eyes[i].desc.Fov.LeftTan / zoomFactor,
						RightTan = eyes[i].desc.Fov.RightTan / zoomFactor
					};


					eyes[i].proj = OvrDLL.ovrMatrix4f_Projection(fov, 0.1f, 1000.0f, ProjectionModifier.ClipRangeOpenGL);

					//bind eye fbo
					bindFbo(eyes[i]);

					//combine the "camera" position/rotation with the position/rotation of the eye
					Vector3 finalPos = camPos + (camOri * eyes[i].pose.Position);
					Matrix4 finalRot = Matrix4.CreateFromQuaternion(camOri * eyes[i].pose.Orientation);

					//create the view matrix with a lookat basis vectors
					Vector3 up = eyes[i].pose.Orientation * Vector3.UnitY;
					Vector3 fwd = eyes[i].pose.Orientation * -Vector3.UnitZ;
					Matrix4 view = Matrix4.LookAt(finalPos, finalPos + fwd, up);

					//draw the scene
					drawScene(view, eyes[i].proj);

					//commit the swapchain
					OvrDLL.ovr_CommitTextureSwapChain(session, eyes[i].swapChain);
				}

				//send to Oculus
				LayerEyeFov eyeFov = layers[0] as LayerEyeFov;
				eyeFov.Header.Flags = LayerFlags.TextureOriginAtBottomLeft;
				eyeFov.ColorTexture[0] = eyes[0].swapChain;
				eyeFov.ColorTexture[1] = eyes[1].swapChain;
				eyeFov.Fov[0] = eyes[0].desc.Fov;
				eyeFov.Fov[1] = eyes[1].desc.Fov;
				eyeFov.Viewport[0] = new Recti(new Vector2i(0,0), eyes[0].renderTargetSize);
				eyeFov.Viewport[1] = new Recti(new Vector2i(0, 0), eyes[1].renderTargetSize);
				eyeFov.RenderPose[0] = eyes[0].pose;
				eyeFov.RenderPose[1] = eyes[1].pose;

 				ViewScaleDesc viewScale = new ViewScaleDesc();
				result = OvrDLL.ovr_SubmitFrame(session, 0, ref viewScale, layers.GetUnmanagedLayers(), 1);
				if (result < 0)
				{
					Console.WriteLine("Error submitting frame");
					OvrDLL.ovr_GetLastErrorInfo(ref error);
					Console.WriteLine("Last Error Info: {0}-{1}", error.result, error.ErrorString);
				}

				//blit mirror to fbo
				OvrDLL.ovr_GetMirrorTextureBufferGL(session, mirror, ref myMirrorTexture);
				blitMirror((int)myMirrorTexture);
#else
				Matrix4 view = Matrix4.CreateTranslation(-camPos);
				Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60.0f), 1.0f, 0.1f, 1000.0f);
				drawScene(view, proj);
#endif
				SwapBuffers();
			}

			void initEyeTarget(EyeType eye)
			{
				EyeTarget e = new EyeTarget();
				e.eye = eye;
				e.fbo = GL.GenFramebuffer();
				e.depthTexture = GL.GenRenderbuffer();
				e.desc = OvrDLL.ovr_GetRenderDesc(session, eye, eye == EyeType.Left ? hmdDesc.LeftDefaultEyeFov : hmdDesc.RightDefaultEyeFov);
				e.renderTargetSize = OvrDLL.ovr_GetFovTextureSize(session, EyeType.Left, hmdDesc.LeftDefaultEyeFov, 1.0f);

				e.proj = OvrDLL.ovrMatrix4f_Projection(e.desc.Fov, 0.1f, 1000.0f, ProjectionModifier.ClipRangeOpenGL);

				//create the texture swap chain
				TextureSwapChainDesc swapDesc = new TextureSwapChainDesc()
				{
					Type = TextureType.Texture2D,
					ArraySize = 1,
					Format = TextureFormat.R8G8B8A8_UNORM_SRGB,
					Width = e.renderTargetSize.Width,
					Height = e.renderTargetSize.Height,
					MipLevels = 1,
					SampleCount = 1,
					StaticImage = 0
				};

				result = OvrDLL.ovr_CreateTextureSwapChainGL(session, ref swapDesc, ref e.swapChain);
				if (result < 0)
				{
					Console.WriteLine("Error creating swap chain");
					OvrDLL.ovr_GetLastErrorInfo(ref error);
					Console.WriteLine("Last Error Info: {0}-{1}", error.result, error.ErrorString);
				}

				int swapChainLength = 0;
				OvrDLL.ovr_GetTextureSwapChainLength(session, e.swapChain, ref swapChainLength);
				Console.WriteLine("Swapchain length: {0}", swapChainLength);

				for(int i = 0; i< swapChainLength; i++)
				{
					UInt32 texId = 0;
					OvrDLL.ovr_GetTextureSwapChainBufferGL(session, e.swapChain, i, ref texId);
					GL.BindTexture(TextureTarget.Texture2D, texId);
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

				}

				int currentIndex = 0;
				OvrDLL.ovr_GetTextureSwapChainCurrentIndex(session, e.swapChain, ref currentIndex);
				Console.WriteLine("Swapchain current index: {0}", currentIndex);

				UInt32 chainTexId = 0;
				OvrDLL.ovr_GetTextureSwapChainBufferGL(session, e.swapChain, currentIndex, ref chainTexId);

				GL.BindFramebuffer(FramebufferTarget.Framebuffer, e.fbo);
				GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, e.depthTexture);
				GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent32f, e.renderTargetSize.Width, e.renderTargetSize.Height);
				GL.FramebufferRenderbuffer(FramebufferTarget.DrawFramebuffer, FramebufferAttachment.Depth, RenderbufferTarget.Renderbuffer, e.depthTexture);
				
				GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, chainTexId, 0);
				DrawBuffersEnum[] drawBuffers = new DrawBuffersEnum[1] { DrawBuffersEnum.ColorAttachment0 };
				GL.DrawBuffers(1, drawBuffers);

				//check frame buffer completeness
				FramebufferErrorCode err = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
				if(err != FramebufferErrorCode.FramebufferComplete)
				{
					Console.WriteLine("Error in frame buffer: {0}", err);
				}

				eyes[(int)eye] = e;

				GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			}

#region OpenGL init and rendering
			void initGLObjects()
			{
				//create the objects
				myVao = GL.GenVertexArray();
				myBlitVao = GL.GenVertexArray();
				myVbo = GL.GenBuffer();
				myBlitVbo = GL.GenBuffer();
				myIbo = GL.GenBuffer();
				int vs = GL.CreateShader(ShaderType.VertexShader);
				int fs = GL.CreateShader(ShaderType.FragmentShader);
				int bvs = GL.CreateShader(ShaderType.VertexShader);
				int bfs = GL.CreateShader(ShaderType.FragmentShader);
				myShader = GL.CreateProgram();
				myBlitShader = GL.CreateProgram();

				GL.Enable(EnableCap.Blend);

				//compile the shaders
				GL.ShaderSource(vs, vertexShader);
				GL.CompileShader(vs);
				Console.WriteLine("Vertex Shader log: {0}", GL.GetShaderInfoLog(vs));
				GL.ShaderSource(fs, fragmentShader);
				GL.CompileShader(fs);
				Console.WriteLine("Fragment Shader log: {0}", GL.GetShaderInfoLog(fs));

				//link to the program
				GL.AttachShader(myShader, vs);
				GL.AttachShader(myShader, fs);
				GL.LinkProgram(myShader);
				Console.WriteLine("Program log: {0}", GL.GetProgramInfoLog(myShader));

				//get uniform locations
				myModelUniform = GL.GetUniformLocation(myShader, "model");
				myViewUniform = GL.GetUniformLocation(myShader, "view");
				myProjectionUniform = GL.GetUniformLocation(myShader, "proj");
				myTextureUniform = GL.GetUniformLocation(myShader, "tex");

				//setup the buffers
				GL.BindBuffer(BufferTarget.ArrayBuffer, myVbo);
				GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * V3T2.stride, verts, BufferUsageHint.StaticDraw); //upload the verts
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, myIbo);
				GL.BufferData(BufferTarget.ElementArrayBuffer, index.Length * 2, index, BufferUsageHint.StaticDraw); //upload the indexes
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

				//setup the VAO
				GL.BindVertexArray(myVao);
				GL.BindBuffer(BufferTarget.ArrayBuffer, myVbo);
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, myIbo);
				GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, V3T2.stride, 0);
				GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, V3T2.stride, 12);
				GL.EnableVertexArrayAttrib(myVao, 0);
				GL.EnableVertexArrayAttrib(myVao, 1);

				//cleanup
				GL.BindVertexArray(0);
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

				//wash/rinse/repeat for blit shader
				GL.ShaderSource(bvs, blitVertexShader);
				GL.CompileShader(bvs);
				Console.WriteLine("Vertex Shader log: {0}", GL.GetShaderInfoLog(bvs));
				GL.ShaderSource(bfs, blitFragmentShader);
				GL.CompileShader(bfs);
				Console.WriteLine("Fragment Shader log: {0}", GL.GetShaderInfoLog(bfs));

				GL.AttachShader(myBlitShader, bvs);
				GL.AttachShader(myBlitShader, bfs);
				GL.LinkProgram(myBlitShader);
				Console.WriteLine("Program log: {0}", GL.GetProgramInfoLog(myBlitShader));

				GL.BindVertexArray(myBlitVao);
				GL.BindBuffer(BufferTarget.ArrayBuffer, myBlitVbo);

				//load the test texture
				myCubeTexture = loadTexture("TestOculus.testOculus.uvTest.png");

				//cleanup
				GL.BindVertexArray(0);
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
			}

			void bindFbo(EyeTarget eye)
			{
				UInt32 chainTexId = 0;
				OvrDLL.ovr_GetTextureSwapChainBufferGL(session, eye.swapChain, -1, ref chainTexId); //-1 for the chain index is the next buffer

				GL.BindFramebuffer(FramebufferTarget.Framebuffer, eye.fbo);
				GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, chainTexId, 0);

				GL.Enable(EnableCap.FramebufferSrgb);
				GL.Viewport(0, 0, eye.renderTargetSize.Width, eye.renderTargetSize.Height);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			}

			void drawScene(Matrix4 view, Matrix4 proj)
			{
				drawCube(view, proj, new Vector3(0, 0, 0));
			}

			void drawCube(Matrix4 view, Matrix4 proj, Vector3 pos, float scale = 1.0f)
			{
				Matrix4 modelMat = Matrix4.CreateTranslation(pos);
				Matrix4 scaleMat = Matrix4.CreateScale(scale);
				modelMat = modelMat * scaleMat;

				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, myCubeTexture);
				GL.UseProgram(myShader);
				GL.Uniform1(myTextureUniform, 0);
				GL.BindVertexArray(myVao);
				GL.UniformMatrix4(myModelUniform, false, ref modelMat);
				GL.UniformMatrix4(myViewUniform, false, ref view);
				GL.UniformMatrix4(myProjectionUniform, false, ref proj);
				GL.DrawElements(BeginMode.Triangles, index.Length, DrawElementsType.UnsignedShort, 0);
			}

			void blitMirror(int mirrorTexture)
			{
				GL.Disable(EnableCap.FramebufferSrgb);
				GL.Viewport(0, 0, theWidth, theHeigth);
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D , mirrorTexture);
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

				GL.UseProgram(myBlitShader);
				GL.Uniform1(myTextureUniform, 0);
				GL.BindVertexArray(myBlitVao);
				GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
			}
#endregion

#region shaders
			string vertexShader = @"
# version 430

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 uv;

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;

smooth out vec2 texCoord;

void main()
{

   texCoord = uv;
	vec4 v = proj * view * model * vec4(position,1);
	//v.y = -v.y;  //rift is rendering upside down.
   gl_Position = v; 
}
";

			string fragmentShader = @"
# version 430

layout(location = 20) uniform sampler2D tex;

smooth in vec2 texCoord;

out vec4 FragColor;

void main()
{
	vec4 outColor = texture(tex, texCoord);
   FragColor = outColor;
	//FragColor = vec4(1,0,0,1);
}
";

			string blitVertexShader = @"
# version 430

smooth out vec2 texcoord;

const vec2 quadVertices[4] = { vec2(-1.0, -1.0), vec2(1.0, -1.0), vec2(-1.0, 1.0), vec2(1.0, 1.0) };
const vec2 texVerts[4] = {vec2(0,1), vec2(1,1), vec2(0,0), vec2(1,0)}; //since the mirror texture is upside down from what we expect
void main()
{
	texcoord = texVerts[gl_VertexID];
   gl_Position = vec4(quadVertices[gl_VertexID], 0.0, 1.0);
}
";

			string blitFragmentShader = @"
# version 430

layout(location = 20) uniform sampler2D tex;

smooth in vec2 texcoord;

out vec4 FragColor;

void main() 
{
   FragColor = texture2D(tex,texcoord);
}
";
#endregion

#region data
			ushort[] index ={0, 1, 2,
								0, 2, 3, //front
                        4, 5, 6,
								4,6, 7, //left
                        8,9,10,
								8,10,11, //right
                        12,13,14,
								12,14,15, // top 
                        16,17,18,
								16,18,19, // bottom
                        20,21,22,
								20,22,23  // back
                        };

			V3T2[] verts =
			{
				//front face
				new V3T2() {Position = new Vector3(1.0f, -1.0f, -1.0f), TexCoord = new Vector2(0, 0)},
				new V3T2() {Position = new Vector3(-1.0f, -1.0f, -1.0f), TexCoord = new Vector2(1, 0)},
				new V3T2() {Position = new Vector3(-1.0f, 1.0f, -1.0f), TexCoord = new Vector2(1, 1)},
				new V3T2() {Position = new Vector3(1.0f, 1.0f, -1.0f), TexCoord = new Vector2(0, 1)},

				//left
				new V3T2() {Position = new Vector3(-1.0f, -1.0f, -1.0f), TexCoord = new Vector2(0, 0)},
				new V3T2() {Position = new Vector3(-1.0f, -1.0f, 1.0f), TexCoord = new Vector2(1, 0)},
				new V3T2() {Position = new Vector3(-1.0f, 1.0f, 1.0f), TexCoord = new Vector2(1, 1)},
				new V3T2() {Position = new Vector3(-1.0f, 1.0f, -1.0f), TexCoord = new Vector2(0, 1)}, 

				//right
				new V3T2() {Position = new Vector3(1.0f, -1.0f, 1.0f), TexCoord = new Vector2(0, 0)},
				new V3T2() {Position = new Vector3(1.0f, -1.0f, -1.0f), TexCoord = new Vector2(1, 0)},
				new V3T2() {Position = new Vector3(1.0f, 1.0f, -1.0f), TexCoord = new Vector2(1, 1)},
				new V3T2() {Position = new Vector3(1.0f, 1.0f, 1.0f), TexCoord = new Vector2(0, 1)},

				//top
				new V3T2() {Position = new Vector3(-1.0f, 1.0f, 1.0f), TexCoord = new Vector2(0, 0)},
				new V3T2() {Position = new Vector3(1.0f, 1.0f, 1.0f), TexCoord = new Vector2(1, 0)},
				new V3T2() {Position = new Vector3(1.0f, 1.0f, -1.0f), TexCoord = new Vector2(1, 1)},
				new V3T2() {Position = new Vector3(-1.0f, 1.0f, -1.0f), TexCoord = new Vector2(0, 1)},
																																 
				//bottom
				new V3T2() { Position = new Vector3(-1.0f, -1.0f, -1.0f), TexCoord = new Vector2(0, 0)},
				new V3T2() { Position = new Vector3(1.0f, -1.0f, -1.0f), TexCoord = new Vector2(1, 0)},
				new V3T2() { Position = new Vector3(1.0f, -1.0f, 1.0f), TexCoord = new Vector2(1, 1)},
				new V3T2() { Position = new Vector3(-1.0f, -1.0f, 1.0f), TexCoord = new Vector2(0, 1)},

				//back
				new V3T2() { Position = new Vector3(-1.0f, -1.0f, 1.0f), TexCoord = new Vector2(0, 0)},
				new V3T2() { Position = new Vector3(1.0f, -1.0f, 1.0f), TexCoord = new Vector2(1, 0)},
				new V3T2() { Position = new Vector3(1.0f, 1.0f, 1.0f), TexCoord = new Vector2(1, 1)},
				new V3T2() { Position = new Vector3(-1.0f, 1.0f, 1.0f), TexCoord = new Vector2(0, 1)}
			};

			int loadTexture(string name)
			{
				Stream imgStream = getEmbeddedTexture(name);
				Bitmap bm = new Bitmap(imgStream);

				//flip it since we want the origin to be the bottom left and the image has the origin in the top left
				bm.RotateFlip(RotateFlipType.RotateNoneFlipY);

				BitmapData Data = bm.LockBits(new System.Drawing.Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, bm.PixelFormat);

				int id = GL.GenTexture();
				GL.BindTexture(TextureTarget.Texture2D, id);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Data.Width, Data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, Data.Scan0);
				bm.UnlockBits(Data);

				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

				GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

				GL.BindTexture(TextureTarget.Texture2D, 0);
				return id;
			}

			public static Stream getEmbeddedTexture(string resourceName)
			{
				Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();
				foreach (Assembly ass in asses)
				{
					string[] resources = ass.GetManifestResourceNames();
					foreach (string s in resources)
					{
						if (s == resourceName)
						{
							Stream stream = ass.GetManifestResourceStream(resourceName);
							if (stream == null)
							{
								System.Console.WriteLine("Cannot find embedded resource {0}", resourceName);
								return null;
							}

							return stream;
						}
					}
				}

				Console.WriteLine("Failed to find embedded texture {0}", resourceName);
				return null;
			}

#endregion
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct V3T2
		{
			static int theStride = Marshal.SizeOf(default(V3T2));

			public Vector3 Position; //8 bytes
			public Vector2 TexCoord; //8 bytes

			public static int stride { get { return theStride; } }
		}

		static void Main(string[] args)
		{
			using (TestOculus example = new TestOculus())
			{
				example.Title = "Test Oculus";
				example.Run(90.0, 90.0);
			}
		}
	}
}

//-----------------------------------------------------------------------------
// The GraphicsManager handles all lower-level aspects of rendering.
//
// __Defense Sample for Game Programming Algorithms and Techniques
// Copyright (C) Sanjay Madhav. All rights reserved.
//
// Released under the Microsoft Permissive License.
// See LICENSE.txt for full details.
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace defense
{
	public enum eDrawOrder
	{
		Default,
		Background,
		Foreground
	}

	public class GraphicsManager : defense.Patterns.Singleton<GraphicsManager>
	{
		GraphicsDeviceManager m_Graphics;
		Game m_Game;
		SpriteBatch m_SpriteBatch;
		Texture2D m_Blank;
		Texture2D[] m_MouseTextures = new Texture2D[(int)eMouseState.MAX_STATES];

		// Render targets for post processing
		RenderTarget2D m_SceneTarget;
		// Working targets for post process effects
		// These are half the size of the full scene target
		RenderTarget2D m_HalfTarget1;
		RenderTarget2D m_HalfTarget2;

		SpriteFont m_FPSFont;

		LinkedList<GameObject> m_DefaultObjects = new LinkedList<GameObject>();
		LinkedList<GameObject> m_BGObjects = new LinkedList<GameObject>();
		LinkedList<GameObject> m_FGObjects = new LinkedList<GameObject>();

#if XNA
		Graphics.BloomEffect m_BloomEffect;
#endif

		public Matrix Projection;
		
		public bool IsFullScreen
		{
			get { return m_Graphics.IsFullScreen; }
			set { m_Graphics.IsFullScreen = value; }
		}

		public bool IsVSync
		{
			get { return m_Graphics.SynchronizeWithVerticalRetrace; }
			set { m_Graphics.SynchronizeWithVerticalRetrace = value; }
		}

		public int Width
		{
			get { return m_Graphics.PreferredBackBufferWidth; }
		}

		public int Height
		{
			get { return m_Graphics.PreferredBackBufferHeight; }
		}

		public GraphicsDevice GraphicsDevice
		{
			get { return m_Graphics.GraphicsDevice; }
		}

		float m_fZoom = GlobalDefines.fCameraZoom;
		public float Zoom
		{
			get { return m_fZoom; }
			set
			{
				if (value > GlobalDefines.fCameraMaxZoom ||
					value < GlobalDefines.fCameraMinZoom)
				{
					return;
				}

				m_fZoom = value;
				SetProjection((float)Width / Height);
			}
		}

		public Utils.MarkupTextEngine MarkupEngine
		{
			get; internal set;
		}
		
		public void Start(Game game)
		{
			m_Graphics = new GraphicsDeviceManager(game);
			m_Game = game;
			IsVSync = GlobalDefines.bVSync;
			
			// TODO: Set resolution to what's saved in the INI, or default fullscreen
			if (!GlobalDefines.bFullScreen)
			{
				SetResolution(1440, 900);
			}
			else
			{
				SetResolutionToCurrent();
				ToggleFullScreen();
			}
		}

		public void LoadContent()
		{
			InitializeRenderTargets();

			m_SpriteBatch = new SpriteBatch(m_Graphics.GraphicsDevice);

			// Load mouse textures
			m_MouseTextures[(int)eMouseState.Default] = m_Game.Content.Load<Texture2D>("UI/Mouse_Default");

			// Load FPS font
			m_FPSFont = m_Game.Content.Load<SpriteFont>("Fonts/FixedText");

			// Debug stuff for line drawing
			m_Blank = new Texture2D(m_Graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
			m_Blank.SetData(new[] { Color.White });

#if XNA
			// Setup bloom
			m_BloomEffect = new Graphics.BloomEffect(m_Game);
			m_BloomEffect.LoadContent();
#endif

			//setup the font, image, video and condition resolvers for the engine
			//the resolvers are simple lambdas that map a string to its corresponding data
			//e.g. an image resolver maps a string to a Texture2D
			var fonts = new Dictionary<string, SpriteFont>();
			Func<string, SpriteFont> fontResolver = f => fonts[f];
			fonts.Add("Tooltip", m_Game.Content.Load<SpriteFont>("Fonts/Tooltip"));
			fonts.Add("Instructions", m_Game.Content.Load<SpriteFont>("Fonts/FixedText"));

			var buttons = new Dictionary<string, Texture2D>();
			Func<string, Texture2D> imageResolver = b => buttons[b];

			var conditions = new Dictionary<string, bool>();
			Func<string, bool> conditionalResolver = c => conditions[c];

			MarkupEngine = new Utils.MarkupTextEngine(fontResolver, imageResolver, conditionalResolver);
		}

		public void SetResolutionToCurrent()
		{
			m_Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
			m_Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

			m_fZoom = GlobalDefines.fCameraZoom;
			SetProjection((float)Width / Height);

			if (m_Graphics.GraphicsDevice != null)
			{
				m_Graphics.ApplyChanges();
				InitializeRenderTargets();
			}
		}

		public void SetResolution(int Width, int Height)
		{
			m_Graphics.PreferredBackBufferWidth = Width;
			m_Graphics.PreferredBackBufferHeight = Height;

			m_fZoom = GlobalDefines.fCameraZoom;
			SetProjection((float)Width / Height);

			if (m_Graphics.GraphicsDevice != null)
			{
				m_Graphics.ApplyChanges();
				InitializeRenderTargets();
			}
		}

		public void SetProjection(float fAspectRatio)
		{
			Projection = Matrix.CreateOrthographic(m_fZoom, m_fZoom / fAspectRatio, 0.1f, 100.0f);
		}

		public void ResetProjection()
		{
			m_fZoom = GlobalDefines.fCameraZoom;
			SetProjection((float)Width / Height);
		}

		public void InitializeRenderTargets()
		{
			// Look up the resolution and format of our main backbuffer.
			PresentationParameters pp = m_Graphics.GraphicsDevice.PresentationParameters;

			int width = m_Graphics.GraphicsDevice.Viewport.Width;
			int height = m_Graphics.GraphicsDevice.Viewport.Height;

			SurfaceFormat format = pp.BackBufferFormat;

			// Create a texture for rendering the main 3D Scene
			m_SceneTarget = new RenderTarget2D(m_Graphics.GraphicsDevice, width, height, false,
												   format, pp.DepthStencilFormat, pp.MultiSampleCount,
												   RenderTargetUsage.DiscardContents);
						
			width /= 2;
			height /= 2;

			m_HalfTarget1 = new RenderTarget2D(m_Graphics.GraphicsDevice, width, height, 
				false, format, DepthFormat.None);
			m_HalfTarget2 = new RenderTarget2D(m_Graphics.GraphicsDevice, width, height, 
				false, format, DepthFormat.None);
		}

		public void ToggleFullScreen()
		{
			m_Graphics.ToggleFullScreen();
		}

		public void Draw(float fDeltaTime)
		{
#if XNA
			// Enable rendering to our scene target
			if (GlobalDefines.bBloomEnabled)
			{
				m_Graphics.GraphicsDevice.SetRenderTarget(m_SceneTarget);
			}
#endif

			// Clear back buffer
			m_Graphics.GraphicsDevice.Clear(Color.Black);

			// First draw all 3D components
			m_Game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
			// For background objects, disabled Z-Buffer
			m_Game.GraphicsDevice.DepthStencilState = DepthStencilState.None;
			foreach (GameObject o in m_BGObjects)
			{
				o.Draw(fDeltaTime);
			}

			m_Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			foreach (GameObject o in m_DefaultObjects)
			{
				o.Draw(fDeltaTime);
			}

			// Also disabled Z-Buffer for background objects
			m_Game.GraphicsDevice.DepthStencilState = DepthStencilState.None;
			foreach (GameObject o in m_FGObjects)
			{
				o.Draw(fDeltaTime);
			}

#if XNA
			// Perform post process
			if (GlobalDefines.bBloomEnabled)
			{
				m_BloomEffect.ApplyEffect(m_SceneTarget, m_HalfTarget1, m_HalfTarget2);
			}
			
			// Set render target to back buffer
			m_Graphics.GraphicsDevice.SetRenderTarget(null);
#endif

			// Now draw all 2D components
			m_SpriteBatch.Begin();

			// Draw the UI screens
			GameState.Get().DrawUI(fDeltaTime, m_SpriteBatch);

			// Draw mouse cursor
			Point MousePos = InputManager.Get().MousePosition;
			Vector2 vMousePos = new Vector2(MousePos.X, MousePos.Y);
			m_SpriteBatch.Draw(GetMouseTexture(InputManager.Get().MouseState), vMousePos, Color.White);

			// Draw FPS counter
			Vector2 vFPSPos = Vector2.Zero;
			if (DebugDefines.bShowBuildString)
			{
				m_SpriteBatch.DrawString(m_FPSFont, "__Defense (Prototype)", vFPSPos, Color.White);
				vFPSPos.Y += 25.0f;
			}
			if (DebugDefines.bShowFPS)
			{
				string sFPS = String.Format("FPS: {0}", (int)(1 / fDeltaTime));
				m_SpriteBatch.DrawString(m_FPSFont, sFPS, vFPSPos, Color.White);
			}

			m_SpriteBatch.End();
		}

		Texture2D GetMouseTexture(eMouseState e)
		{
			return m_MouseTextures[(int)e];
		}

		public void AddGameObject(GameObject o)
		{
			if (o.DrawOrder == eDrawOrder.Background)
			{
				m_BGObjects.AddLast(o);
			}
			else if (o.DrawOrder == eDrawOrder.Default)
			{
				m_DefaultObjects.AddLast(o);
			}
			else
			{
				m_FGObjects.AddLast(o);
			}
		}

		public void RemoveGameObject(GameObject o)
		{
			if (o.DrawOrder == eDrawOrder.Background)
			{
				m_BGObjects.Remove(o);
			}
			else if (o.DrawOrder == eDrawOrder.Default)
			{
				m_DefaultObjects.Remove(o);
			}
			else
			{
				m_FGObjects.Remove(o);
			}
		}

		public void ClearAllObjects()
		{
			m_BGObjects.Clear();
			m_DefaultObjects.Clear();
			m_FGObjects.Clear();
		}

		/// <summary>
		/// Helper for drawing a texture into a rendertarget, using
		/// a custom shader to apply postprocessing effects.
		/// </summary>
		public void DrawFullscreenQuad(Texture2D texture, RenderTarget2D renderTarget,
								Effect effect)
		{
			m_Graphics.GraphicsDevice.SetRenderTarget(renderTarget);

			DrawFullscreenQuad(texture,
							   renderTarget.Width, renderTarget.Height,
							   effect);
		}


		/// <summary>
		/// Helper for drawing a texture into the current rendertarget,
		/// using a custom shader to apply postprocessing effects.
		/// </summary>
		public void DrawFullscreenQuad(Texture2D texture, int width, int height, Effect effect)
		{
			m_SpriteBatch.Begin(0, BlendState.Opaque, null, null, null, effect);
			m_SpriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
			m_SpriteBatch.End();
		}

		// Draws a line
		public void DrawLine(SpriteBatch batch, float width, Color color, 
			Vector2 point1, Vector2 point2)
		{
			float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
			float length = Vector2.Distance(point1, point2);

			batch.Draw(m_Blank, point1, null, color,
					   angle, Vector2.Zero, new Vector2(length, width),
					   SpriteEffects.None, 0);
		}

		public void DrawLine3D(SpriteBatch batch, float width, Color color, Vector3 point1, Vector3 point2)
		{
			// Convert the 3D points into screen space points
			Vector3 point1_screen = GraphicsDevice.Viewport.Project(point1, Projection, 
				GameState.Get().CameraMatrix, Matrix.Identity);
			Vector3 point2_screen = GraphicsDevice.Viewport.Project(point2, Projection,
				GameState.Get().CameraMatrix, Matrix.Identity);

			// Now draw a 2D line with the appropriate points
			DrawLine(batch, width, color, new Vector2(point1_screen.X, point1_screen.Y),
				new Vector2(point2_screen.X, point2_screen.Y));
		}

		public void DrawFilled(SpriteBatch batch, Rectangle rect, Color color, float outWidth, Color outColor)
		{
			// Draw the background
			batch.Draw(m_Blank, rect, color);

			// Draw the outline
			DrawLine(batch, outWidth, outColor, new Vector2(rect.Left, rect.Top),
				new Vector2(rect.Right, rect.Top));
			DrawLine(batch, outWidth, outColor, new Vector2(rect.Left, rect.Top),
				new Vector2(rect.Left, rect.Bottom + (int)outWidth));
			DrawLine(batch, outWidth, outColor, new Vector2(rect.Left, rect.Bottom),
				new Vector2(rect.Right, rect.Bottom));
			DrawLine(batch, outWidth, outColor, new Vector2(rect.Right, rect.Top),
				new Vector2(rect.Right, rect.Bottom));
		}
	}
}

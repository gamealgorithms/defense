//-----------------------------------------------------------------------------
// InputManager checks for key binds and adds them to the active binds list
// as appropriate.
// The implementation is similar to the one discussed later in Chapter 5.
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
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace defense
{
	public enum eBindType
	{
		JustPressed, // Was just pressed
		JustReleased, // Was just released
		Held // Was just pressed OR being held
	}

	public enum eBindings
	{
		UI_Exit = 0,
		UI_ProjectileTower,
		UI_SlowTower,
		UI_Upgrade,
		UI_Delete,
		UI_Stop,
		UI_Slow,
		UI_Fast,
		Pan_Left,
		Pan_Forward,
		Pan_Right,
		Pan_Back,
		NUM_BINDINGS
	}

	public class BindInfo
	{
		public BindInfo(Keys Key, eBindType Type)
		{
			m_Key = Key;
			m_Type = Type;
		}

		public Keys m_Key;
		public eBindType m_Type;
	}

	public enum eMouseState
	{
		Default = 0,
		ScrollUp,
		ScrollRight,
		ScrollDown,
		ScrollLeft,
		MAX_STATES
	}

	public class InputManager : defense.Patterns.Singleton<InputManager>
	{
		// Keyboard binding map
		private SortedList<eBindings, BindInfo> m_Bindings;
		private void InitializeBindings()
		{
			m_Bindings = new SortedList<eBindings, BindInfo>();
			// UI Bindings
			m_Bindings.Add(eBindings.UI_Exit, new BindInfo(Keys.Escape, eBindType.JustPressed));
			m_Bindings.Add(eBindings.UI_ProjectileTower, new BindInfo(Keys.E, eBindType.JustPressed));
			m_Bindings.Add(eBindings.UI_SlowTower, new BindInfo(Keys.S, eBindType.JustPressed));
			m_Bindings.Add(eBindings.UI_Upgrade, new BindInfo(Keys.R, eBindType.JustPressed));
			m_Bindings.Add(eBindings.UI_Delete, new BindInfo(Keys.X, eBindType.JustPressed));
			m_Bindings.Add(eBindings.UI_Stop, new BindInfo(Keys.Space, eBindType.JustPressed));
			m_Bindings.Add(eBindings.UI_Slow, new BindInfo(Keys.OemMinus, eBindType.JustPressed));
			m_Bindings.Add(eBindings.UI_Fast, new BindInfo(Keys.OemPlus, eBindType.JustPressed));
			
			// Camera Bindings
			m_Bindings.Add(eBindings.Pan_Left, new BindInfo(Keys.Left, eBindType.Held));
			m_Bindings.Add(eBindings.Pan_Forward, new BindInfo(Keys.Up, eBindType.Held));
			m_Bindings.Add(eBindings.Pan_Right, new BindInfo(Keys.Right, eBindType.Held));
			m_Bindings.Add(eBindings.Pan_Back, new BindInfo(Keys.Down, eBindType.Held));
		}

		private SortedList<eBindings, BindInfo> m_ActiveBinds = new SortedList<eBindings, BindInfo>();

		// Mouse Data
		private MouseState m_PrevMouse;
		private MouseState m_CurrMouse;

		eMouseState m_MouseState = eMouseState.Default;
		public eMouseState MouseState
		{
			get { return m_MouseState; }
			set { m_MouseState = value; }
		}

		// The mouse position according to Windows
		private Point m_DeviceMousePos = Point.Zero;
		// The mouse position taking into account deltas, no clamping
		private Point m_ActualMousePos = Point.Zero;
		// Mouse position with clamping
		private Point m_MousePos = Point.Zero;
		
		public Point MousePosition
		{
			get { return m_MousePos; }
		}

		// Keyboard Data
		private KeyboardState m_PrevKey;
		private KeyboardState m_CurrKey;

		public void Start()
		{
			InitializeBindings();

			m_PrevMouse = Mouse.GetState();
			m_CurrMouse = Mouse.GetState();

			m_DeviceMousePos.X = m_CurrMouse.X;
			m_DeviceMousePos.Y = m_CurrMouse.Y;

			m_ActualMousePos = m_DeviceMousePos;
			m_MousePos = m_ActualMousePos;
			ClampMouse();

			m_PrevKey = Keyboard.GetState();
			m_CurrKey = Keyboard.GetState();
		}

		private void ClampMouse()
		{
			if (m_MousePos.X < 0)
			{
				m_MousePos.X = 0;
			}
			if (m_MousePos.Y < 0)
			{
				m_MousePos.Y = 0;
			}
			if (m_MousePos.X > GraphicsManager.Get().Width)
			{
				m_MousePos.X = GraphicsManager.Get().Width - GlobalDefines.iMouseCursorSize / 4;
			}
			if (m_MousePos.Y > GraphicsManager.Get().Height)
			{
				m_MousePos.Y = GraphicsManager.Get().Height - GlobalDefines.iMouseCursorSize / 4;
			}
		}

		public void UpdateMouse(float fDeltaTime)
		{
			m_PrevMouse = m_CurrMouse;
			m_CurrMouse = Mouse.GetState();

			m_DeviceMousePos.X = m_CurrMouse.X;
			m_DeviceMousePos.Y = m_CurrMouse.Y;

			m_ActualMousePos = m_DeviceMousePos;
			m_MousePos = m_ActualMousePos;
						
			ClampMouse();

			// Check for click
			if (JustPressed(m_PrevMouse.LeftButton, m_CurrMouse.LeftButton))
			{
				// If the UI doesn't handle it, send it to GameState
				if (GameState.Get().UICount == 0 ||
					!GameState.Get().GetCurrentUI().MouseClick(m_MousePos))
				{
					GameState.Get().MouseClick(m_MousePos);
				}
			}

			// Do camera pans and zoom
			if (GameState.Get().State == eGameState.Gameplay &&
				!GameState.Get().IsPaused)
			{
				if (m_MousePos.X < GlobalDefines.fCameraScroll)
				{
					GameState.Get().Camera.AddToPan(Vector3.Left);
				}
				else if (m_MousePos.X > (GraphicsManager.Get().Width -
					GlobalDefines.fCameraScroll))
				{
					GameState.Get().Camera.AddToPan(Vector3.Right);
				}

				if (m_MousePos.Y < GlobalDefines.fCameraScroll)
				{
					GameState.Get().Camera.AddToPan(Vector3.Forward);
				}
				else if (m_MousePos.Y > (GraphicsManager.Get().Height -
					GlobalDefines.fCameraScroll))
				{
					GameState.Get().Camera.AddToPan(Vector3.Backward);
				}

				int iWheelChange = m_CurrMouse.ScrollWheelValue - m_PrevMouse.ScrollWheelValue;
				if (iWheelChange != 0)
				{
					// No delta time modifier because iWheelChange will vary based on FPS
					float fZoomChange = -1.0f * iWheelChange * GlobalDefines.fCameraZoomSpeed;
					GraphicsManager.Get().Zoom += fZoomChange;
				}
			}
		}

		public void UpdateKeyboard(float fDeltaTime)
		{
			m_PrevKey = m_CurrKey;
			m_CurrKey = Keyboard.GetState();
			m_ActiveBinds.Clear();

			// Build the list of bindings which were triggered this frame
			foreach (KeyValuePair<eBindings, BindInfo> k in m_Bindings)
			{
				Keys Key = k.Value.m_Key;
				eBindType Type = k.Value.m_Type;
				switch (Type)
				{
					case (eBindType.Held):
						if ((m_PrevKey.IsKeyDown(Key) &&
							m_CurrKey.IsKeyDown(Key)) ||
							(!m_PrevKey.IsKeyDown(Key) &&
							m_CurrKey.IsKeyDown(Key)))
						{
							m_ActiveBinds.Add(k.Key, k.Value);
						}
						break;
					case (eBindType.JustPressed):
						if (!m_PrevKey.IsKeyDown(Key) &&
							m_CurrKey.IsKeyDown(Key))
						{
							m_ActiveBinds.Add(k.Key, k.Value);
						}
						break;
					case (eBindType.JustReleased):
						if (m_PrevKey.IsKeyDown(Key) &&
							!m_CurrKey.IsKeyDown(Key))
						{
							m_ActiveBinds.Add(k.Key, k.Value);
						}
						break;
				}
			}

			if (m_ActiveBinds.Count > 0)
			{
				// Send the list to the UI first, then any remnants to the game
				if (GameState.Get().UICount != 0)
				{
					GameState.Get().GetCurrentUI().KeyboardInput(m_ActiveBinds);
				}

				GameState.Get().KeyboardInput(m_ActiveBinds);
			}
		}

		public void Update(float fDeltaTime)
		{
			UpdateMouse(fDeltaTime);
			UpdateKeyboard(fDeltaTime);
		}

		protected bool JustPressed(ButtonState Previous, ButtonState Current)
		{
			if (Previous == ButtonState.Released &&
				Current == ButtonState.Pressed)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public Ray CalculateMouseRay()
		{
			// create 2 positions in screenspace using the cursor position. 0 is as
			// close as possible to the camera, 1 is as far away as possible.
			Vector3 nearSource = new Vector3(m_MousePos.X, m_MousePos.Y, 0f);
			Vector3 farSource = new Vector3(m_MousePos.X, m_MousePos.Y, 1f);

			// use Viewport.Unproject to tell what those two screen space positions
			// would be in world space. we'll need the projection matrix and view
			// matrix, which we have saved as member variables. We also need a world
			// matrix, which can just be identity.

			GraphicsDevice GraphicsDevice = GraphicsManager.Get().GraphicsDevice;
			Matrix projectionMatrix = GraphicsManager.Get().Projection;
			Matrix viewMatrix = GameState.Get().CameraMatrix;
			Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(nearSource,
				projectionMatrix, viewMatrix, Matrix.Identity);

			Vector3 farPoint = GraphicsDevice.Viewport.Unproject(farSource,
				projectionMatrix, viewMatrix, Matrix.Identity);

			// find the direction vector that goes from the nearPoint to the farPoint
			// and normalize it....
			Vector3 direction = farPoint - nearPoint;
			direction.Normalize();

			// and then create a new ray using nearPoint as the source.
			return new Ray(nearPoint, direction);
		}
		
		// Convert key binding to string representing the name
		// TODO: THIS IS NOT LOCALIZED
		public string GetBinding(eBindings binding)
		{
			Keys k = m_Bindings[binding].m_Key;
			string name = Enum.GetName(typeof(Keys), k);
			if (k == Keys.OemPlus)
			{
				name = "+";
			}
			else if (k == Keys.OemMinus)
			{
				name = "-";
			}

			return name;
		}
	}
}

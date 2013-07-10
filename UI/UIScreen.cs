//-----------------------------------------------------------------------------
// Base UIScreen class that all other UIScreens inherit from.
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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace defense.UI
{
	public class UIScreen
	{
		protected LinkedList<Button> m_Buttons = new LinkedList<Button>();
		protected ContentManager m_Content;
		protected float m_fLiveTime = 0.0f;
		// Determines whether or not you can press ESC to leave the screen
		protected bool m_bCanExit = false;
		protected Tooltip m_Tooltip = null;
		protected Utils.Timer m_Timer = new Utils.Timer();

		public UIScreen(ContentManager Content)
		{
			m_Content = Content;
		}

		public virtual void Update(float fDeltaTime)
		{
			m_fLiveTime += fDeltaTime;
			m_Tooltip = null;
			foreach (Button b in m_Buttons)
			{
				// If the button is enabled, the mouse is pointing to it, and the UI is the top one
				if (b.Enabled && b.m_Bounds.Contains(InputManager.Get().MousePosition) &&
					GameState.Get().GetCurrentUI() == this)
				{
					b.HasFocus = true;
					m_Tooltip = b.m_Tooltip;
				}
				else
				{
					b.HasFocus = false;
				}
			}

			m_Timer.Update(fDeltaTime);
		}

		public virtual void Draw(float fDeltaTime, SpriteBatch DrawBatch)
		{
			DrawButtons(fDeltaTime, DrawBatch);
			if (m_Tooltip != null)
			{
				m_Tooltip.Draw(fDeltaTime, DrawBatch);
			}
		}

		public virtual bool MouseClick(Point Position)
		{
			bool bReturn = false;
			foreach (Button b in m_Buttons)
			{
				if (b.Enabled && b.m_Bounds.Contains(Position))
				{
					b.Click();
					bReturn = true;
					break;
				}
			}

			return bReturn;
		}

		protected void DrawButtons(float fDeltaTime, SpriteBatch DrawBatch)
		{
			foreach (Button b in m_Buttons)
			{
				if (b.Enabled)
				{
					b.Draw(fDeltaTime, DrawBatch);
				}
			}
		}

		public void DrawCenteredString(SpriteBatch DrawBatch, string sText, 
			SpriteFont font, Color color, Vector2 vOffset)
		{
			Vector2 pos = new Vector2(GraphicsManager.Get().Width / 2.0f, GraphicsManager.Get().Height / 2.0f);
			pos -= font.MeasureString(sText) / 2.0f;
			pos += vOffset;
			DrawBatch.DrawString(font, sText, pos, color);
		}

		public virtual void KeyboardInput(SortedList<eBindings, BindInfo> binds)
		{
			if (binds.ContainsKey(eBindings.UI_Exit))
			{
				if (m_bCanExit)
				{
					GameState.Get().PopUI();
				}

				binds.Remove(eBindings.UI_Exit);
			}
		}

		public virtual void OnExit()
		{

		}
	}
}

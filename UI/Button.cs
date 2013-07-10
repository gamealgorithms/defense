//-----------------------------------------------------------------------------
// Buttons are used by UIScreens -- they have a list of Buttons and decide
// whether a click hits a particular button.
// There's logic for both text and image-based buttons in here.
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

namespace defense.UI
{
	public enum eButtonAlign
	{
		Left = 0,
		Center,
		Right
	}

	public class Button
	{
		// Constructor for text-only buttons
		public Button(Point Position, string TextKey, SpriteFont Font, Color Default,
			Color MouseOver, Action Callback,
			eButtonAlign Align = eButtonAlign.Center, string TooltipKey = "", LinkedList<TipData> list = null)
		{
			string Text = Localization.Get().Text(TextKey);
			// Grab the extents of this text so we can align it properly
			Vector2 vTextSize = Font.MeasureString(Text);

			switch (Align)
			{
				case (eButtonAlign.Left):
					m_Bounds.Location = Position;
					break;
				case (eButtonAlign.Center):
					m_Bounds.Location = Position;
					m_Bounds.X -= (int)(vTextSize.X / 2.0f);
					m_Bounds.Y -= (int)(vTextSize.Y / 2.0f);
					break;
				case (eButtonAlign.Right):
					m_Bounds.Location = Position;
					m_Bounds.X -= (int)vTextSize.X;
					break;
			}
			
			m_Bounds.Width = (int)vTextSize.X;
			m_Bounds.Height = (int)vTextSize.Y;

			m_Text = Text;
			m_Font = Font;
			m_ColorDefault = Default;
			m_ColorFocus = MouseOver;

			OnClick += Callback;

			MakeTooltip(TooltipKey, list);
		}

		// This is for "hotzone" buttons that just provide tooltips
		public Button(Rectangle rect, string TooltipKey, LinkedList<TipData> list = null)
		{
			m_Bounds = rect;
			MakeTooltip(TooltipKey, list);
		}

		// This is for image buttons
		public Button(Point Position, Texture2D DefaultTexture, Texture2D FocusTexture,
			Action Callback, string TooltipKey = "", LinkedList<TipData> list = null)
		{
			m_TexDefault = DefaultTexture;
			m_TexFocus = FocusTexture;

			m_Bounds.Location = Position;
			m_Bounds.Width = m_TexDefault.Width;
			m_Bounds.Height = m_TexDefault.Height;

			OnClick += Callback;

			
			MakeTooltip(TooltipKey, list);
		}

		private void MakeTooltip(string TooltipKey, LinkedList<TipData> list)
		{
			if (TooltipKey != "")
			{
				Vector2 vBottomLeft = new Vector2(m_Bounds.X, m_Bounds.Y);
				vBottomLeft.X += m_Bounds.Width;
				if (vBottomLeft.X + GlobalDefines.fToolTipMaxWidth >
					GraphicsManager.Get().Width)
				{
					vBottomLeft.X = GraphicsManager.Get().Width - GlobalDefines.fToolTipMaxWidth - 10.0f;
				}

				m_Tooltip = new Tooltip(TooltipKey, vBottomLeft, list);
			}
		}

		public Rectangle m_Bounds = new Rectangle();
		
		public Texture2D m_TexDefault;
		public Texture2D m_TexFocus;
		
		public SpriteFont m_Font;
		public string m_Text = "";
		public Color m_ColorDefault;
		public Color m_ColorFocus;
		public Tooltip m_Tooltip = null;

		public Action OnClick;
		public void Click()
		{
			if (OnClick != null)
			{
				OnClick();
			}
		}

		private bool m_IsFocused;
		public bool HasFocus
		{
			set { m_IsFocused = value; }
			get { return m_IsFocused; }
		}

		private bool m_bEnabled = true;
		public bool Enabled
		{
			get { return m_bEnabled; }
			set { m_bEnabled = value; }
		}

		public void Draw(float fDeltaTime, SpriteBatch DrawBatch)
		{
			if (HasFocus)
			{
				if (m_TexFocus != null)
				{
					DrawBatch.Draw(m_TexFocus, m_Bounds, Color.White);
				}

				if (m_Text != "")
				{
					DrawBatch.DrawString(m_Font, m_Text, new Vector2(m_Bounds.X, m_Bounds.Y), m_ColorFocus);
				}
			}
			else
			{
				if (m_TexDefault != null)
				{
					DrawBatch.Draw(m_TexDefault, m_Bounds, Color.White);
				}

				if (m_Text != "")
				{
					DrawBatch.DrawString(m_Font, m_Text, new Vector2(m_Bounds.X, m_Bounds.Y), m_ColorDefault);
				}
			}
#if DEBUG
			// Draw the bounds of the rect
			if (DebugDefines.bDrawButtonBounds)
			{
				Vector2 vStart = new Vector2(m_Bounds.X, m_Bounds.Y);
				Vector2 vEnd = vStart;
				vEnd.X += m_Bounds.Width;
				// Top
				GraphicsManager.Get().DrawLine(DrawBatch, 1.0f, Color.White, vStart, vEnd);
				
				// Left
				vEnd = vStart;
				vEnd.Y += m_Bounds.Height;
				GraphicsManager.Get().DrawLine(DrawBatch, 1.0f, Color.White, vStart, vEnd);

				// Bottom
				vStart.Y += m_Bounds.Height;
				vEnd = vStart;
				vEnd.X += m_Bounds.Width;
				GraphicsManager.Get().DrawLine(DrawBatch, 1.0f, Color.White, vStart, vEnd);

				// Right
				vStart = new Vector2(m_Bounds.X, m_Bounds.Y);
				vStart.X += m_Bounds.Width;
				vEnd = vStart;
				vEnd.Y += m_Bounds.Height;
				GraphicsManager.Get().DrawLine(DrawBatch, 1.0f, Color.White, vStart, vEnd);
			}
#endif
		}
	}
}

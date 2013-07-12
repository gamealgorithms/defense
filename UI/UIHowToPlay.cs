//-----------------------------------------------------------------------------
// UIHowToPlay has the instructions for how to play the game.
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
	public class UIHowToPlay : UIScreen
	{
		SpriteFont m_TitleFont;
		SpriteFont m_ButtonFont;
		string m_Title;

		Utils.CompiledMarkup m_Instructions;

		public UIHowToPlay(ContentManager Content) :
			base(Content)
		{
			m_TitleFont = m_Content.Load<SpriteFont>("Fonts/QuartzTitle");
			m_ButtonFont = m_Content.Load<SpriteFont>("Fonts/QuartzButton");
			m_Title = Localization.Get().Text("ui_how_title");

			m_bCanExit = true;

			// Create buttons
			Point vPos = new Point();
			vPos.X = (int)(GraphicsManager.Get().Width / 2.0f);
			vPos.Y = (int)(GraphicsManager.Get().Height - 100);
					
			m_Buttons.AddLast(new Button(vPos, "ui_how_back",
				m_ButtonFont, Color.DarkBlue,
				Color.White, Back, eButtonAlign.Center));

			// Generate the instructions block
			m_Instructions = GraphicsManager.Get().MarkupEngine.Compile(
				Localization.Get().Text("ui_how_instructions"), GraphicsManager.Get().Width / 1.5f);
		}

		public override void Update(float fDeltaTime)
		{
			base.Update(fDeltaTime);
		}

		public override void Draw(float fDeltaTime, SpriteBatch DrawBatch)
		{
			// Draw background
			GraphicsManager g = GraphicsManager.Get();
			Rectangle rect = new Rectangle(0, 0, g.Width, g.Height);
			g.DrawFilled(DrawBatch, rect, Color.Black, 4.0f, Color.Black);

			// Title
			Vector2 vOffset = Vector2.Zero;
			vOffset.Y = -1.0f * GraphicsManager.Get().Height / 2.5f;
			DrawCenteredString(DrawBatch, m_Title, m_TitleFont, Color.DarkBlue, vOffset);
			
			// Instruction block
			vOffset.X = GraphicsManager.Get().Width / 2.0f - m_Instructions.Size.X / 2.0f;
			vOffset.Y = GraphicsManager.Get().Height / 2.0f - m_Instructions.Size.Y / 2.0f;
			m_Instructions.Draw(DrawBatch, vOffset);
			base.Draw(fDeltaTime, DrawBatch);
		}

		public void Back()
		{
			SoundManager.Get().PlaySoundCue("MenuClick");
			GameState.Get().PopUI();
		}

		public override void OnExit()
		{
			SoundManager.Get().PlaySoundCue("MenuClick");
			base.OnExit();
		}
	}
}

//-----------------------------------------------------------------------------
// UIGameOver comes up when the game ends, either win or lose.
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
	public class UIGameOver : UIScreen
	{
		SpriteFont m_TitleFont;
		SpriteFont m_ButtonFont;
		string m_GameOverText;
		bool m_bVictorious;

		public UIGameOver(ContentManager Content, bool victory) :
			base(Content)
		{
			m_bVictorious = victory;
			m_bCanExit = false;

			m_TitleFont = m_Content.Load<SpriteFont>("Fonts/FixedTitle");
			m_ButtonFont = m_Content.Load<SpriteFont>("Fonts/FixedButton");

			if (m_bVictorious)
			{
				m_GameOverText = Localization.Get().Text("ui_victory");
				SoundManager.Get().PlaySoundCue("Victory");
			}
			else
			{
				m_GameOverText = Localization.Get().Text("ui_defeat");
				SoundManager.Get().PlaySoundCue("GameOver");
			}
			
			// Create buttons
			Point vPos = new Point();
			vPos.X = (int) (GraphicsManager.Get().Width / 2.0f);
			vPos.Y = (int)(GraphicsManager.Get().Height / 2.0f);

			m_Buttons.AddLast(new Button(vPos, "ui_quit",
				m_ButtonFont, new Color(0, 0, 200),
				Color.White, Quit, eButtonAlign.Center));
		}

		public void Quit()
		{
			GameState.Get().SetState(eGameState.MainMenu);
		}

		public override void Update(float fDeltaTime)
		{
			base.Update(fDeltaTime);
		}

		public override void Draw(float fDeltaTime, SpriteBatch DrawBatch)
		{
			// Draw background
			GraphicsManager g = GraphicsManager.Get();
			Rectangle rect = new Rectangle(g.Width / 2 - 200, g.Height / 2 - 115,
				400, 200);
			g.DrawFilled(DrawBatch, rect, Color.Black, 4.0f, Color.DarkBlue);

			Vector2 vOffset = Vector2.Zero;
			vOffset.Y -= 75;
			Color color = Color.Green;
			if (!m_bVictorious)
			{
				color = Color.Red;
			}
			DrawCenteredString(DrawBatch, m_GameOverText, m_TitleFont, color, vOffset);
						
			base.Draw(fDeltaTime, DrawBatch);
		}

		public override void OnExit()
		{
			GameState.Get().IsPaused = false;
		}
	}
}

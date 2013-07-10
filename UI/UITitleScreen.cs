//-----------------------------------------------------------------------------
// UITitleScreen is currently not really used, though if you change
// GlobalDefines.fTitleFadeTime, it will show up on load.
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
	public class UITitleScreen : UIScreen
	{
		SpriteFont m_TitleFont;
		string m_Title;

		public UITitleScreen(ContentManager Content) :
			base(Content)
		{
			m_TitleFont = m_Content.Load<SpriteFont>("Fonts/QuartzTitle");
			m_Title = Localization.Get().Text("ui_title");
		}

		public override void Update(float fDeltaTime)
		{
			if (m_fLiveTime > GlobalDefines.fTitleFadeTime)
			{
				GameState.Get().SetState(eGameState.MainMenu);
			}
			base.Update(fDeltaTime);
		}

		public override void Draw(float fDeltaTime, SpriteBatch DrawBatch)
		{
			Vector2 vOffset = Vector2.Zero;
			vOffset.Y = -1.0f * GraphicsManager.Get().Height / 4.0f;
			float C = MathHelper.Clamp(m_fLiveTime / GlobalDefines.fTitleFadeTime, 0.0f, 1.0f);
			Color TextColor = Color.Lerp(Color.Black, Color.DarkBlue, C);
			DrawCenteredString(DrawBatch, m_Title, m_TitleFont, TextColor, vOffset);

			base.Draw(fDeltaTime, DrawBatch);
		}
	}
}

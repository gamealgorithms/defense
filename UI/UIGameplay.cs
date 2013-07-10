//-----------------------------------------------------------------------------
// UIGameplay is UI while in the main game state.
// Because there are so many aspects to the UI, this class is relatively large.
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
	public enum eGridState
	{
		Default = 0,
		Build,
		Upgrade,
		NUM_GRIDS
	}

	public class UIGameplay : UIScreen
	{
		Texture2D m_RightPanel;
		Texture2D m_LeftPanel;
		SpriteFont m_FixedFont;
		SpriteFont m_FixedSmall;
		SpriteFont m_StatusFont;
		
		LinkedList<Button>[] m_BtnGrids = new LinkedList<Button>[(int)eGridState.NUM_GRIDS];
		eGridState m_CurrentGrid = eGridState.Default;

		#region Localized Strings
		// Resource Display
		string m_MoneyStr;
		string m_LifeStr;
		string m_WaveStr;
		string m_WaveTimeStr;
		string m_WaveActiveStr;

		// Build mode strings
		string m_BuildStr;

		// Selected tile strings
		string m_TileStr;
		string m_BuildingStr;
		string m_StatusStr;
		string m_ActiveStr;
		string m_DisabledStr;
		string m_BeingBuiltStr;
		string m_EnemyStr;

		// Speed display strings
		string m_SpeedStr;
		string m_SlowStr;
		string m_PausedStr;
		string m_NormalStr;
		string m_FastStr;
		string m_FastestStr;
		#endregion

		// Current status message being displayed in center of screen
		string m_StatusMessage = "";

		// Current error message being displayed\
		string m_ErrorMessage = "";

		public UIGameplay(ContentManager Content) :
			base(Content)
		{
			m_RightPanel = Content.Load<Texture2D>("UI/UI_Right");
			m_LeftPanel = Content.Load<Texture2D>("UI/UI_Left");
			m_FixedFont = Content.Load<SpriteFont>("Fonts/FixedText");
			m_FixedSmall = Content.Load<SpriteFont>("Fonts/FixedSmall");
			m_StatusFont = Content.Load<SpriteFont>("Fonts/FixedTitle");

			// Load the massive number of localized strings, so we don't repeat lookups
			InitializeLocStrings(Content);

			// Hot zones for tooltips
			int Width = GraphicsManager.Get().Width;
			int Height = GraphicsManager.Get().Height;
			const int iSmallSpacing = 22;

			Rectangle rect = new Rectangle(Width - 290, Height - 223, 200, 22);

			// Life
			m_Buttons.AddLast(new Button(rect, "tip_life"));
			rect.Y += iSmallSpacing;

			// Money
			m_Buttons.AddLast(new Button(rect, "tip_money"));
			rect.Y += iSmallSpacing * 2;

			// Wave
			LinkedList<TipData> tipdata = new LinkedList<TipData>();
			tipdata.AddLast(new TipData(eTipData.Generic, Balance.TotalWaves));
			m_Buttons.AddLast(new Button(rect, "tip_wave", tipdata));
			rect.Y += iSmallSpacing;

			// Money
			m_Buttons.AddLast(new Button(rect, "tip_wavetime"));
			rect.Y += iSmallSpacing * 2;

			// Speed
			tipdata.Clear();
			tipdata.AddLast(new TipData(eTipData.Keybind, eBindings.UI_Slow));
			tipdata.AddLast(new TipData(eTipData.Keybind, eBindings.UI_Fast));
			tipdata.AddLast(new TipData(eTipData.Keybind, eBindings.UI_Stop));
			m_Buttons.AddLast(new Button(rect, "tip_speed", tipdata));
			rect.Y += iSmallSpacing;

			// Initialize the buttons for the bottom left grids
			InitializeGrid(Content);
		}

		#region Initialization Helper Functions
		private void InitializeGrid(ContentManager Content)
		{
			// Make the list of buttons for each type of grid.
			// This is super messy and should be more data-driven. But it works.

			// This is the position on the screen of the top left button on the grid
			Point StartPos = Point.Zero;
			StartPos.Y = GraphicsManager.Get().Height - m_LeftPanel.Height + 44;
			StartPos.X += 16;

			// These are used for the default and focus textures of the respective grid button
			Texture2D DefaultTex = null;
			Texture2D FocusTex = null;

			// These integers represent the row/column of the button in the grid; this is used
			// with the button size constant to know where a button should be.
			int iGridRow = 0;
			int iGridCol = 0;
			const int ButtonSize = 66;

			// This is the data used to store all of the grids
			Button b = null;
			int iBtnGrid = (int)eGridState.Default;
			m_BtnGrids[iBtnGrid] = new LinkedList<Button>();
			LinkedList<TipData> tipdata = new LinkedList<TipData>();

			// The default grid (nothing selected) has no buttons
			// So don't do anything here

			// Make the build button grid
			// This grid is when you click on a hex that doesn't have a building currently
			iGridRow = 0;
			iGridCol = 0;
			iBtnGrid = (int)eGridState.Build;
			m_BtnGrids[iBtnGrid] = new LinkedList<Button>();

			// Projectile Tower button
			tipdata.Clear();
			tipdata.AddLast(new TipData(eTipData.Keybind, eBindings.UI_ProjectileTower));
			tipdata.AddLast(new TipData(eTipData.TowerBuildCost, eTowerType.Projectile));
			tipdata.AddLast(new TipData(eTipData.TowerBuildTime, eTowerType.Projectile));
			DefaultTex = Content.Load<Texture2D>("UI/Btn_Projectile_Default");
			FocusTex = Content.Load<Texture2D>("UI/Btn_Projectile_Focus");
			b = new Button(new Point(StartPos.X + iGridCol * ButtonSize, StartPos.Y + iGridRow * ButtonSize),
				DefaultTex, FocusTex, Build_ProjectileTower, "tip_projectiletower",
				tipdata);
			b.Enabled = false;
			m_Buttons.AddLast(b);
			m_BtnGrids[iBtnGrid].AddLast(b);
			iGridCol++;

			// Slow Tower button
			tipdata.Clear();
			tipdata.AddLast(new TipData(eTipData.Keybind, eBindings.UI_SlowTower));
			tipdata.AddLast(new TipData(eTipData.TowerBuildCost, eTowerType.Slow));
			tipdata.AddLast(new TipData(eTipData.TowerBuildTime, eTowerType.Slow));
			DefaultTex = Content.Load<Texture2D>("UI/Btn_Slow_Default");
			FocusTex = Content.Load<Texture2D>("UI/Btn_Slow_Focus");
			b = new Button(new Point(StartPos.X + iGridCol * ButtonSize, StartPos.Y + iGridRow * ButtonSize),
				DefaultTex, FocusTex, Build_SlowTower, "tip_slowtower",
				tipdata);
			b.Enabled = false;
			m_Buttons.AddLast(b);
			m_BtnGrids[iBtnGrid].AddLast(b);
			iGridCol++;

			// Make the Upgrade grid
			// This grid is when you click on a hex that has a building already (other than the base)
			iGridRow = 0;
			iGridCol = 0;
			iBtnGrid = (int)eGridState.Upgrade;
			m_BtnGrids[iBtnGrid] = new LinkedList<Button>();

			// Upgrade button
			tipdata.Clear();
			tipdata.AddLast(new TipData(eTipData.TowerName));
			tipdata.AddLast(new TipData(eTipData.Keybind, eBindings.UI_Upgrade));
			tipdata.AddLast(new TipData(eTipData.TowerLevel));
			tipdata.AddLast(new TipData(eTipData.TowerUpgradeCost));
			tipdata.AddLast(new TipData(eTipData.TowerUpgradeTime));
			tipdata.AddLast(new TipData(eTipData.TowerUpgradeText));
			DefaultTex = Content.Load<Texture2D>("UI/Btn_Upgrade_Default");
			FocusTex = Content.Load<Texture2D>("UI/Btn_Upgrade_Focus");
			b = new Button(new Point(StartPos.X + iGridCol * ButtonSize, StartPos.Y + iGridRow * ButtonSize),
				DefaultTex, FocusTex, Upgrade_Upgrade, "tip_upgrade",
				tipdata);
			b.Enabled = false;
			m_Buttons.AddLast(b);
			m_BtnGrids[iBtnGrid].AddLast(b);
			iGridCol++;

			// Cancel button
			iGridRow = 2;
			iGridCol = 3;
			tipdata.Clear();
			tipdata.AddLast(new TipData(eTipData.Keybind, eBindings.UI_Delete));
			tipdata.AddLast(new TipData(eTipData.TowerRefund));
			DefaultTex = Content.Load<Texture2D>("UI/Btn_Cancel_Default");
			FocusTex = Content.Load<Texture2D>("UI/Btn_Cancel_Focus");
			b = new Button(new Point(StartPos.X + iGridCol * ButtonSize, StartPos.Y + iGridRow * ButtonSize),
				DefaultTex, FocusTex, Upgrade_Delete, "tip_delete",
				tipdata);
			b.Enabled = false;
			m_Buttons.AddLast(b);
			m_BtnGrids[iBtnGrid].AddLast(b);
			iGridCol++;
		}

		private void InitializeLocStrings(ContentManager Content)
		{
			m_MoneyStr = Localization.Get().Text("ui_money");
			m_LifeStr = Localization.Get().Text("ui_life");
			m_WaveStr = Localization.Get().Text("ui_wave");
			m_WaveTimeStr = Localization.Get().Text("ui_wavetime");
			m_WaveActiveStr = Localization.Get().Text("ui_waveactive");

			m_BuildStr = Localization.Get().Text("ui_build");

			m_TileStr = Localization.Get().Text("ui_tile");
			m_BuildingStr = Localization.Get().Text("ui_building");
			m_StatusStr = Localization.Get().Text("ui_status");
			m_ActiveStr = Localization.Get().Text("ui_active");
			m_DisabledStr = Localization.Get().Text("ui_disabled");
			m_BeingBuiltStr = Localization.Get().Text("ui_beingbuilt");
			m_EnemyStr = Localization.Get().Text("ui_enemystr");

			m_SpeedStr = Localization.Get().Text("ui_speed");
			m_SlowStr = Localization.Get().Text("ui_slow");
			m_PausedStr = Localization.Get().Text("ui_paused");
			m_NormalStr = Localization.Get().Text("ui_normal");
			m_FastStr = Localization.Get().Text("ui_fast");
			m_FastestStr = Localization.Get().Text("ui_fastest");
		}
		#endregion

		public override void Update(float fDeltaTime)
		{
			base.Update(fDeltaTime);
		}

		public override void Draw(float fDeltaTime, SpriteBatch DrawBatch)
		{
			if (DebugDefines.bShowGlobalPath)
			{
				Pathfinder.Get().DrawPath(DrawBatch);
			}

			// Draw the bottom right panel BG
			DrawRightPanel(DrawBatch);
						
			// Draw the bottom left panel BG
			DrawLeftPanel(DrawBatch);

			// Draw status and error messages
			DrawStatusMessage(DrawBatch);
			DrawErrorMessage(DrawBatch);
			
			base.Draw(fDeltaTime, DrawBatch);
		}

		#region Draw Helper Functions
		void DrawRightPanel(SpriteBatch DrawBatch)
		{
			Vector2 vRightPos = new Vector2(GraphicsManager.Get().Width, GraphicsManager.Get().Height);
			vRightPos.X -= m_RightPanel.Width;
			vRightPos.Y -= m_RightPanel.Height;
			DrawBatch.Draw(m_RightPanel, vRightPos, Color.White);

			// Draw text for bottom right panel
			GameState g = GameState.Get();
			const float fSmallSpacing = 22.0f;
			Vector2 vStatOffset = new Vector2(220, 32);
			Color StatColor = Color.White;
			string DisplayString;

			// Life
			DisplayString = m_LifeStr + String.Format("{0:N0}", g.Life);
			DrawBatch.DrawString(m_FixedSmall, DisplayString,
				vRightPos + vStatOffset, Color.White);
			vStatOffset.Y += fSmallSpacing;

			// Money
			DisplayString = m_MoneyStr + String.Format("{0:N0}", g.Money);
			DrawBatch.DrawString(m_FixedSmall, DisplayString,
				vRightPos + vStatOffset, Color.White);
			vStatOffset.Y += fSmallSpacing * 2;

			// Wave
			DisplayString = m_WaveStr + String.Format("{0:N0}", g.WaveNumber);
			DrawBatch.DrawString(m_FixedSmall, DisplayString,
				vRightPos + vStatOffset, Color.White);
			vStatOffset.Y += fSmallSpacing;

			// Next Wave Time
			if (!g.IsWaveActive)
			{
				DisplayString = m_WaveTimeStr + String.Format("{0:F2}", g.NextWaveTime);
				DrawBatch.DrawString(m_FixedSmall, DisplayString,
					vRightPos + vStatOffset, Color.White);
			}
			else
			{
				DisplayString = m_WaveActiveStr;
				DrawBatch.DrawString(m_FixedSmall, DisplayString,
					vRightPos + vStatOffset, Color.Red);
			}
			vStatOffset.Y += fSmallSpacing * 2;

			// Speed
			DisplayString = m_SpeedStr;
			switch (g.Speed)
			{
				case (eGameSpeed.Stop): DisplayString += m_PausedStr; break;
				case (eGameSpeed.Slow): DisplayString += m_SlowStr; break;
				case (eGameSpeed.Normal): DisplayString += m_NormalStr; break;
				case (eGameSpeed.Fast): DisplayString += m_FastStr; break;
				case (eGameSpeed.Fastest): DisplayString += m_FastestStr; break;
			}

			DrawBatch.DrawString(m_FixedSmall, DisplayString,
				vRightPos + vStatOffset, Color.White);
			vStatOffset.Y += fSmallSpacing * 2;
		}

		void DrawLeftPanel(SpriteBatch DrawBatch)
		{
			const float fSmallSpacing = 22.0f;
			Color StatColor = Color.White;
			GameState g = GameState.Get();

			Vector2 vLeftPos = Vector2.Zero;
			vLeftPos.Y = GraphicsManager.Get().Height - m_LeftPanel.Height;
			DrawBatch.Draw(m_LeftPanel, vLeftPos, Color.White);

			// Text for selected tile
			Vector2 vSelectOffset = new Vector2(300, 150);
			if (g.SelectedTile != null)
			{
				Objects.Tower b = g.SelectedTile.Tower;
				// If there's a building on this tile, show its info
				if (b != null)
				{
					string BuildingDisplay = b.Name;
					DrawBatch.DrawString(m_FixedSmall, BuildingDisplay, vLeftPos + vSelectOffset, Color.White);
					vSelectOffset.Y += fSmallSpacing;

					string StatusDisplay = "";
					if (b.IsBeingBuilt)
					{
						StatusDisplay = String.Format(m_BeingBuiltStr, b.BuildPercent);
						DrawBatch.DrawString(m_FixedSmall, StatusDisplay,
							vLeftPos + vSelectOffset, Color.White);
					}
					else
					{
						StatusDisplay = m_ActiveStr;
						StatColor = Color.Green;

						DrawBatch.DrawString(m_FixedSmall, StatusDisplay,
							vLeftPos + vSelectOffset, StatColor);
					}

					vSelectOffset.Y += fSmallSpacing * 2;
				}
				else if (!g.SelectedTile.Buildable)
				{
					DrawBatch.DrawString(m_FixedSmall, m_EnemyStr, vLeftPos + vSelectOffset, Color.Red);
					vSelectOffset.Y += fSmallSpacing;
				}
			}
		}

		void DrawStatusMessage(SpriteBatch DrawBatch)
		{
			if (m_StatusMessage != "")
			{
				// Grab the size of the text so it can be centered in the screen
				Vector2 vTextSize = m_StatusFont.MeasureString(m_StatusMessage);

				Vector2 vPosition = new Vector2(GraphicsManager.Get().Width / 2 - vTextSize.X / 2,
												GraphicsManager.Get().Height / 2 - vTextSize.Y);

				DrawBatch.DrawString(m_StatusFont, m_StatusMessage, vPosition, Color.Red);
			}
		}

		void DrawErrorMessage(SpriteBatch DrawBatch)
		{
			if (m_ErrorMessage != "")
			{
				// Grab the size of the text so it can be centered in the screen
				Vector2 vTextSize = m_FixedFont.MeasureString(m_ErrorMessage);

				Vector2 vPosition = new Vector2(GraphicsManager.Get().Width / 2 - vTextSize.X / 2,
												GraphicsManager.Get().Height - 200);

				DrawBatch.DrawString(m_FixedFont, m_ErrorMessage, vPosition, Color.Red);
			}
		}
		#endregion

		public override void KeyboardInput(SortedList<eBindings, BindInfo> binds)
		{
			GameState g = GameState.Get();
			if (binds.ContainsKey(eBindings.UI_Exit))
			{
				g.ShowPauseMenu();
				binds.Remove(eBindings.UI_Exit);
			}

			if (binds.ContainsKey(eBindings.UI_Stop))
			{
				if (g.Speed == eGameSpeed.Stop)
				{
					g.Speed = eGameSpeed.Normal;
				}
				else
				{
					g.Speed = eGameSpeed.Stop;
				}

				binds.Remove(eBindings.UI_Stop);
			}

			if (binds.ContainsKey(eBindings.UI_Slow))
			{
				g.DecreaseSpeed();
				binds.Remove(eBindings.UI_Slow);
			}

			if (binds.ContainsKey(eBindings.UI_Fast))
			{
				g.IncreaseSpeed();
				binds.Remove(eBindings.UI_Fast);
			}

			// Check keys based on grid state
			if (m_CurrentGrid == eGridState.Build)
			{
				if (binds.ContainsKey(eBindings.UI_ProjectileTower))
				{
					Build_ProjectileTower();
					binds.Remove(eBindings.UI_ProjectileTower);
				}
				else if (binds.ContainsKey(eBindings.UI_SlowTower))
				{
					Build_SlowTower();
					binds.Remove(eBindings.UI_SlowTower);
				}
			}
			else if (m_CurrentGrid == eGridState.Upgrade)
			{
				if (binds.ContainsKey(eBindings.UI_Delete))
				{
					Upgrade_Delete();
					binds.Remove(eBindings.UI_Delete);
				}
			}

			base.KeyboardInput(binds);
		}

		// Try to build a building as requested
		protected void TryBuild(eTowerType type)
		{
			bool bSuccess = GameState.Get().TryBuild(type);
			if (bSuccess)
			{
				SoundManager.Get().PlaySoundCue("Build");
				SwitchGrid(eGridState.Upgrade);
			}
		}

		// This function is called by GameState when a new tile is selected
		public void NewSelectedTile(Objects.Tile t)
		{
			// Select the correct button grid depending on what's on this tile
			if (t == null)
			{
				SwitchGrid(eGridState.Default);
			}
			else if (t.Tower == null && t.Buildable)
			{
				SwitchGrid(eGridState.Build);
			}
			else if (t.Tower == null || t.Tower.TowerType == eTowerType.Base)
			{
				SwitchGrid(eGridState.Default);
			}
			else
			{
				SwitchGrid(eGridState.Upgrade);
			}
		}

		// Switches to specified button grid
		protected void SwitchGrid(eGridState NewGrid)
		{
			// Disable all the buttons in the old grid
			foreach (Button b in m_BtnGrids[(int)m_CurrentGrid])
			{
				b.Enabled = false;
			}

			// Enable all of the buttons in the new grid
			m_CurrentGrid = NewGrid;
			foreach (Button b in m_BtnGrids[(int)m_CurrentGrid])
			{
				b.Enabled = true;
			}
		}
		
		// This function is called when a building is built on the current tile,
		// which means we should now show the Upgrade grid.
		public void FinishBuild()
		{
			SwitchGrid(eGridState.Upgrade);
		}

		#region Button Actions for Grid
		// Default Grid Button Actions

		// Build Grid Button Actions
		protected void Build_ProjectileTower()
		{
			TryBuild(eTowerType.Projectile);
		}

		protected void Build_SlowTower()
		{
			TryBuild(eTowerType.Slow);
		}

		// Upgrade Grid Button Actions
		public void Upgrade_Upgrade()
		{
			GameState.Get().TryUpgrade();
		}

		public void Upgrade_Delete()
		{
			SoundManager.Get().PlaySoundCue("MenuClick");
			GameState.Get().DeleteSelectedTower();
		}
		#endregion

		// Show a status message in the center of the screen
		public void ShowStatusMessage(string text, float duration = 1.5f)
		{
			m_StatusMessage = text;

			// Remove it if it already exists
			m_Timer.RemoveTimer("HideStatus");
			m_Timer.AddTimer("HideStatus", duration, ClearStatusMessage, false);
		}

		public void ClearStatusMessage()
		{
			m_StatusMessage = "";
		}

		public void ShowErrorMessage(string text, float duration = 1.0f)
		{
			// If this message is already up, don't spam it
			if (text != m_ErrorMessage)
			{
				SoundManager.Get().PlaySoundCue("Error");
				m_ErrorMessage = text;

				// Remove it if it already exists
				m_Timer.RemoveTimer("HideError");
				m_Timer.AddTimer("HideError", duration, ClearErrorMessage, false);
			}
		}

		public void ClearErrorMessage()
		{
			m_ErrorMessage = "";
		}
	}
}

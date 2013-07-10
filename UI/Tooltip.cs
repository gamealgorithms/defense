//-----------------------------------------------------------------------------
// The Tooltip class helps generate all the mouseover tooltips.
// It's pretty complicated.
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
	public enum eTipData
	{
		Keybind = 0,
		TowerBuildTime,
		TowerBuildCost,
		TowerRefund,
		TowerName,
		TowerLevel,
		TowerUpgradeCost,
		TowerUpgradeTime,
		TowerUpgradeText,
		Generic,
	}

	public class TipData
	{
		public eTipData Type;
		public object Data;
		public TipData(eTipData type, object data = null)
		{
			Type = type;
			Data = data;
		}
	}

	public class Tooltip
	{
		string m_Key;
		LinkedList<TipData> m_TipData;
		Utils.CompiledMarkup m_Markup;
		Vector2 m_vBottomLeft;

		public Tooltip(string Key, Vector2 vBottomLeft, LinkedList<TipData> data)
		{
			m_vBottomLeft = vBottomLeft;
			m_Key = Key;
			if (data != null)
			{
				m_TipData = new LinkedList<TipData>(data);
			}
			GenerateMarkup();
		}

		// This is called before every draw to make sure it's correct
		protected void GenerateMarkup()
		{
			// Build list of parameters to insert
			List<object> list = new List<object>();
			if (m_TipData != null)
			{
				foreach (TipData t in m_TipData)
				{
					switch (t.Type)
					{
						case (eTipData.Keybind):
							list.Add(InputManager.Get().GetBinding((eBindings)t.Data));
							break;
						case (eTipData.TowerBuildTime):
							list.Add(CreateTowerBuildTime((eTowerType)t.Data));
							break;
						case (eTipData.TowerBuildCost):
							list.Add(CreateTowerBuildCost((eTowerType)t.Data));
							break;
						case (eTipData.TowerRefund):
							list.Add(CreateTowerRefund());
							break;
						case (eTipData.TowerName):
							list.Add(CreateTowerName());
							break;
						case (eTipData.TowerLevel):
							list.Add(CreateTowerLevel());
							break;
						case (eTipData.TowerUpgradeCost):
							list.Add(CreateTowerUpgradeCost());
							break;
						case (eTipData.TowerUpgradeTime):
							list.Add(CreateTowerUpgradeTime());
							break;
						case (eTipData.TowerUpgradeText):
							list.Add(CreateTowerUpgradeText());
							break;
						case (eTipData.Generic):
							list.Add(t.Data);
							break;
					}
				}
			}

			string Text = "<font face='Tooltip' color='#ffffff'>" +
				String.Format(Localization.Get().Text(m_Key), list.ToArray())
				+ "</font>";

			m_Markup = GraphicsManager.Get().MarkupEngine.Compile(Text, GlobalDefines.fToolTipMaxWidth);
		}

		public void Draw(float fDeltaTime, SpriteBatch DrawBatch)
		{
			GenerateMarkup();
			Vector2 vPosition = m_vBottomLeft;
			vPosition.Y -= m_Markup.Size.Y;

			// Draw my BG
			Rectangle rect = new Rectangle((int)vPosition.X - 4, (int)vPosition.Y - 1,
				(int)m_Markup.Size.X + 4, (int)m_Markup.Size.Y + 1);
			GraphicsManager.Get().DrawFilled(DrawBatch, rect, Color.Black, 2.0f, Color.DarkBlue);

			// Draw some lines to make a BOX


			// Now draw the wrapped/markup text
			m_Markup.Draw(DrawBatch, vPosition);
		}

		// Cost to build a level 1 tower of this type
		protected string CreateTowerBuildCost(eTowerType type)
		{
			string str = "";
			int cost = 0;
			switch (type)
			{
				case (eTowerType.Projectile):
					cost = Balance.ProjectileTower[0].BuildCost;
					break;
				case (eTowerType.Slow):
					cost = Balance.SlowTower[0].BuildCost;
					break;
			}
			GameState g = GameState.Get();
			if (cost > g.Money)
			{
				str = String.Format("<font color='#ff0000'>${0:N0}</font>", cost);
			}
			else
			{
				str = String.Format("${0:N0}", cost);
			}

			return str;
		}

		// Amount of time it takes to build a level 1 tower of this type
		protected string CreateTowerBuildTime(eTowerType type)
		{
			string str = "";
			float time = 0.0f;
			switch (type)
			{
				case (eTowerType.Projectile):
					time = Balance.ProjectileTower[0].BuildTime;
					break;
				case (eTowerType.Slow):
					time = Balance.SlowTower[0].BuildTime;
					break;
			}
			str = String.Format("{0:F1}", time);

			return str;
		}

		// Get the amount of refund for the current selected tower
		protected string CreateTowerRefund()
		{
			string str = "";

			if (GameState.Get().SelectedTile != null)
			{
				Objects.Tower t = GameState.Get().SelectedTile.Tower;
				if (t != null)
				{
					str = String.Format("{0:N0}", t.GetRefundAmount());
				}
			}
			
			return str;
		}

		// Get the name of the current selected tower
		protected string CreateTowerName()
		{
			string str = "";

			if (GameState.Get().SelectedTile != null)
			{
				Objects.Tower t = GameState.Get().SelectedTile.Tower;
				if (t != null)
				{
					str = t.Name;
				}
			}

			return str;
		}

		// Get the level information of current tower
		protected string CreateTowerLevel()
		{
			string str = "";

			if (GameState.Get().SelectedTile != null)
			{
				Objects.Tower t = GameState.Get().SelectedTile.Tower;
				if (t != null)
				{
					// At the maximum level
					if (t.Level == t.MaxLevel)
					{
						str = String.Format("<font color='#00ff00'>{0}</font>", Localization.Get().Text("max_level"));
					}
					else
					{
						str = String.Format("{0}{1}{2}", t.Level, Localization.Get().Text("level_separator"),
							t.MaxLevel);
					}
				}
			}

			return str;
		}

		// Get the upgrade cost for the current tower
		protected string CreateTowerUpgradeCost()
		{
			string str = "";

			if (GameState.Get().SelectedTile != null)
			{
				Objects.Tower t = GameState.Get().SelectedTile.Tower;
				if (t != null)
				{
					// At the maximum level
					if (t.Level == t.MaxLevel)
					{
						str = Localization.Get().Text("n_a");
					}
					else
					{
						int cost = t.GetUpgradeCost();
						GameState g = GameState.Get();
						if (cost > g.Money)
						{
							str = String.Format("<font color='#ff0000'>${0:N0}</font>", cost);
						}
						else
						{
							str = String.Format("${0:N0}", cost);
						}
					}
				}
			}

			return str;
		}

		// Get the upgrade time for the current tower
		protected string CreateTowerUpgradeTime()
		{
			string str = "";

			if (GameState.Get().SelectedTile != null)
			{
				Objects.Tower t = GameState.Get().SelectedTile.Tower;
				if (t != null)
				{
					// At the maximum level
					if (t.Level == t.MaxLevel)
					{
						str = Localization.Get().Text("n_a");
					}
					else
					{
						str = String.Format("{0:F1}", t.GetUpgradeTime());
					}
				}
			}

			return str;
		}

		// Get the text describing the upgrade potential
		protected string CreateTowerUpgradeText()
		{
			string str = "";

			if (GameState.Get().SelectedTile != null)
			{
				Objects.Tower t = GameState.Get().SelectedTile.Tower;
				if (t != null)
				{
					if (t.TowerType == eTowerType.Projectile)
					{
						str = Localization.Get().Text("upgrade_projectile");
					}
					else if (t.TowerType == eTowerType.Slow)
					{
						str = Localization.Get().Text("upgrade_slow");
					}
				}
			}

			return str;
		}
	}
}

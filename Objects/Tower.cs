//-----------------------------------------------------------------------------
// This is the base Tower class that ProjectileTower and SlowTower
// inherit from.
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

namespace defense
{
	public enum eTowerType
	{
		Base,
		Projectile,
		Slow,
		INVALID
	}
}

namespace defense.Objects
{
	public class Tower : GameObject
	{
		protected Texture2D m_Texture;
		protected string m_TextureName;

		protected Texture2D m_OffTexture;
		protected string m_OffName;

		protected eTowerType m_Type = eTowerType.INVALID;
		public eTowerType TowerType
		{
			get { return m_Type; }
		}
		protected Tile m_Owner;

		protected string m_DisplayKey;
		protected string m_DisplayName;
		public string Name
		{
			get { return m_DisplayName; }
		}

		protected bool m_bSpawned;
		public bool Spawned
		{
			get { return m_bSpawned; }
			set { m_bSpawned = value; }
		}

		protected bool m_bRunning;
		public bool IsRunning
		{
			get { return m_bRunning; }
		}

		protected bool m_bBuilding;
		public bool IsBeingBuilt
		{
			get { return m_bBuilding; }
		}

		float m_fBuildTime;
		float m_fTotalBuildTime;
		public int BuildPercent
		{
			get { return (int)((m_fTotalBuildTime - m_fBuildTime) / m_fTotalBuildTime * 100); }
		}

		// This must point to the correct tower data array for this tower
		protected TowerData[] m_TowerData;

		// The maximum level this tower can be. Must be set by child classes
		protected int m_MaxLevel;
		public int MaxLevel
		{
			get { return m_MaxLevel; }
		}

		// Current Level
		protected int m_Level = 1;
		public int Level
		{
			get { return m_Level; }
		}

		// Whether or not it can currently fire (used to enforce cooldown)
		protected bool m_bCanFire = true;

		// How much money has been spent to build this tower/upgrades?
		protected int m_TotalSpent;
		public int TotalSpent
		{
			get { return m_TotalSpent; }
		}

		public Tower(Game game):
			base(game)
		{
			m_ModelName = "Buildings/Building";
		}

		public override void Load()
		{
			base.Load();

			m_Texture = m_Game.Content.Load<Texture2D>(m_TextureName);
			m_OffTexture = m_Game.Content.Load<Texture2D>(m_OffName);
			m_DisplayName = Localization.Get().Text(m_DisplayKey);
		}

		public virtual void Build(Tile Owner)
		{
			m_Owner = Owner;

			GameState g = GameState.Get();
			// Use up resources to build this building
			g.Money -= m_TowerData[0].BuildCost;
			m_TotalSpent += m_TowerData[0].BuildCost;

			float BuildTime = m_TowerData[0].BuildTime;
			if (BuildTime > 0.01f)
			{
				m_bBuilding = true;
				m_fBuildTime = BuildTime;
				m_fTotalBuildTime = m_fBuildTime;
			}
			else
			{
				m_bBuilding = false;
				StartRunning();
			}
		}

		public override void Draw(float fDeltaTime)
		{
			// Depending on whether it's active or not,
			// we need to show the correct texture for the tower
			Texture2D texture = m_Texture;
			if (!m_bSpawned || !m_bRunning)
			{
				texture = m_OffTexture;
			}

			// Replace the texture
			foreach (ModelMesh mesh in m_Model.Meshes)
			{
				foreach (BasicEffect effect in mesh.Effects)
				{
					effect.Texture = texture;
				}
			}

			base.Draw(fDeltaTime);
		}

		public override void Update(float fDeltaTime)
		{
			if (m_bSpawned)
			{
				if (m_bBuilding)
				{
					m_fBuildTime -= fDeltaTime;
					if (m_fBuildTime <= 0.0f)
					{
						m_bBuilding = false;
						StartRunning();
					}
				}
				else
				{
					base.Update(fDeltaTime);
				}
			}
		}

		public virtual void StartRunning()
		{
			m_bRunning = true;
		}

		public virtual void StopRunning()
		{
			m_bRunning = false;
		}
		
		protected void AllowFiring()
		{
			m_bCanFire = true;
		}

		// Amount of money you'd get back if you destroy this tower
		public int GetRefundAmount()
		{
			float refundFactor = Balance.RefundPercent / 100.0f;
			return (int)(TotalSpent * refundFactor);
		}

		// Cost to upgrade
		public int GetUpgradeCost()
		{
			return m_TowerData[m_Level].BuildCost;
		}

		// Amount of time to upgrade
		public float GetUpgradeTime()
		{
			return m_TowerData[m_Level].BuildTime;
		}

		// Upgrade this tower by one level
		public void Upgrade()
		{
			GameState g = GameState.Get();
			// Use up resources to upgrade this building
			g.Money -= m_TowerData[m_Level].BuildCost;
			m_TotalSpent += m_TowerData[m_Level].BuildCost;

			// Set it in the build state while it's upgrading
			StopRunning();
			float BuildTime = m_TowerData[m_Level].BuildTime;
			m_bBuilding = true;
			m_fBuildTime = BuildTime;
			m_fTotalBuildTime = m_fBuildTime;

			// Increment the level
			m_Level++;
			// Scale up based on the level so there's some visual signal when a tower is leveled up
			Scale = 1.0f + (m_Level - 1) * 0.25f;
		}
	}
}

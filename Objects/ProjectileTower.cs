//-----------------------------------------------------------------------------
// ProjectileTower is the orange tower that shoots Projectiles.
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

namespace defense.Objects
{
	public class ProjectileTower : Tower
	{
		public ProjectileTower(Game game) :
			base(game)
		{
			m_TextureName = "Buildings/Tower_Projectile";
			m_OffName = "Buildings/Tower_Projectile_Off";
			m_Type = eTowerType.Projectile;
			m_DisplayKey = "build_projectiletower";
			// Set TowerData to a level 1 projectile tower
			m_TowerData = Balance.ProjectileTower;
			m_MaxLevel = Balance.ProjectileMaxLevels;
		}

		public override void Update(float fDeltaTime)
		{
			base.Update(fDeltaTime);

			// If we can fire, find if there is an enemy in range
			if (IsRunning && m_bCanFire)
			{
				Enemy target = GameState.Get().GetClosestEnemyInRange(Position, m_TowerData[m_Level - 1].Range);

				if (target != null)
				{
					// Create the new projectile
					Projectile p = new Projectile(m_Game, target, m_Level);
					p.Position = Position;
					// Spawn at a position slightly higher (since it's a tower)
					p.IncrementY(0.25f);
					GameState.Get().SpawnGameObject(p);
					// Play the sound
					SoundManager.Get().PlaySoundCue("Shoot");

					// Disable firing until this tower recharges
					m_bCanFire = false;
					m_Timer.AddTimer("AllowFiring", m_TowerData[m_Level - 1].ReloadTime, AllowFiring, false);
				}
			}
		}
	}
}

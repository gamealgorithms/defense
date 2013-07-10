//-----------------------------------------------------------------------------
// SlowTower snares enemies.
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
	public class SlowTower : Tower
	{
		bool m_bCanPlaySound = true;

		public SlowTower(Game game) :
			base(game)
		{
			m_TextureName = "Buildings/Tower_Slow";
			m_OffName = "Buildings/Tower_Slow_Off";
			m_Type = eTowerType.Slow;
			m_DisplayKey = "build_slowtower";
			// Set TowerData to a level 1 slow tower
			m_TowerData = Balance.SlowTower;
			m_MaxLevel = Balance.SlowMaxLevels;
		}

		public override void Update(float fDeltaTime)
		{
			base.Update(fDeltaTime);

			// If we can fire, pulse the snare on any enemies within range
			if (IsRunning && m_bCanFire)
			{
				int num = GameState.Get().ExecuteOnEnemiesInRange(SnareEnemy,
					Position, m_TowerData[m_Level - 1].Range);
				if (num != 0 && m_bCanPlaySound)
				{
					SoundManager.Get().PlaySoundCue("Snared");
					// So we don't spam the sound
					m_bCanPlaySound = false;
					m_Timer.AddTimer("AllowSound", 2.0f, AllowSound, false);
				}

				m_bCanFire = false;

				m_Timer.AddTimer("AllowFiring", m_TowerData[m_Level - 1].ReloadTime, AllowFiring, false);
			}
		}

		protected void SnareEnemy(Enemy e)
		{
			// We want to snare for a little bit longer than the reload time
			e.Snare(m_TowerData[m_Level - 1].Damage, m_TowerData[m_Level - 1].ReloadTime + 0.1f);
		}

		void AllowSound()
		{
			m_bCanPlaySound = true;
		}
	}
}

//-----------------------------------------------------------------------------
// The ProjectileTower spawns the sphere Projectiles contained herein.
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
	public class Projectile : GameObject
	{
		// Level of tower this projectile was shot from
		int m_Level;

		// Enemy the projectile is traveling towards
		Objects.Enemy m_Target;

		// Amount of time we've been alive
		float m_fLiveTime = 0.0f;

		public Projectile(Game game, Objects.Enemy target, int level)
			: base(game)
		{
			m_ModelName = "Projectiles/Sphere";
			Scale = 0.05f;
			m_Target = target;
			m_Level = level;
		}
		
		public override void Update(float fDeltaTime)
		{
			base.Update(fDeltaTime);

			m_fLiveTime += fDeltaTime;

			// If the target is no longer enabled, that means it hit the base,
			// which means this projectile should just go away.
			if (!m_Target.Enabled)
			{
				GameState.Get().RemoveGameObject(this, true);
			}

			// Move towards target
			Position = Vector3.Lerp(Position, m_Target.Position, 
				m_fLiveTime / Balance.ProjectileTower[m_Level - 1].ReloadTime);

			// Once we reach the target, deal damage
			// Don't check against bounding sphere because otherwise really big enemies will quickly intersect
			if (Vector3.Distance(Position, m_Target.Position) < 0.05f)
			{
				GameState.Get().RemoveGameObject(this, true);
				m_Target.TakeDamage(m_Level * Balance.ProjectileTower[m_Level - 1].Damage);
			}
		}
	}
}

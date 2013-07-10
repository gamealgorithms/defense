//-----------------------------------------------------------------------------
// The Enemy is the triangular prism that attacks the base.
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

namespace defense.Objects
{
	public class Enemy : GameObject
	{
		// Current health of enemy
		int m_Health;
		public int Health
		{
			get { return m_Health; }
		}

		// Level of enemy (higher levels are nastier)
		int m_Level;
		public int Level
		{
			get { return m_Level; }
		}
		
		// The path the enemy is traveling on
		public PathNode m_Path;
		
		// Variables to help moving between two nodes
		PathNode m_LerpFrom;
		PathNode m_LerpTo;
		float m_fMoveTime;
		Vector3 m_StartPos;
		Vector3 m_EndPos;

		// Tracks the tile the Enemy is currently on
		Tile m_CurrentTile;
		public Tile CurrentTile
		{
			get { return m_CurrentTile; }
		}

		// Snare factor is the percent slowed
		// 100% is full speed, 25% is 1/4th speed
		int SnareFactor { get; set; }

		// Default Enemy Texture
		protected Texture2D m_DefaultTexure;

		// Texture to show when the Enemy is snared
		protected Texture2D m_SnareTexture;

		public Enemy(Game game, int level = 1):
			base(game)
		{
			m_ModelName = "Miner/Miner";

			m_Path = Pathfinder.Get().Path;
			Position = m_Path.tile.Position;
			m_fMoveTime = 0.0f;
			ResetPath();

			m_Level = level;
			m_Health = Balance.Enemies[m_Level - 1].Health;
			Scale = Balance.Enemies[m_Level - 1].Scale;

			SnareFactor = 100;
		}

		public override void Load()
		{
			base.Load();

			m_DefaultTexure = m_Game.Content.Load<Texture2D>("Miner/Miner_Default");
			m_SnareTexture = m_Game.Content.Load<Texture2D>("Miner/Miner_Snared");
		}

		public override void Update(float fDeltaTime)
		{
			// Update the position between the two nodes
			if (m_LerpTo != null)
			{
				float fTotalDistance = Vector3.Distance(m_StartPos, m_EndPos);
				float fTotalTime = fTotalDistance / Balance.Enemies[m_Level - 1].Speed;
				float fSnareAmount = SnareFactor / 100.0f;
				m_fMoveTime += fDeltaTime * fSnareAmount;

				Position = Vector3.Lerp(m_StartPos, m_EndPos, m_fMoveTime / fTotalTime);

				// If we're at the target node, time to move on to the next one
				if (Vector3.Distance(Position, m_EndPos) < 0.05f)
				{
					m_LerpFrom = m_LerpTo;
					m_LerpTo = m_LerpFrom.parent;
					m_StartPos = m_LerpFrom.tile.Position;
					m_EndPos = m_LerpTo.tile.Position;

					// Update the current tile
					m_CurrentTile = m_LerpFrom.tile;

					if (m_LerpTo != null)
					{
						m_fMoveTime = 0.0f;
						SetDirection();
					}
				}
			}

			// If we're at the goal node, deal damage and remove from the world
			if (Vector3.Distance(Position, Pathfinder.Get().GlobalGoalTile.Position) < 0.5f)
			{
				GameState.Get().DamageBase(Balance.Enemies[m_Level - 1].Damage);
				GameState.Get().RemoveEnemy(this);
			}

			base.Update(fDeltaTime);
		}

		// Snare by the following snare factor for X amount of time
		public void Snare(int factor, float time)
		{
			SnareFactor = factor;

			// Remove the timer in case it already is running (this is safe even if it doesn't exist)
			m_Timer.RemoveTimer("SnareTimer");
			m_Timer.AddTimer("SnareTimer", time, ResetSnare, false);
 		}

		// Set the snare back to normal
		public void ResetSnare()
		{
			SnareFactor = 100;
		}

		public void ResetPath(bool bRecalculated = false)
		{
			if (m_Path != null)
			{
				m_LerpFrom = m_Path;
				m_CurrentTile = m_LerpFrom.tile;
				m_LerpTo = m_LerpFrom.parent;

				// If this was recalculated (because something was built),
				// set the start pos to the current one, so it smoothly transitions to the new path.
				if (bRecalculated)
				{
					m_StartPos = Position;
				}
				else
				{
					m_StartPos = m_LerpFrom.tile.Position;
				}

				m_EndPos = m_LerpTo.tile.Position;
				SetDirection();
			}
			else
			{
				// We have no path, so we're just stuck
				m_LerpFrom = null;
				m_LerpTo = null;
			}

			m_fMoveTime = 0.0f;
		}

		// Makes sure the Enemy faces in the correct direction when moving
		protected void SetDirection()
		{
			// Set the quaternion to face along this path
			// FROM LerpFrom TO LerpTo
			Vector3 vDir = m_LerpTo.tile.Position - m_LerpFrom.tile.Position;
			vDir.Normalize();
			float fAngle = (float)Math.Acos(Vector3.Dot(vDir, Vector3.Forward));
			if (Vector3.Dot(Vector3.Cross(vDir, Vector3.Forward), Vector3.Up) > 0.0f)
			{
				Rotation = Quaternion.CreateFromAxisAngle(Vector3.Down, fAngle);
			}
			else
			{
				Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, fAngle);
			}
		}

		// Function called when enemy takes damage
		public void TakeDamage(int amount)
		{
			m_Health -= amount;
			if (m_Health <= 0)
			{
				GameState.Get().RemoveEnemy(this, true);
			}
		}

		public override void Draw(float fDeltaTime)
		{
			Texture2D texture = m_DefaultTexure;
			// If we're snared, replace the texture with the Snare texture
			if (SnareFactor != 100)
			{
				texture = m_SnareTexture;
			}

			foreach (ModelMesh mesh in m_Model.Meshes)
			{
				foreach (BasicEffect effect in mesh.Effects)
				{
					effect.Texture = texture;
				}
			}

			base.Draw(fDeltaTime);
		}
	}
}

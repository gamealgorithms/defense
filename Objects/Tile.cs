//-----------------------------------------------------------------------------
// This is the base Tile that the other two colors (Green/Red) inherit from
// Contains logic for building Towers on said tile.
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
	public class Tile : GameObject
	{
		protected Texture2D m_Texture;
		protected Texture2D m_SelectTexture;

		protected string m_TextureName;
		protected string m_SelectName;

		// Is tile in use (eg. it has a building on it)?
		protected bool m_bInUse = false;
		public bool InUse
		{
			get { return m_bInUse; }
			set { m_bInUse = value; }
		}

		// Is this tile selected by the mouse?
		protected bool m_bSelected = false;
		public bool IsSelected
		{
			get { return m_bSelected; }
			set { m_bSelected = value; }
		}

		// Can you build on this node?
		protected bool m_bBuildable = true;
		public bool Buildable
		{
			get { return m_bBuildable; }
			set { m_bBuildable = value; }
		}

		// The tower that's on this tile, if any
		protected Tower m_Tower;
		public Tower Tower
		{
			get { return m_Tower; }
		}

		public void ClearTower()
		{
			m_Tower = null;
			m_bBuildable = true;
		}

		protected eTileType m_Type = eTileType.Default;
		public eTileType Type
		{
			get { return m_Type; }
		}

		// Links to other tiles (for nav graph)
		public LinkedList<Tile> m_Neighbors = new LinkedList<Tile>();

		public Tile(Game game):
			base(game)
		{
			m_ModelName = "Tiles/Tile";
			m_TextureName = "Tiles/Tile_Default_Diffuse";
			m_SelectName = "Tiles/Tile_Default_Select";
			m_bUseAABB = true;
		}

		public override void Load()
		{
			base.Load();
			
			m_Texture = m_Game.Content.Load<Texture2D>(m_TextureName);
			m_SelectTexture = m_Game.Content.Load<Texture2D>(m_SelectName);
		}

		public override void Update(float fDeltaTime)
		{
			base.Update(fDeltaTime);

			// Update my building
			if (m_Tower != null)
			{
				if (m_bTransformDirty)
				{
					m_Tower.Position = Position;
				}

				m_Tower.Update(fDeltaTime);
			}
		}

		public override void Draw(float fDeltaTime)
		{
			Texture2D texture = m_Texture;
			if (m_bSelected)
			{
				texture = m_SelectTexture;
			}

			// Replace the texture with my texture
			foreach (ModelMesh mesh in m_Model.Meshes)
			{
				foreach (BasicEffect effect in mesh.Effects)
				{
					effect.Texture = texture;
				}
			}

			base.Draw(fDeltaTime);

			// Draw my building
			if (m_Tower != null)
			{
				m_Tower.Draw(fDeltaTime);
			}
		}

		public void AddNeighbor(Tile t)
		{
			m_Neighbors.AddLast(t);
		}

		public void PreviewBuild(Tower tower)
		{
			m_Tower = tower;
			m_Tower.Position = Position;
		}

		public void Build(Tower tower, bool bLoad = true)
		{
			if (bLoad)
			{
				tower.Load();
			}

			m_Tower = tower;
			m_Tower.Position = Position;
			m_Tower.Spawned = true;
			m_Tower.Build(this);

			m_bInUse = true;
			m_bBuildable = false;
		}
	}
}

//-----------------------------------------------------------------------------
// Level constructs the main level and the tile layout.
// Right now the TileData is just an array in the constructor --
// ideally, it would be in an external file.
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

namespace defense
{
	public enum eTileType
	{
		Default = 0,
		Green,
		Red,
		Base,
		EnemySpawn
	}

	public class Level
	{
		Game m_Game;
		LinkedList<Objects.Tile> m_Tiles = new LinkedList<Objects.Tile>();
		
		public Level(Game game)
		{
			m_Game = game;
		}

		public virtual void LoadLevel(string sLevelName)
		{
			m_Tiles.Clear();
			int iWidth = 10;
			int iHeight = 5;

			int[,] TileData =
			{
				  {2,  0,  0,  0,  0,  0,  0,  0,  0,  0},
		        {2,  2,  0,  0,  0,  0,  0,  0,  0,  0},
				  {4,  0,  0,  0,  0,  0,  0,  0,  0,  3},
		        {2,  2,  0,  0,  0,  0,  0,  0,  0,  0},
				  {2,  0,  0,  0,  0,  0,  0,  0,  0,  0},
			};

			// Generate tile array based on width/height
			// (used to setup neighbors)
			Objects.Tile[,] Tiles = new Objects.Tile[iHeight, iWidth];

			float fPerColumn = 1.72f;
			float fPerRow = 1.5f;

			Objects.Tile BaseTile = null;
			Objects.Tile SpawnTile = null;

			int iColCount = 0;
			float fXOffset = -1.0f * (iWidth / 2) * fPerColumn;
			while (iColCount < iWidth)
			{
				int iRowCount = 0;
				float fZOffset = -1.0f * (iHeight / 2) * fPerRow;
				while (iRowCount < iHeight)
				{
					eTileType type = (eTileType)TileData[iRowCount, iColCount];
					float fTileHeight = GlobalDefines.fTileHeight;
					Objects.Tile t;
					if (iRowCount % 2 == 0)
					{
						t = CreateTile(new Vector3(fXOffset, fTileHeight, fZOffset), type);
					}
					else
					{
						t = CreateTile(new Vector3(fXOffset - 0.875f, fTileHeight, fZOffset), type);
					}

					Tiles[iRowCount, iColCount] = t;
					if (type == eTileType.Base)
					{
						BaseTile = t;
					}
					else if (type == eTileType.EnemySpawn)
					{
						SpawnTile = t;
					}
					fZOffset += fPerRow;
					iRowCount++;
				}

				fXOffset += fPerColumn;
				iColCount++;
			}

			// Now loop through the array of tiles to assign neighbors.
			// Since these are hexagons, the row affects which hexagons are neighbors.
			for (int i = 0; i < iHeight; i++)
			{
				for (int j = 0; j < iWidth; j++)
				{
					// East/West are same regardless of row modulus
					// E
					if (j + 1 < iWidth)
					{
						Tiles[i, j].AddNeighbor(Tiles[i, j + 1]);
					}
					// W
					if (j - 1 >= 0)
					{
						Tiles[i, j].AddNeighbor(Tiles[i, j - 1]);
					}

					if (i % 2 == 0)
					{
						// NE
						if ((i - 1 >= 0) && (j + 1 < iWidth))
						{
							Tiles[i, j].AddNeighbor(Tiles[i - 1, j + 1]);
						}
						// SE
						if ((i + 1 < iHeight) && (j + 1 < iWidth))
						{
							Tiles[i, j].AddNeighbor(Tiles[i + 1, j + 1]);
						}
						// SW
						if (i + 1 < iHeight)
						{
							Tiles[i, j].AddNeighbor(Tiles[i + 1, j]);
						}
						// NW
						if (i - 1 >= 0)
						{
							Tiles[i, j].AddNeighbor(Tiles[i - 1, j]);
						}
					}
					else
					{
						// NE
						if (i - 1 >= 0)
						{
							Tiles[i, j].AddNeighbor(Tiles[i - 1, j]);
						}
						// SE
						if (i + 1 < iHeight)
						{
							Tiles[i, j].AddNeighbor(Tiles[i + 1, j]);
						}
						// SW
						if ((i + 1 < iHeight) && (j - 1 >= 0))
						{
							Tiles[i, j].AddNeighbor(Tiles[i + 1, j - 1]);
						}
						// NW
						if ((i - 1 >= 0) && (j - 1 >= 0))
						{
							Tiles[i, j].AddNeighbor(Tiles[i - 1, j - 1]);
						}
					}
				}
			}

			// These values let the camera know what the maximum scroll area should be.
			GameState.Get().Camera.LevelMin = Tiles[0, 0].Position;
			GameState.Get().Camera.LevelMax = Tiles[iHeight - 1, iWidth - 1].Position;
			

			// Create the player's base and initial path for enemies
			if (BaseTile != null && SpawnTile != null)
			{
				BaseTile.Build(new Objects.Base(m_Game));

				Pathfinder.Get().GlobalStartTile = SpawnTile;
				Pathfinder.Get().GlobalGoalTile = BaseTile;
				Pathfinder.Get().ComputeAStar();

				GameState.Get().SetSelected(BaseTile);
			}
		}

		// Helper function that creates the correct type of tile
		Objects.Tile CreateTile(Vector3 vPos, eTileType type = eTileType.Default)
		{
			Objects.Tile Tile = null;
			switch (type)
			{
				case (eTileType.Default):
					Tile = new Objects.Tile(m_Game);
					break;
				case (eTileType.Green):
					Tile = new Objects.TileGreen(m_Game);
					break;
				case (eTileType.Base):
					Tile = new Objects.TileGreen(m_Game);
					break;
				case (eTileType.Red):
					Tile = new Objects.TileRed(m_Game);
					break;
				case (eTileType.EnemySpawn):
					Tile = new Objects.TileRed(m_Game);
					break;
			}

			if (Tile != null)
			{
				Tile.Position = vPos;
				GameState.Get().SpawnGameObject(Tile);
				m_Tiles.AddLast(Tile);
			}

			return Tile;
		}
		
		// This selects a particular tile given a ray from the camera
		public Objects.Tile Intersects(Ray ray)
		{
			// It's possible that multiple tiles intersect, so we need to create a list
			// of potential tiles to select.
			LinkedList<Objects.Tile> possibles = new LinkedList<Objects.Tile>();
			foreach (Objects.Tile t in m_Tiles)
			{
				if (t.AABB.Intersects(ray) != null)
				{
					possibles.AddLast(t);
				}
			}

			// Now select the tile that is the closest to the start position of the ray
			Objects.Tile retval = null;
			float fBestDist = 999999999.0f;
			foreach (Objects.Tile t in possibles)
			{
				float fDist = Vector3.DistanceSquared(t.Position, ray.Position);
				if (fDist < fBestDist)
				{
					retval = t;
					fBestDist = fDist;
				}
			}

			return retval;
		}
	}
}

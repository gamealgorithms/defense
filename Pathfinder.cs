//-----------------------------------------------------------------------------
// Pathfinder class executes the A* algorithm to find the global path
// used by all Enemies. It also can run on an individual Enemy
// if the path was changed while the Enemy was alive.
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
	public class PathNode
	{
		// Reference to parent
		public PathNode parent;

		// Algorithm values
		public float f, g, h;

		// Reference to actual corresponding tile
		public Objects.Tile tile;
	}

	public class Pathfinder : Patterns.Singleton<Pathfinder>
	{
		// The tiles that enemies go from and to
		public Objects.Tile GlobalStartTile { get; set; }
		public Objects.Tile GlobalGoalTile { get; set; }

		// Open and closed sets for A*
		// Key these by the tile so that it makes it easier to find
		// if a tile is already in the set
		Dictionary<Objects.Tile, PathNode> m_OpenSet = new Dictionary<Objects.Tile, PathNode>();
		Dictionary<Objects.Tile, PathNode> m_ClosedSet = new Dictionary<Objects.Tile, PathNode>();

		// Final solved Path
		PathNode GlobalPath;
		public PathNode Path
		{
			get { return GlobalPath; }
		}
		
		// Computes the enemy path using A*
		// Returns true if there is a valid path, false if not.
		// If the optional parameter is NOT set, it sets the global path that all enemies use by default
		// otherwise, this function calculates the path only for the passed in Enemy.
		// This is to allow recalculation of paths for currently active Enemiess.
		public bool ComputeAStar(Objects.Enemy e = null)
		{
			// Clear out the open/closed set from previous paths
			m_OpenSet.Clear();
			m_ClosedSet.Clear();

			Objects.Tile StartTile = GlobalStartTile;
			// If we have a specific Enemy, the starting tile is the one the Enemy is currently at
			if (e != null)
			{
				StartTile = e.CurrentTile;
			}

			// We start at the goal node instead of the start node
			// so that the parent linked list does not need to be reversed once the A* is complete.
			PathNode node = new PathNode();
			node.tile = GlobalGoalTile;
			node.g = 0.0f;
			node.h = 0.0f;
			node.f = 0.0f;

			// Initialize the A* algorithm
			PathNode CurrentNode = node;
			m_ClosedSet.Add(CurrentNode.tile, CurrentNode);

			do 
			{
				// Loop through all the nodes adjacent to the current one
				foreach (Objects.Tile t in CurrentNode.tile.m_Neighbors)
				{
					// Is this tile already in the closed set?
					if (m_ClosedSet.ContainsKey(t))
					{
						// If so, skip.
						continue;
					}
					else if (m_OpenSet.ContainsKey(t))
					{
						// Check if node adoption should occur
						float test_g = CurrentNode.g + Vector3.Distance(t.Position, CurrentNode.tile.Position);

						// Is the new g(x) value lower than the current one?
						node = m_OpenSet[t];
						if (test_g < node.g)
						{
							// If so, change parent
							node.parent = CurrentNode;
							node.g = test_g;
							node.f = node.g + node.h;
						}
					}
					// Tiles with a building are not passable, so should be ignored...
					// UNLESS it's the start node, which can happen if this is a Enemy repositioning
					else if (t.Tower == null || t == StartTile)
					{
						// Add a new PathNode to open set with CurrentNode as parent
						node = new PathNode();
						node.parent = CurrentNode;
						node.tile = t;

						// Calculate h(x) using Euclidean distance
						node.h = Vector3.Distance(GlobalGoalTile.Position, t.Position);

						// Calculate g(x)
						node.g = node.parent.g + Vector3.Distance(t.Position, node.parent.tile.Position);

						// f(x) =  g(x) + h(x)
						node.f = node.g + node.h;

						// Add to open set
						m_OpenSet.Add(t, node);
					}
				}

				// If the open set is empty, we failed to find a path
				if (m_OpenSet.Count == 0)
				{
					break;
				}

				// Search the open set for lowest f(x) cost
				float lowest_f = 10000000000.0f;
				foreach (PathNode p in m_OpenSet.Values)
				{
					if (p.f < lowest_f)
					{
						CurrentNode = p;
						lowest_f = p.f;
					}
				}

				// Move CurrentNode to closed set
				m_OpenSet.Remove(CurrentNode.tile);
				m_ClosedSet.Add(CurrentNode.tile, CurrentNode);
			}
			while (CurrentNode.tile != StartTile);

			// Check to see if we found a path
			if (CurrentNode.tile == StartTile)
			{
				// Set the start of the path to the current node
				// If there is no Enemy, this is the new global path
				if (e == null)
				{
					GlobalPath = CurrentNode;
				}
				else
				{
					e.m_Path = CurrentNode;
					e.ResetPath(true);
				}
				
				return true;
			}
			else
			{
				// If there is no Enemy, this is the new global path
				if (e == null)
				{
					GlobalPath = null;
				}
				else
				{
					e.m_Path = null;
					e.ResetPath(true);
				}
				return false;
			}			
		}

		// Debug function that draws the path
		public void DrawPath(SpriteBatch batch)
		{
			PathNode CurrentNode = GlobalPath;
			while (CurrentNode != null && CurrentNode.tile != GlobalGoalTile)
			{
				// Draw a line from the current tile to the next one
				GraphicsManager.Get().DrawLine3D(batch, 2.0f, Color.White,
					CurrentNode.tile.Position, CurrentNode.parent.tile.Position);

				CurrentNode = CurrentNode.parent;
			}
		}
	}
}

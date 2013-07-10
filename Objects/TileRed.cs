//-----------------------------------------------------------------------------
// TileRed is just a tile that's red.
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
	public class TileRed : Tile
	{
		public TileRed(Game game) :
			base(game)
		{
			m_TextureName = "Tiles/Tile_Red_Diffuse";
			m_SelectName = "Tiles/Tile_Red_Select";
			m_Type = eTileType.Red;
			Buildable = false;
		}
	}
}

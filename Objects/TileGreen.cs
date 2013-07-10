//-----------------------------------------------------------------------------
// TileGreen is just a Tile that's green.
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
	public class TileGreen : Tile
	{
		public TileGreen(Game game) :
			base(game)
		{
			m_TextureName = "Tiles/Tile_Green_Diffuse";
			m_SelectName = "Tiles/Tile_Green_Select";
			m_Type = eTileType.Green;
		}
	}
}

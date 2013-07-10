//-----------------------------------------------------------------------------
// The Base is the placeholder main base you're defending -- no special logic.
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
	public class Base : Tower
	{
		public Base(Game game) :
			base(game)
		{
			m_TextureName = "Buildings/Building_Base";
			m_OffName = "Buildings/Building_Base_Off";
			m_Type = eTowerType.Base;
			m_DisplayKey = "build_base";
			// Set TowerData to a level 1 base
			m_TowerData = Balance.Base;
			m_MaxLevel = Balance.BaseMaxLevels;
		}
	}
}

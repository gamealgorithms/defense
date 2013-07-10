//-----------------------------------------------------------------------------
// Implementation of the Singleton design pattern with templates.
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

namespace defense.Patterns
{
	public class Singleton<T> where T : new()
	{
		private static T m_Instance;
		public static T Get()
		{
			if (m_Instance == null)
			{
				m_Instance = new T();
				return m_Instance;
			}
			else
			{
				return m_Instance;
			}
		}
	}
}

//-----------------------------------------------------------------------------
// Localization Singleton that stores a Dictionary of all the UI strings
// that are visible in the game.
// Right now there's only an en_us.xml file, but you could easily perform
// translations with this system.
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
using System.Xml;

namespace defense
{
	public class Localization : Patterns.Singleton<Localization>
	{
		Dictionary<string, string> m_Translations = new Dictionary<string, string>();

		public Localization()
		{

		}

		public void Start(string sLocFilename)
		{
			m_Translations.Clear();

			using (XmlTextReader reader = new XmlTextReader(sLocFilename))
			{
				while (reader.Read())
				{
					if (reader.Name == "text" && reader.NodeType == XmlNodeType.Element)
					{
						reader.MoveToAttribute("id");
						string key = reader.Value;
						reader.MoveToElement();
						// Grab everything from this element INCLUDING any other markup
						// so it can be parsed later on
						string value = reader.ReadInnerXml();
						m_Translations.Add(key, value);
					}
				}
			}
		}

		public string Text(string Key)
		{
			return m_Translations[Key];
		}
	}
}

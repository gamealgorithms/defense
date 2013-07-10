//-----------------------------------------------------------------------------
// SoundManager maintains a list of cues and their corresponding files.
// This is a very bare bones way to play sound files.
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
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace defense
{
	public class SoundManager : Patterns.Singleton<SoundManager>
	{
		Dictionary<string, SoundEffect> m_Sounds = new Dictionary<string, SoundEffect>();

		public SoundManager()
		{

		}

		// Load the SFX
		public void LoadContent(ContentManager Content)
		{
			m_Sounds.Add("Shoot", Content.Load<SoundEffect>("Sounds/Shoot"));
			m_Sounds.Add("MenuClick", Content.Load<SoundEffect>("Sounds/MenuClick"));
			m_Sounds.Add("Build", Content.Load<SoundEffect>("Sounds/Build"));
			m_Sounds.Add("GameOver", Content.Load<SoundEffect>("Sounds/GameOver"));
			m_Sounds.Add("Victory", Content.Load<SoundEffect>("Sounds/Victory"));
			m_Sounds.Add("Error", Content.Load<SoundEffect>("Sounds/Error"));
			m_Sounds.Add("Snared", Content.Load<SoundEffect>("Sounds/Snared"));
			m_Sounds.Add("Alarm", Content.Load<SoundEffect>("Sounds/Alarm"));
		}

		public void PlaySoundCue(string cue)
		{
			m_Sounds[cue].Play();
		}
	}
}

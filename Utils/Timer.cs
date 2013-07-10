//-----------------------------------------------------------------------------
// Timer class allows registration of functions to fire after X seconds.
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

namespace defense.Utils
{
	class TimerInstance
	{
		public Action OnTimer;
		public bool bLooping;
		public bool bRemove;
		public float fTotalTime;
		public float fRemainingTime;
		public int iTriggerCount;
	}

	public class Timer
	{
		#region Fields

		// Timers which are currently active
		private SortedList<string, TimerInstance> m_Timers = new SortedList<string, TimerInstance>();

		// When a timer is added, it's placed in the pending list first.
		// The pending timers are then moved to the actual list at the start of update.
		// This is to prevent an issue where the Action of one Timer adds another.
		private SortedList<string, TimerInstance> m_Pending = new SortedList<string, TimerInstance>();

		#endregion

		#region Methods

		/// <summary>
		/// Update is called every frame by the owner of this timer class.
		/// It's responsible for updating every currently registered timer.
		/// If any timer has expired, it is triggered, and based on looping or not 
		/// it may either be removed or restarted
		/// Additionally, iTriggerCount for a timer should be incremented every time it triggers.
		/// </summary>
		/// <param name="gameTime">Only ElasedGameTime is used to update all registered timers</param>
		public void Update(float fDeltaTime)
		{
			// Move any pending timers into the active timer list
			foreach (KeyValuePair<string, TimerInstance> k in m_Pending)
			{
				m_Timers.Add(k.Key, k.Value);
			}
			m_Pending.Clear();

			foreach (TimerInstance t in m_Timers.Values)
			{
				t.fRemainingTime -= fDeltaTime;
				if (t.fRemainingTime < 0.0f && !t.bRemove)
				{
					t.OnTimer();
					if (t.bLooping)
					{
						t.fRemainingTime = t.fTotalTime;
					}
					else
					{
						t.bRemove = true;
					}
				}
			}

			for (int i = m_Timers.Count - 1; i >= 0; i--)
			{
				if (m_Timers.Values[i].bRemove)
				{
					m_Timers.RemoveAt(i);
				}
			}
		}

		/// <summary>
		/// AddTimer will add a new timer provided a timer of the same name does not already exist.
		/// </summary>
		/// <param name="sTimerName">Name of timer to be added</param>
		/// <param name="fTimerDuration">Duration timer should last, in seconds</param>
		/// <param name="Callback">Call back delegate which should be called when the timer expires</param>
		/// <param name="bLooping">Whether the timer should loop infinitely, or should fire once and remove itself</param>
		/// <returns>Returns true if the timer was successfully added, false if it wasn't</returns>
		public bool AddTimer(string sTimerName, float fTimerDuration, Action Callback, bool bLooping)
		{
			if (!m_Timers.ContainsKey(sTimerName))
			{
				TimerInstance t = new TimerInstance();
				t.fRemainingTime = fTimerDuration;
				t.fTotalTime = fTimerDuration;
				t.iTriggerCount = 0;
				t.OnTimer += Callback;
				t.bLooping = bLooping;
				t.bRemove = false;

				// Add to the pending list for now, will get moved to active list later
				// If it's already in the pending list for some reason, remove the existing one
				if (m_Pending.ContainsKey(sTimerName))
				{
					m_Pending.Remove(sTimerName);
				}
				m_Pending.Add(sTimerName, t);

				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// RemoveTimer removes the timer with the specified name
		/// You must support being able to remove one timer from another timer's callback
		/// (But don't worry about removing the same timer from your callback, 'cause that's confusing)
		/// </summary>
		/// <param name="sTimerName">Name of timer to remove</param>
		/// <returns>True if successfully removed, false if not found</returns>
		public bool RemoveTimer(string sTimerName)
		{
			if (m_Timers.ContainsKey(sTimerName))
			{
				m_Timers[sTimerName].bRemove = true;
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// GetTriggerCount gets the number of times the specified timer has been triggered
		/// </summary>
		/// <param name="sTimerName">Name of timer to get value for</param>
		/// <returns>iTriggerCount if found, otherwise -1</returns>
		public int GetTriggerCount(string sTimerName)
		{
			if (m_Timers.ContainsKey(sTimerName))
			{
				return m_Timers[sTimerName].iTriggerCount;
			}
			else
			{
				return -1;
			}
		}

		/// <summary>
		/// GetRemainingTime gets the remaining time on the specified timer
		/// </summary>
		/// <param name="sTimerName">Name of timer to get value for</param>
		/// <returns>fRemainingTime if found, otherwise -1.0f</returns>
		public float GetRemainingTime(string sTimerName)
		{
			if (m_Timers.ContainsKey(sTimerName))
			{
				return m_Timers[sTimerName].fRemainingTime;
			}
			else
			{
				return -1.0f;
			}
		}

		public void RemoveAll()
		{
			m_Timers.Clear();
		}

		public void ResetTimer(string sTimerName)
		{
			if (m_Timers.ContainsKey(sTimerName))
			{
				m_Timers[sTimerName].fRemainingTime = m_Timers[sTimerName].fTotalTime;
			}
		}
		#endregion
	}
}

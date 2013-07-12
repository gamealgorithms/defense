//-----------------------------------------------------------------------------
// The main GameState Singleton. All actions that change the game state,
// as well as any global updates that happen durign gameplay occur in here.
// Because of this, the file is relatively lengthy.
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
	public enum eGameState
	{
		None = 0,
		TitleScreen,
		MainMenu,
		Gameplay,
	}

	public enum eGameSpeed
	{
		Stop = 0,
		Slow,
		Normal,
		Fast,
		Fastest,
		NUM_SPEEDS
	}

	public class GameState : defense.Patterns.Singleton<GameState>
	{
		Game m_Game;
		eGameState m_State;
		public eGameState State
		{
			get { return m_State; }
		}

		eGameState m_NextState;
		Stack<UI.UIScreen> m_UIStack;
		bool m_bPaused = false;
		public bool IsPaused
		{
			get { return m_bPaused; }
			set	{ m_bPaused = value; }
		}

		// Keeps track of all active game objects
		LinkedList<GameObject> m_GameObjects = new LinkedList<GameObject>();

		// Track the enemies separately
		List<Objects.Enemy> m_Enemies = new List<Objects.Enemy>();

		// Camera Information
		Camera m_Camera;
		public Camera Camera
		{
			get { return m_Camera; }
		}

		public Matrix CameraMatrix
		{
			get { return m_Camera.CameraMatrix; }
		}

		Level m_Level;
		public Level Level
		{
			get { return m_Level; }
		}

		// CurrentTile is the one moused over, Selected is actively selected
		Objects.Tile m_CurrentTile;
		Objects.Tile m_SelectedTile;
		public Objects.Tile SelectedTile
		{
			get { return m_SelectedTile; }
		}

		// Track the speed and time factor based on the speed
		eGameSpeed m_Speed = eGameSpeed.Normal;
		public eGameSpeed Speed
		{
			get { return m_Speed; }
			set { m_Speed = value; }
		}

		public float TimeFactor
		{
			get { return Balance.GameSpeeds[(int)m_Speed]; }
		}

		// Timer class for the global GameState
		Utils.Timer m_Timer = new Utils.Timer();

		UI.UIGameplay m_UIGameplay;

		// Money available for purchases
		public int Money { get; set; }
		
		// The base's life total
		public int Life { get; set; }
		
		// Whether a wave is active or not
		bool m_bWaveActive;
		public bool IsWaveActive
		{
			get { return m_bWaveActive; }
		}

		// Current Wave Number
		int m_WaveNumber;
		public int WaveNumber
		{
			get { return m_WaveNumber; }
		}

		// Number of enemies left for this wave
		int m_EnemiesThisWave;

		// Time until next wave
		public float NextWaveTime
		{
			get { return m_Timer.GetRemainingTime("StartWave"); }
		}

		// So we don't spam the alarm cooldown
		bool m_bCanPlayAlarm = true;

		public void Start(Game game)
		{
			m_Game = game;
			m_State = eGameState.None;
			m_UIStack = new Stack<UI.UIScreen>();

			m_Camera = new Camera(m_Game);
		}

		public void SetState(eGameState NewState)
		{
			m_NextState = NewState;
		}

		private void HandleStateChange()
		{
			if (m_NextState == m_State)
				return;

			switch (m_NextState)
			{
				case eGameState.TitleScreen:
					// Going to the title screen so clear everything
					m_UIStack.Clear();
					m_UIGameplay = null;
					m_Timer.RemoveAll();
					m_UIStack.Push(new UI.UITitleScreen(m_Game.Content));
					break;
				case eGameState.MainMenu:
					m_UIStack.Clear();
					m_UIGameplay = null;
					m_Timer.RemoveAll();
					m_UIStack.Push(new UI.UIMainMenu(m_Game.Content));
					ClearGameObjects();
					break;
				case eGameState.Gameplay:
					SetupGameplay();
					break;
			}

			m_State = m_NextState;
		}

		protected void ClearGameObjects()
		{
			// Clear out any and all game objects
			foreach (GameObject o in m_GameObjects)
			{
				RemoveGameObject(o, false);
			}
			m_GameObjects.Clear();
			m_Enemies.Clear();
		}

		public void SetupGameplay()
		{
			ClearGameObjects();
			m_UIStack.Clear();
			m_UIGameplay = new UI.UIGameplay(m_Game.Content);
			m_UIStack.Push(m_UIGameplay);

			m_bPaused = false;
			m_Speed = eGameSpeed.Normal;
			m_Camera.ResetCamera();
			GraphicsManager.Get().ResetProjection();
			m_SelectedTile = null;
			m_CurrentTile = null;
			
			m_Timer.RemoveAll();

			Money = Balance.StartingMoney;
			Life = Balance.StartingLife;
			m_WaveNumber = 0;
			m_bWaveActive = false;
			SpawnWorld();

			// Start the timer for the first wave
			m_Timer.AddTimer("StartWave", Balance.FirstWaveTime, StartWave, false);
			m_bCanPlayAlarm = true;
		}

		public void Update(float fDeltaTime)
		{
			HandleStateChange();

			switch (m_State)
			{
				case eGameState.TitleScreen:
					UpdateTitleScreen(fDeltaTime);
					break;
				case eGameState.MainMenu:
					UpdateMainMenu(fDeltaTime);
					break;
				case eGameState.Gameplay:
					UpdateGameplay(fDeltaTime);
					break;
			}

			foreach (UI.UIScreen u in m_UIStack)
			{
				u.Update(fDeltaTime);
			}
		}

		void UpdateTitleScreen(float fDeltaTime)
		{
			
		}

		void UpdateMainMenu(float fDeltaTime)
		{

		}

		void UpdateGameplay(float fDeltaTime)
		{
			if (!IsPaused)
			{
				m_Camera.Update(fDeltaTime);

				if (GameState.Get().Speed != eGameSpeed.Stop)
				{
					float fAlteredTime = fDeltaTime * TimeFactor;

					// We have to make a temp copy in case the objects list changes
					LinkedList<GameObject> temp = new LinkedList<GameObject>(m_GameObjects);
					foreach (GameObject o in temp)
					{
						if (o.Enabled)
						{
							o.Update(fAlteredTime);
						}
					}

					m_Timer.Update(fAlteredTime);
				}

				// Use mouse picking to select the appropriate tile.
				Ray ray = InputManager.Get().CalculateMouseRay();
				m_CurrentTile = m_Level.Intersects(ray);
			}
		}

		void SpawnWorld()
		{
			m_Level = new Level(m_Game);
			m_Level.LoadLevel("");
		}

		// Called when an Enemy damages the base
		public void DamageBase(int amount)
		{
			Life -= amount;
			if (Life == 0)
			{
				GameOver(false);
			}
			else
			{
				m_UIGameplay.ShowStatusMessage(Localization.Get().Text("ui_msg_base"));
				if (m_bCanPlayAlarm)
				{
					SoundManager.Get().PlaySoundCue("Alarm");
					m_bCanPlayAlarm = false;
					m_Timer.AddTimer("AlarmSound", 5.0f, ResetAlarmSound, false);
				}
			}
		}

		void ResetAlarmSound()
		{
			m_bCanPlayAlarm = true;
		}

		public void SpawnGameObject(GameObject o)
		{
			o.Load();
			m_GameObjects.AddLast(o);
			GraphicsManager.Get().AddGameObject(o);
		}

		public void RemoveGameObject(GameObject o, bool bRemoveFromList = true)
		{
			o.Enabled = false;
			o.Unload();
			GraphicsManager.Get().RemoveGameObject(o);
			if (bRemoveFromList)
			{
				m_GameObjects.Remove(o);
			}
		}

		// This function is called when a new tile is clicked on and selected
		public void SetSelected(Objects.Tile t)
		{
			if (m_SelectedTile != null)
			{
				m_SelectedTile.IsSelected = false;
			}

			m_SelectedTile = t;

			// Notify the UI that a new tile has been selected
			m_UIGameplay.NewSelectedTile(t);

			if (m_SelectedTile != null)
			{
				m_SelectedTile.IsSelected = true;
			}
		}

		public void MouseClick(Point Position)
		{
			if (m_State == eGameState.Gameplay && !IsPaused)
			{
				if (m_CurrentTile != m_SelectedTile)
				{
					SetSelected(m_CurrentTile);
				}
			}
		}

		// I'm the last person to get keyboard input, so don't need to remove
		public void KeyboardInput(SortedList<eBindings, BindInfo> binds)
		{
			if (m_State == eGameState.Gameplay && !IsPaused)
			{
				// Check for panning
				if (binds.ContainsKey(eBindings.Pan_Left))
				{
					m_Camera.AddToPan(Vector3.Left);
				}
				if (binds.ContainsKey(eBindings.Pan_Right))
				{
					m_Camera.AddToPan(Vector3.Right);
				}
				if (binds.ContainsKey(eBindings.Pan_Forward))
				{
					m_Camera.AddToPan(Vector3.Forward);
				}
				if (binds.ContainsKey(eBindings.Pan_Back))
				{
					m_Camera.AddToPan(Vector3.Backward);
				}
			}
		}

		// Try to build the specified tower at the current tile.
		// Returns true if succeeds
		public bool TryBuild(eTowerType type)
		{
			// Check and make sure we can afford this building
			int BuildCost = 0;
			switch (type)
			{
				case (eTowerType.Projectile):
					BuildCost = Balance.ProjectileTower[0].BuildCost;
					break;
				case (eTowerType.Slow):
					BuildCost = Balance.SlowTower[0].BuildCost;
					break;
			}
			if (BuildCost > Money)
			{
				m_UIGameplay.ShowErrorMessage(Localization.Get().Text("ui_err_money"));
				return false;
			}

			// Create the tower
			// We don't use SpawnGameObject here because the Tower is a child of its Tile
			Objects.Tower tower = null;
			switch (type)
			{
				case (eTowerType.Projectile):
					tower = new Objects.ProjectileTower(m_Game);
					break;
				case (eTowerType.Slow):
					tower = new Objects.SlowTower(m_Game);
					break;
			}

			tower.Load();

			// Check whether or not there still is a route for enemies to take
			// before building this!
			m_SelectedTile.PreviewBuild(tower);
			if (!Pathfinder.Get().ComputeAStar())
			{
				m_UIGameplay.ShowErrorMessage(Localization.Get().Text("ui_err_block"));
				
				// Clear out this tower and recompute old path
				m_SelectedTile.ClearTower();

				Pathfinder.Get().ComputeAStar();
				return false;
			}
			else
			{
				// Any active enemies need to compute new paths, in case the old one is blocked
				foreach (Objects.Enemy e in m_Enemies)
				{
					Pathfinder.Get().ComputeAStar(e);
				}

				m_SelectedTile.Build(tower, false);
				m_UIGameplay.FinishBuild();
			}

			return true;
		}

		public UI.UIScreen GetCurrentUI()
		{
			return m_UIStack.Peek();
		}

		public int UICount
		{
			get { return m_UIStack.Count; }
		}

		// Has to be here because only this can access stack!
		public void DrawUI(float fDeltaTime, SpriteBatch batch)
		{
			// We draw in reverse so the items at the TOP of the stack are drawn after those on the bottom
			foreach (UI.UIScreen u in m_UIStack.Reverse())
			{
				u.Draw(fDeltaTime, batch);
			}
		}

		// Pops the current UI
		public void PopUI()
		{
			m_UIStack.Peek().OnExit();
			m_UIStack.Pop();
		}

		public void ShowPauseMenu()
		{
			IsPaused = true;
			m_UIStack.Push(new UI.UIPauseMenu(m_Game.Content));
		}

		public void ShowHowToPlay()
		{
			m_UIStack.Push(new UI.UIHowToPlay(m_Game.Content));
		}

		public void Exit()
		{
			m_Game.Exit();
		}

		public void IncreaseSpeed()
		{
			switch (m_Speed)
			{
				case (eGameSpeed.Stop): m_Speed = eGameSpeed.Slow; break;
				case (eGameSpeed.Slow): m_Speed = eGameSpeed.Normal; break;
				case (eGameSpeed.Normal): m_Speed = eGameSpeed.Fast; break;
				case (eGameSpeed.Fast): m_Speed = eGameSpeed.Fastest; break;
			}
		}

		public void DecreaseSpeed()
		{
			switch (m_Speed)
			{
				case (eGameSpeed.Slow): m_Speed = eGameSpeed.Stop; break;
				case (eGameSpeed.Normal): m_Speed = eGameSpeed.Slow; break;
				case (eGameSpeed.Fast): m_Speed = eGameSpeed.Normal; break;
				case (eGameSpeed.Fastest): m_Speed = eGameSpeed.Fast; break;
			}
		}

		// Find the closest enemy that's within range
		public Objects.Enemy GetClosestEnemyInRange(Vector3 vPosition, float fRange)
		{
			float fClosest = fRange + 1.0f;
			Objects.Enemy retVal = null;

			foreach (Objects.Enemy e in m_Enemies)
			{
				float fDist = Vector3.Distance(vPosition, e.Position);
				if (fDist < fClosest)
				{
					retVal = e;
					fClosest = fDist;
				}
			}

			return retVal;
		}

		// Execute provided function on all enemies within range
		// Returns number that were in range
		public int ExecuteOnEnemiesInRange(Action<Objects.Enemy> myFunction, Vector3 vPosition, float fRange)
		{
			int num = 0;
			foreach (Objects.Enemy e in m_Enemies)
			{
				float fDist = Vector3.Distance(vPosition, e.Position);
				if (fDist < fRange)
				{
					myFunction(e);
					num++;
				}
			}

			return num;
		}

		// Delete the tower on the selected tile
		public void DeleteSelectedTower()
		{
			if (m_SelectedTile != null && m_SelectedTile.Tower != null)
			{
				// Refund the tower
				Money += m_SelectedTile.Tower.GetRefundAmount();

				// Destroy the tower
				RemoveGameObject(m_SelectedTile.Tower);
				m_SelectedTile.ClearTower(true);

				// Notify the UI the tile state has changed
				m_UIGameplay.NewSelectedTile(m_SelectedTile);
			}
		}

		// Tries to upgrade the tower on the selected tile
		// Returns false if unsuccessful
		public bool TryUpgrade()
		{
			// Make sure we have a selected tile (this should never happen)
			if (m_SelectedTile != null && m_SelectedTile.Tower != null)
			{
				Objects.Tower t = m_SelectedTile.Tower;

				// Can't upgrade if it's not active
				if (!t.IsRunning)
				{
					m_UIGameplay.ShowErrorMessage(Localization.Get().Text("ui_err_building"));
					return false;
				}

				// We can't upgrade if we're at the max level already
				if (t.Level == t.MaxLevel)
				{
					m_UIGameplay.ShowErrorMessage(Localization.Get().Text("ui_err_level"));
					return false;
				}

				// Make sure we can afford it
				if (t.GetUpgradeCost() > Money)
				{
					m_UIGameplay.ShowErrorMessage(Localization.Get().Text("ui_err_money"));
					return false;
				}

				// Do the upgrade
				t.Upgrade();
				SoundManager.Get().PlaySoundCue("Build");
				return true;
			}
			
			return false;
		}

		// Start the next wave
		void StartWave()
		{
			m_EnemiesThisWave = Balance.Waves[m_WaveNumber].Count;
			// Start the enemy spawn timer
			m_Timer.AddTimer("SpawnEnemy", Balance.Waves[m_WaveNumber].Interval, SpawnEnemy, true);

			// Increment here because the array is zero-index based
			m_WaveNumber++;
			m_bWaveActive = true;
		}

		void SpawnEnemy()
		{
			Objects.Enemy e = new Objects.Enemy(m_Game, Balance.Waves[m_WaveNumber - 1].Level);
			SpawnGameObject(e);
			m_Enemies.Add(e);

			m_EnemiesThisWave--;
			// Stop spawning once we hit the limit
			if (m_EnemiesThisWave == 0)
			{
				m_Timer.RemoveTimer("SpawnEnemy");
			}
		}

		public void RemoveEnemy(Objects.Enemy e, bool bGiveMoney = false)
		{
			RemoveGameObject(e, true);
			m_Enemies.Remove(e);

			// Give them the well deserved money
			if (bGiveMoney)
			{
				Money += Balance.Enemies[e.Level - 1].Money;
			}

			// If there are no enemies left, the wave is over
			if (m_EnemiesThisWave == 0 && m_Enemies.Count == 0)
			{
				m_bWaveActive = false;
				// If we just finished the last wave, VICTORY!!
				if (m_WaveNumber == Balance.TotalWaves)
				{
					GameOver(true);
				}
				else
				{
					// If the next wave will have stronger enemies, give a warning
					if (Balance.Waves[m_WaveNumber].Level == Balance.MaxEnemyLevel)
					{
						m_UIGameplay.ShowStatusMessage(Localization.Get().Text("ui_msg_boss"), 2.5f);
						SoundManager.Get().PlaySoundCue("Alarm");
					}
					else if (Balance.Waves[m_WaveNumber].Level > Balance.Waves[m_WaveNumber - 1].Level)
					{
						m_UIGameplay.ShowStatusMessage(Localization.Get().Text("ui_msg_enemiesstronger"), 2.5f);
						SoundManager.Get().PlaySoundCue("Alarm");
					}
					else
					{
						m_UIGameplay.ShowStatusMessage(Localization.Get().Text("ui_msg_waveover"), 2.5f);
						SoundManager.Get().PlaySoundCue("Victory");
					}
					m_Timer.AddTimer("StartWave", Balance.Waves[m_WaveNumber - 1].NextWaveTime, StartWave, false);
				}
			}
		}

		void GameOver(bool victorious)
		{
			IsPaused = true;
			m_UIGameplay.ClearStatusMessage();
			m_UIStack.Push(new UI.UIGameOver(m_Game.Content, victorious));
		}
	}
}

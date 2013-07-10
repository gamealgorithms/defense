//-----------------------------------------------------------------------------
// Balance contains all parameters to control the balance of the game.
// Ideally this would be in an external file, but for ease it's here.
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

namespace defense
{
	public enum eCostType
	{
		Money = 0,
		Food,
		Water,
		Power,
		Population
	}

	// Helper class to store stats for towers at different levels
	public class TowerData
	{
		// Money cost and amount of time to build (or upgrade) this tower
		public int BuildCost;
		public float BuildTime;

		// Range and rate of fire
		public float Range;
		public float ReloadTime;

		// Amount of damage the tower does per hit
		public int Damage;
	}

	// Helper class to store stats for enemies at different levels
	public class EnemyData
	{
		// Damage done to base
		public int Damage;
		// Speed it can move
		public float Speed;
		// Money given when it dies
		public int Money;
		// Health
		public int Health;
		// How big they scale up
		public float Scale;
	}

	// Helper class to store all the data regarding enemy waves
	public class WaveData
	{
		// Level of the enemy
		public int Level;
		// How often enemies spawn (every x seconds)
		public float Interval;
		// Total number of enemies in this wave
		public int Count;
		// Time after this wave is over before the next wave
		public float NextWaveTime;
	}

	public static class Balance
	{
		// Speed Factors
		public static float[] GameSpeeds =
		{
			0.0f, // Paused
			0.5f, // Slow
			1.0f, // Normal
			2.0f, // Fast
			4.0f, // Fastest
		};

		// Distance between the center of two tiles
		public static float fTileDistance = 1.72f;
		
		// Globals
		public static int StartingMoney = 30;
		public static int StartingLife = 20;
		
		// Percent of money refunded when a tower is destroyed
		public static int RefundPercent = 50;
				
		// Projectile Tower Parameters
		public static int ProjectileMaxLevels = 3;
		public static TowerData[] ProjectileTower =
		{
			// For ProjectileTowers, Damage is the amount done per projectile
			// Level 1
			new TowerData {BuildCost = 25, BuildTime = 3.0f, Range = 1.0f, ReloadTime = 1.0f, Damage = 10},
			// Level 2
			new TowerData {BuildCost = 250, BuildTime = 3.0f, Range = 1.0f, ReloadTime = 1.0f, Damage = 20},
			// Level 3
			new TowerData {BuildCost = 2500, BuildTime = 3.0f, Range = 1.0f, ReloadTime = 1.0f, Damage = 40},
		};

		// Slow Tower Parameters
		public static int SlowMaxLevels = 3;
		public static TowerData[] SlowTower =
		{
			// For SlowTowers, Damage is the percent of snare (100% is full speed, 25% is 1/4th speed)
			// Level 1
			new TowerData {BuildCost = 50, BuildTime = 5.0f, Range = 2.0f, ReloadTime = 0.5f, Damage = 75},
			// Level 2
			new TowerData {BuildCost = 500, BuildTime = 5.0f, Range = 2.0f, ReloadTime = 0.5f, Damage = 50},
			// Level 3
			new TowerData {BuildCost = 5000, BuildTime = 5.0f, Range = 2.0f, ReloadTime = 0.5f, Damage = 25},
		};

		// Main Base Parameters
		public static int BaseMaxLevels = 1;
		public static TowerData[] Base =
		{
			// Level 1
			new TowerData {BuildCost = 0, BuildTime = 0.0f, Range = 0.0f, ReloadTime = 0.0f, Damage = 0},
		};

		// Enemies
		public static int MaxEnemyLevel = 4;
		public static EnemyData[] Enemies =
		{
			// Level 1
			new EnemyData { Damage = 1, Speed = 1.0f, Money = 5, Health = 20, Scale = 0.75f},
			// Level 2
			new EnemyData { Damage = 1, Speed = 1.0f, Money = 10, Health = 40, Scale = 1.0f},
			// Level 3
			new EnemyData { Damage = 2, Speed = 1.0f, Money = 20, Health = 80, Scale = 1.25f},
			// Level 4
			new EnemyData { Damage = 20, Speed = 1.0f, Money = 5000, Health = 1250, Scale = 2.0f},
		};

		// Waves
		public static int TotalWaves = 16;
		// Number of seconds after loading that the first wave starts
		public static float FirstWaveTime = 5.0f;

		public static WaveData[] Waves =
		{
			// Wave 1
			new WaveData { Level = 1, Count = 5, Interval = 2.0f, NextWaveTime = 5.0f},
			// Wave 2
			new WaveData { Level = 1, Count = 10, Interval = 1.5f, NextWaveTime = 5.0f},
			// Wave 3
			new WaveData { Level = 1, Count = 15, Interval = 1.0f, NextWaveTime = 5.0f},
			// Wave 4
			new WaveData { Level = 1, Count = 20, Interval = 0.5f, NextWaveTime = 5.0f},
			// Wave 5
			new WaveData { Level = 1, Count = 20, Interval = 0.25f, NextWaveTime = 10.0f},

			// Wave 6
			new WaveData { Level = 2, Count = 10, Interval = 1.5f, NextWaveTime = 5.0f},
			// Wave 7
			new WaveData { Level = 2, Count = 15, Interval = 1.0f, NextWaveTime = 5.0f},
			// Wave 8
			new WaveData { Level = 2, Count = 20, Interval = 0.5f, NextWaveTime = 5.0f},
			// Wave 9
			new WaveData { Level = 2, Count = 30, Interval = 0.25f, NextWaveTime = 5.0f},
			// Wave 10
			new WaveData { Level = 2, Count = 30, Interval = 0.15f, NextWaveTime = 10.0f},

			// Wave 11
			new WaveData { Level = 3, Count = 15, Interval = 1.0f, NextWaveTime = 5.0f},
			// Wave 12
			new WaveData { Level = 3, Count = 20, Interval = 0.5f, NextWaveTime = 5.0f},
			// Wave 13
			new WaveData { Level = 3, Count = 25, Interval = 0.25f, NextWaveTime = 5.0f},
			// Wave 14
			new WaveData { Level = 3, Count = 30, Interval = 0.15f, NextWaveTime = 5.0f},
			// Wave 15
			new WaveData { Level = 3, Count = 35, Interval = 0.1f, NextWaveTime = 10.0f},

			// Boss Wave
			new WaveData { Level = 4, Count = 1, Interval = 1.0f, NextWaveTime = 10.0f},
		};
	}
}

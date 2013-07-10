//-----------------------------------------------------------------------------
// These defines don't affect the balance of the game, but change things like
// the graphics parameters and camera speed.
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
	public static class GlobalDefines
	{
		public static int iMouseCursorSize = 32;
		public static float fMouseDefaultSpeed = 1.2f;
		public static float fCameraSpeed = 10.0f;
		public static float fCameraScroll = 15.0f;
		public static float fCameraZoom = 16.0f;
		public static float fCameraMinZoom = 8.0f;
		public static float fCameraMaxZoom = 20.0f;
		public static float fCameraZoomSpeed = 0.0166f;
		public static float fCameraAnimSpeed = 7.5f;

		public static bool bVSync = false;
		public static bool bFullScreen = true;

		public static float fTitleFadeTime = 0.0f;

		// Bloom effect
		public static bool bBloomEnabled = true;

		public static float fBloomThreshold = 0.25f;
		public static float fBlurAmount = 4;
		public static float fBloomIntensity = 2;
		public static float fBaseIntensity = 1;
		public static float fBloomSaturation = 2;
		public static float fBaseSaturation = 1;

		public static float fTileSpeed = 1.0f;
		public static float fMaxTileYOffset = 0.15f;
		public static float fTileHeight = 0.15f;

		public static string DefaultLocFile = "Content/Localization/en_us.xml";

		public static float fToolTipMaxWidth = 300.0f;
	}
}

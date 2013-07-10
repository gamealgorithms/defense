//-----------------------------------------------------------------------------
// Camera Singleton that allows simple panning around the screen.
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

namespace defense
{
	public enum eCameraState
	{
		Free,
		Animating,
	}

	public class Camera
	{
		Game m_Game;
		Vector3 m_vEye = new Vector3(0, 10, 10);
		Vector3 m_vTarget = Vector3.Zero;
		Vector3 m_vCurrPan = Vector3.Zero;
		Vector3 m_vTotalPan = Vector3.Zero;
		eCameraState m_State = eCameraState.Free;
		// Borders of level to determine if we are panning too far
		Vector3 m_vLevelMin;
		public Vector3 LevelMin
		{
			get { return m_vLevelMin; }
			set { m_vLevelMin = value; }
		}

		Vector3 m_vLevelMax;
		public Vector3 LevelMax
		{
			get { return m_vLevelMax; }
			set { m_vLevelMax = value; }
		}

		// For animating camera pans
		Vector3 m_vOldEye;
		Vector3 m_vOldTarget;
		Vector3 m_vNewEye;
		Vector3 m_vNewTarget;
		float m_fLerp;
		float m_fLerpTime;
		
		public void AddToPan(Vector3 v)
		{
			m_vCurrPan += v;
		}

		Matrix m_Camera;
		public Matrix CameraMatrix
		{
			get { return m_Camera; }
		}

		public Camera(Game game)
		{
			m_Game = game;
			ComputeMatrix();
		}

		public void Update(float fDeltaTime)
		{
			if (m_State == eCameraState.Free)
			{
				m_vCurrPan *= GlobalDefines.fCameraSpeed * fDeltaTime;
				m_vTotalPan += m_vCurrPan;
				m_vTotalPan.X = MathHelper.Clamp(m_vTotalPan.X, m_vLevelMin.X, m_vLevelMax.X);
				m_vTotalPan.Z = MathHelper.Clamp(m_vTotalPan.Z, m_vLevelMin.Z, m_vLevelMax.Z);
			}
			else
			{
				if (m_fLerp < 1.0f)
				{
					m_fLerp += fDeltaTime / m_fLerpTime;
					m_vTarget = Vector3.Lerp(m_vOldTarget, m_vNewTarget, m_fLerp);
					m_vEye = Vector3.Lerp(m_vOldEye, m_vNewEye, m_fLerp);
				}
				else
				{
					m_State = eCameraState.Free;
				}
			}

			ComputeMatrix();
			m_vCurrPan = Vector3.Zero;
		}

		void ComputeMatrix()
		{
			Vector3 vEye = m_vEye + m_vTotalPan;
			Vector3 vTarget = m_vTarget + m_vTotalPan;
			Vector3 vUp = Vector3.Cross(Vector3.Zero - vEye, Vector3.Left);
			m_Camera = Matrix.CreateLookAt(vEye, vTarget, vUp);
		}

		public void SetTargetImmediate(Vector3 vTarget)
		{
			Vector3 vDiff = vTarget - m_vTarget;
			m_vEye += vDiff;
			m_vTarget = vTarget;
			m_vTotalPan = Vector3.Zero;
		}

		public void SetTargetSmooth(Vector3 vTarget)
		{
			m_vOldEye = m_vEye;
			m_vOldTarget = m_vTarget;
			
			Vector3 vDiff = vTarget - m_vTarget;
			m_vNewEye = m_vEye + vDiff;
			m_vNewTarget = vTarget;
			
			m_vTotalPan = Vector3.Zero;
			m_fLerp = 0.0f;
			m_fLerpTime = vDiff.Length() / GlobalDefines.fCameraAnimSpeed;

			m_State = eCameraState.Animating;
		}

		public void ResetCamera()
		{
			m_vTotalPan = Vector3.Zero;
			m_vEye = new Vector3(0, 10, 10);
			m_vTarget = Vector3.Zero;
			m_State = eCameraState.Free;
		}
	}
}

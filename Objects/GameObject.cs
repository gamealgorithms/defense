//-----------------------------------------------------------------------------
// Base GameObject class that every other class in the Objects namespace
// inherits from. It's both drawable and updatable.
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
	public class GameObject
	{
		protected Game m_Game;
		public Model m_Model;
		protected Matrix[] m_ModelBones;
		protected string m_ModelName;

		protected Matrix m_WorldTransform = Matrix.Identity;
		protected bool m_bTransformDirty = false;

		protected eDrawOrder m_DrawOrder = eDrawOrder.Default;
		public eDrawOrder DrawOrder
		{
			get { return m_DrawOrder; }
		}

		// Anything that's timer logic is assumed to be affected by time factor
		protected Utils.Timer m_Timer = new Utils.Timer();

		// Bounding Sphere
		protected BoundingSphere m_ModelSphere;
		protected BoundingSphere m_WorldSphere;
		public BoundingSphere Bounds
		{
			get { return m_WorldSphere; }
		}

		// If you want to use AABBs instead, set below to true
		protected bool m_bUseAABB = false;
		// WARNING!!!
		// AABBs assume you aren't doing any rotations
		protected BoundingBox m_ModelAABB;
		protected BoundingBox m_WorldAABB;
		public BoundingBox AABB
		{
			get { return m_WorldAABB; }
		}

		Quaternion m_Rot = Quaternion.Identity;
		public Quaternion Rotation
		{
			get { return m_Rot; }
			set { m_Rot = value; m_bTransformDirty = true; }
		}

		Vector3 m_vPos = Vector3.Zero;
		public Vector3 Position
		{
			get { return m_vPos; }
			set { m_vPos = value; m_bTransformDirty = true; }
		}

		public void IncrementY(float f)
		{
			m_vPos.Y += f;
			m_bTransformDirty = true;
		}

		float m_fScale = 1.0f;
		public float Scale
		{
			get { return m_fScale; }
			set { m_fScale = value; m_bTransformDirty = true; }
		}

		public void RebuildWorldTransform()
		{
			m_bTransformDirty = false;
			m_WorldTransform = Matrix.CreateScale(m_fScale) *
				Matrix.CreateFromQuaternion(m_Rot) * Matrix.CreateTranslation(m_vPos);

			// Update world space AABB and bounding spheres
			m_WorldSphere = m_ModelSphere;
			m_WorldSphere.Center = Position;
			m_WorldSphere.Radius *= Scale;

			if (m_bUseAABB)
			{
				m_WorldAABB = m_ModelAABB;
				m_WorldAABB.Max += Position;
				m_WorldAABB.Min += Position;
			}
		}

		public bool m_bEnabled = true;
		public bool Enabled
		{
			get { return m_bEnabled; }
			set { m_bEnabled = value; }
		}

		public GameObject(Game game)
		{
			m_Game = game;
		}

		public virtual void Load()
		{
			if (m_ModelName != "")
			{
				m_Model = m_Game.Content.Load<Model>(m_ModelName);
				m_ModelBones = new Matrix[m_Model.Bones.Count];
				m_Model.CopyAbsoluteBoneTransformsTo(m_ModelBones);
			}

			// All models have a bounding sphere
			m_ModelSphere = PhysicsManager.Get().GetBoundingSphere(m_ModelName);
			m_WorldSphere = m_ModelSphere;
			m_WorldSphere.Center = Position;
			m_WorldSphere.Radius *= Scale;

			// Only models with this flag have an AABB
			if (m_bUseAABB)
			{
				m_ModelAABB = PhysicsManager.Get().GetBoundingBox(m_ModelName);
				m_WorldAABB = m_ModelAABB;
				m_WorldAABB.Max += Position;
				m_WorldAABB.Min += Position;
			}
		}

		public virtual void Unload()
		{

		}

		public virtual void Update(float fDeltaTime)
		{
			m_Timer.Update(fDeltaTime);
		}

		public virtual void Draw(float fDeltaTime)
		{
			if (m_bTransformDirty)
			{
				RebuildWorldTransform();
			}

			
			foreach (ModelMesh mesh in m_Model.Meshes)
			{
				foreach (BasicEffect effect in mesh.Effects)
				{
					effect.World = m_ModelBones[mesh.ParentBone.Index] * m_WorldTransform;
					effect.View = GameState.Get().CameraMatrix;
					effect.Projection = GraphicsManager.Get().Projection;
					effect.EnableDefaultLighting();
					effect.AmbientLightColor = new Vector3(1.0f, 1.0f, 1.0f);
					effect.DirectionalLight0.Enabled = false;
					effect.DirectionalLight1.Enabled = false;
					effect.DirectionalLight2.Enabled = false;
					effect.PreferPerPixelLighting = true;
				}
				mesh.Draw();
			}
		}
	}
}

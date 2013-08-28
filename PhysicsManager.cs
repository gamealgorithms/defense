//-----------------------------------------------------------------------------
// The PhysicsManager caches the model space bounding spheres and AABBs
// for all 3D models used in the game.
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

namespace itp380
{
	public class PhysicsManager : Patterns.Singleton<PhysicsManager>
	{
		Game m_Game;
		SortedList<string, BoundingBox> m_AABBList = new SortedList<string, BoundingBox>();
		SortedList<string, BoundingSphere> m_SphereList = new SortedList<string, BoundingSphere>();

		public void Start(Game game)
		{
			m_Game = game;
		}

		// Get the cached AABB for this model, if it exists. Otherwise generate it.
		public BoundingBox GetBoundingBox(string modelName)
		{
			if (m_AABBList.ContainsKey(modelName))
			{
				return m_AABBList[modelName];
			}

			BoundingBox bb = GenerateBoundingBox(modelName);
			m_AABBList[modelName] = bb;
			return bb;
		}

		BoundingBox GenerateBoundingBox(string modelName)
		{
			if (m_Game.Content == null)
			{
				throw new System.InvalidOperationException("No content manager for BB :(");
			}

			Model model = m_Game.Content.Load<Model>(modelName);
			Matrix[] ModelBones = new Matrix[model.Bones.Count];
			model.CopyAbsoluteBoneTransformsTo(ModelBones);

			// Initialize minimum and maximum corners of the bounding box to max and min values
			Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

			// For each mesh of the model
			foreach (ModelMesh mesh in model.Meshes)
			{
				foreach (ModelMeshPart meshPart in mesh.MeshParts)
				{
					// Vertex buffer parameters
					int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
					int vertexBufferSize = meshPart.NumVertices * vertexStride;

					// Get vertex data as float
					float[] vertexData = new float[vertexBufferSize / sizeof(float)];
					meshPart.VertexBuffer.GetData<float>(vertexData);

					// Iterate through each vertex
					for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
					{
						// Transform this by any sub-mesh transform information
						Vector3 transformedPosition =
							Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]),
							ModelBones[mesh.ParentBone.Index]);

						min = Vector3.Min(min, transformedPosition);
						max = Vector3.Max(max, transformedPosition);
					}
				}
			}

			// Create and return bounding box
			return new BoundingBox(min, max);
		}

		// Get the cached BoundingSphere for this model, if it exists. Otherwise generate it.
		public BoundingSphere GetBoundingSphere(string modelName)
		{
			if (m_SphereList.ContainsKey(modelName))
			{
				return m_SphereList[modelName];
			}

			BoundingSphere sphere = GenerateBoundingSphere(modelName);
			m_SphereList[modelName] = sphere;
			return sphere;
		}

		BoundingSphere GenerateBoundingSphere(string modelName)
		{
			BoundingSphere modelBounds = new BoundingSphere();
			Model model = m_Game.Content.Load<Model>(modelName);
			Matrix[] ModelBones = new Matrix[model.Bones.Count];
			model.CopyAbsoluteBoneTransformsTo(ModelBones);

			// Initialize initial radius
			float radius = 0.0f;

			// For each mesh of the model
			foreach (ModelMesh mesh in model.Meshes)
			{
				foreach (ModelMeshPart meshPart in mesh.MeshParts)
				{
					// Vertex buffer parameters
					int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
					int vertexBufferSize = meshPart.NumVertices * vertexStride;

					// Get vertex data as float
					float[] vertexData = new float[vertexBufferSize / sizeof(float)];
					meshPart.VertexBuffer.GetData<float>(vertexData);

					// Iterate through each vertex
					for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
					{
						// Transform this by any sub-mesh transform information
						Vector3 transformedPosition =
							Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]),
							ModelBones[mesh.ParentBone.Index]);

						// Get the distance from center to this point, and see if it's a new radius
						float dist = Vector3.Distance(Vector3.Zero, transformedPosition);
						if (dist > radius)
						{
							radius = dist;
						}
					}
				}
			}

			modelBounds.Radius = radius;
			return modelBounds;
		}
	}
}

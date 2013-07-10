//-----------------------------------------------------------------------------
// Based on BloomComponent.cs, from Microsoft Bloom Post-Process Sample:
// http://xbox.create.msdn.com/en-US/education/catalog/sample/bloom
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//
// Used under the Microsoft Permissive license.
// See LICENSE.txt for full details.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace defense.Graphics
{
	class BloomEffect
	{
		Effect m_BloomExtractEffect;
		Effect m_BloomCombineEffect;
		Effect m_GaussianBlurEffect;

		Game m_Game;
		public BloomEffect(Game game)
		{
			m_Game = game;
		}

		public void LoadContent()
		{

			m_BloomExtractEffect = m_Game.Content.Load<Effect>("Effects/BloomExtract");
			m_BloomCombineEffect = m_Game.Content.Load<Effect>("Effects/BloomCombine");
			m_GaussianBlurEffect = m_Game.Content.Load<Effect>("Effects/GaussianBlur");
		}

		public void ApplyEffect(RenderTarget2D sceneRenderTarget, RenderTarget2D renderTarget1, RenderTarget2D renderTarget2)
		{
			m_Game.GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

			// Pass 1: draw the scene into rendertarget 1, using a
			// shader that extracts only the brightest parts of the image.
			m_BloomExtractEffect.Parameters["BloomThreshold"].SetValue(
				GlobalDefines.fBloomThreshold);

			GraphicsManager.Get().DrawFullscreenQuad(sceneRenderTarget, renderTarget1,
							   m_BloomExtractEffect);

			// Pass 2: draw from rendertarget 1 into rendertarget 2,
			// using a shader to apply a horizontal gaussian blur filter.
			SetBlurEffectParameters(1.0f / (float)renderTarget1.Width, 0);

			GraphicsManager.Get().DrawFullscreenQuad(renderTarget1, renderTarget2,
							   m_GaussianBlurEffect);

			// Pass 3: draw from rendertarget 2 back into rendertarget 1,
			// using a shader to apply a vertical gaussian blur filter.
			SetBlurEffectParameters(0, 1.0f / (float)renderTarget1.Height);

			GraphicsManager.Get().DrawFullscreenQuad(renderTarget2, renderTarget1,
							   m_GaussianBlurEffect);

			// Pass 4: draw both rendertarget 1 and the original scene
			// image back into the main backbuffer, using a shader that
			// combines them to produce the final bloomed result.
			m_Game.GraphicsDevice.SetRenderTarget(null);

			EffectParameterCollection parameters = m_BloomCombineEffect.Parameters;

			parameters["BloomIntensity"].SetValue(GlobalDefines.fBloomIntensity);
			parameters["BaseIntensity"].SetValue(GlobalDefines.fBaseIntensity);
			parameters["BloomSaturation"].SetValue(GlobalDefines.fBloomSaturation);
			parameters["BaseSaturation"].SetValue(GlobalDefines.fBaseSaturation);

			m_Game.GraphicsDevice.Textures[1] = sceneRenderTarget;

			Viewport viewport = m_Game.GraphicsDevice.Viewport;

			GraphicsManager.Get().DrawFullscreenQuad(renderTarget1,
							   viewport.Width, viewport.Height,
							   m_BloomCombineEffect);
		}

		/// <summary>
		/// Computes sample weightings and texture coordinate offsets
		/// for one pass of a separable gaussian blur filter.
		/// </summary>
		void SetBlurEffectParameters(float dx, float dy)
		{
			// Look up the sample weight and offset effect parameters.
			EffectParameter weightsParameter, offsetsParameter;

			weightsParameter = m_GaussianBlurEffect.Parameters["SampleWeights"];
			offsetsParameter = m_GaussianBlurEffect.Parameters["SampleOffsets"];

			// Look up how many samples our gaussian blur effect supports.
			int sampleCount = weightsParameter.Elements.Count;

			// Create temporary arrays for computing our filter settings.
			float[] sampleWeights = new float[sampleCount];
			Vector2[] sampleOffsets = new Vector2[sampleCount];

			// The first sample always has a zero offset.
			sampleWeights[0] = ComputeGaussian(0);
			sampleOffsets[0] = new Vector2(0);

			// Maintain a sum of all the weighting values.
			float totalWeights = sampleWeights[0];

			// Add pairs of additional sample taps, positioned
			// along a line in both directions from the center.
			for (int i = 0; i < sampleCount / 2; i++)
			{
				// Store weights for the positive and negative taps.
				float weight = ComputeGaussian(i + 1);

				sampleWeights[i * 2 + 1] = weight;
				sampleWeights[i * 2 + 2] = weight;

				totalWeights += weight * 2;

				// To get the maximum amount of blurring from a limited number of
				// pixel shader samples, we take advantage of the bilinear filtering
				// hardware inside the texture fetch unit. If we position our texture
				// coordinates exactly halfway between two texels, the filtering unit
				// will average them for us, giving two samples for the price of one.
				// This allows us to step in units of two texels per sample, rather
				// than just one at a time. The 1.5 offset kicks things off by
				// positioning us nicely in between two texels.
				float sampleOffset = i * 2 + 1.5f;

				Vector2 delta = new Vector2(dx, dy) * sampleOffset;

				// Store texture coordinate offsets for the positive and negative taps.
				sampleOffsets[i * 2 + 1] = delta;
				sampleOffsets[i * 2 + 2] = -delta;
			}

			// Normalize the list of sample weightings, so they will always sum to one.
			for (int i = 0; i < sampleWeights.Length; i++)
			{
				sampleWeights[i] /= totalWeights;
			}

			// Tell the effect about our new filter settings.
			weightsParameter.SetValue(sampleWeights);
			offsetsParameter.SetValue(sampleOffsets);
		}


		/// <summary>
		/// Evaluates a single point on the gaussian falloff curve.
		/// Used for setting up the blur filter weightings.
		/// </summary>
		float ComputeGaussian(float n)
		{
			float theta = GlobalDefines.fBlurAmount;

			return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
						   Math.Exp(-(n * n) / (2 * theta * theta)));
		}
	}
}

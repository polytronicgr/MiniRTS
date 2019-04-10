﻿using Microsoft.Xna.Framework.Graphics;
using MiniEngine.Effects;
using MiniEngine.Effects.DeviceStates;
using MiniEngine.Pipeline.Lights.Components;
using MiniEngine.Pipeline.Shadows.Factories;
using MiniEngine.Primitives;
using MiniEngine.Primitives.Cameras;
using MiniEngine.Systems;
using System.Collections.Generic;

namespace MiniEngine.Pipeline.Lights.Systems
{
    public sealed class SunlightSystem : ISystem
    {
        private readonly GraphicsDevice Device;
        private readonly Frustum Frustum;
        private readonly FullScreenTriangle FullScreenTriangle;

        private readonly EntityLinker EntityLinker;
        private readonly SunlightEffect Effect;

        private readonly List<Sunlight> Lights;

        public SunlightSystem(GraphicsDevice device, SunlightEffect effect, EntityLinker entityLinker,
            CascadedShadowMapFactory cascadedShadowMapFactory)
        {
            this.Device = device;
            this.Effect = effect;
            this.EntityLinker = entityLinker;

            this.FullScreenTriangle = new FullScreenTriangle();
            this.Frustum = new Frustum();

            this.Lights = new List<Sunlight>(1);
        }

        public void RenderLights(PerspectiveCamera perspectiveCamera, GBuffer gBuffer)
        {
            this.Lights.Clear();
            this.EntityLinker.GetComponents(this.Lights);

            this.Device.ShadowCastingLightState();

            for (var i = 0; i < this.Lights.Count; i++)
            {
                var light = this.Lights[i];

                var shadowMapCascades = light.ShadowMapCascades;

                // G-Buffer input     
                this.Effect.NormalMap = gBuffer.NormalTarget;
                this.Effect.DepthMap = gBuffer.DepthTarget;

                // Light properties
                this.Effect.SurfaceToLightVector = shadowMapCascades.SurfaceToLightVector;
                this.Effect.LightColor = light.Color;

                // Camera properties for specular reflections, and rebuilding world positions
                this.Effect.CameraPosition = perspectiveCamera.Position;
                this.Effect.InverseViewProjection = perspectiveCamera.InverseViewProjection;

                // Shadow properties
                this.Effect.ShadowMap = shadowMapCascades.DepthMapArray;
                this.Effect.ColorMap = shadowMapCascades.ColorMapArray;
                this.Effect.ShadowMatrix = shadowMapCascades.GlobalShadowMatrix;
                this.Effect.CascadeSplits = shadowMapCascades.CascadeSplits;
                this.Effect.CascadeOffsets = shadowMapCascades.CascadeOffsets;
                this.Effect.CascadeScales = shadowMapCascades.CascadeScales;

                this.Effect.Apply();

                this.FullScreenTriangle.Render(this.Device);
            }
        }
    }
}
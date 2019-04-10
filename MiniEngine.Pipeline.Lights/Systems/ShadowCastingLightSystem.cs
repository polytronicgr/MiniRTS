﻿using Microsoft.Xna.Framework.Graphics;
using MiniEngine.Effects;
using MiniEngine.Effects.DeviceStates;
using MiniEngine.Pipeline.Lights.Components;
using MiniEngine.Pipeline.Shadows.Systems;
using MiniEngine.Primitives;
using MiniEngine.Primitives.Cameras;
using MiniEngine.Systems;
using System.Collections.Generic;

namespace MiniEngine.Pipeline.Lights.Systems
{
    public sealed class ShadowCastingLightSystem : ISystem
    {
        private readonly GraphicsDevice Device;

        private readonly FullScreenTriangle FullScreenTriangle;

        private readonly List<ShadowCastingLight> Lights;
        private readonly ShadowCastingLightEffect Effect;

        private readonly ShadowMapSystem ShadowMapSystem;
        private readonly EntityLinker EntityLinker;

        public ShadowCastingLightSystem(
            GraphicsDevice device,
            EntityLinker entityLinker,
            ShadowCastingLightEffect effect,
            ShadowMapSystem shadowMapSystem)
        {
            this.Device = device;
            this.Effect = effect;
            this.EntityLinker = entityLinker;
            this.ShadowMapSystem = shadowMapSystem;

            this.FullScreenTriangle = new FullScreenTriangle();

            this.Lights = new List<ShadowCastingLight>();
        }

        public void RenderLights(PerspectiveCamera perspectiveCamera, GBuffer gBuffer)
        {
            this.Lights.Clear();
            this.EntityLinker.GetComponents(this.Lights);

            this.Device.ShadowCastingLightState();

            for (var i = 0; i < this.Lights.Count; i++)
            {
                var light = this.Lights[i];

                // G-Buffer input                    
                this.Effect.NormalMap = gBuffer.NormalTarget;
                this.Effect.DepthMap = gBuffer.DepthTarget;

                // Light properties                    
                this.Effect.LightDirection = light.ViewPoint.Forward;
                this.Effect.LightPosition = light.ViewPoint.Position;
                this.Effect.Color = light.Color;

                // Camera properties for specular reflections
                this.Effect.CameraPosition = perspectiveCamera.Position;
                this.Effect.InverseViewProjection = perspectiveCamera.InverseViewProjection;

                // Shadow properties
                this.Effect.ShadowMap = light.ShadowMap.DepthMap;
                this.Effect.ColorMap = light.ShadowMap.ColorMap;
                this.Effect.LightViewProjection = light.ViewPoint.ViewProjection;

                this.Effect.Apply();

                this.FullScreenTriangle.Render(this.Device);
            }
        }
    }
}
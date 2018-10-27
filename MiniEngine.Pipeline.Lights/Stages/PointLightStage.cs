﻿using Microsoft.Xna.Framework.Graphics;
using MiniEngine.Primitives.Cameras;
using MiniEngine.Primitives;
using MiniEngine.Pipeline.Lights.Systems;

namespace MiniEngine.Pipeline.Lights.Stages
{
    public sealed class PointLightStage : ILightingPipelineStage
    {
        private readonly GraphicsDevice Device;
        private readonly PointLightSystem PointLightSystem;

        public PointLightStage(GraphicsDevice device, PointLightSystem pointLightSystem)
        {
            this.Device = device;
            this.PointLightSystem = pointLightSystem;
        }

        public void Execute(PerspectiveCamera camera, GBuffer gBuffer)
        {
            this.Device.SetRenderTarget(gBuffer.LightTarget);
            this.PointLightSystem.Render(camera, gBuffer);
        }
    }
}
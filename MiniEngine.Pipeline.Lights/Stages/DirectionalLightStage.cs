﻿using Microsoft.Xna.Framework.Graphics;
using MiniEngine.Primitives.Cameras;
using MiniEngine.Primitives;
using MiniEngine.Pipeline.Lights.Systems;

namespace MiniEngine.Pipeline.Lights.Stages
{
    public sealed class DirectionalLightStage : ILightingPipelineStage
    {
        private readonly GraphicsDevice Device;
        private readonly DirectionalLightSystem DirectionalLightSystem;

        public DirectionalLightStage(GraphicsDevice device, DirectionalLightSystem directionalLightSystem)
        {
            this.Device = device;
            this.DirectionalLightSystem = directionalLightSystem;
        }

        public void Execute(PerspectiveCamera camera, GBuffer gBuffer)
        {
            this.Device.SetRenderTarget(gBuffer.LightTarget);
            this.DirectionalLightSystem.Render(camera, gBuffer);
        }
    }
}
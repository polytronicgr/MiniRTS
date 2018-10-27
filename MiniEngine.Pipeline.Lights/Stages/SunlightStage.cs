﻿using Microsoft.Xna.Framework.Graphics;
using MiniEngine.Primitives.Cameras;
using MiniEngine.Primitives;
using MiniEngine.Pipeline.Lights.Systems;

namespace MiniEngine.Pipeline.Lights.Stages
{
    public sealed class SunlightStage : ILightingPipelineStage
    {
        private readonly GraphicsDevice Device;
        private readonly SunlightSystem SunlightSystem;

        public SunlightStage(GraphicsDevice device, SunlightSystem sunlightSystem)
        {
            this.Device = device;
            this.SunlightSystem = sunlightSystem;
        }

        public void Execute(PerspectiveCamera camera, GBuffer gBuffer)
        {
            this.Device.SetRenderTarget(gBuffer.LightTarget);
            this.SunlightSystem.RenderLights(camera, gBuffer);
        }
    }
}
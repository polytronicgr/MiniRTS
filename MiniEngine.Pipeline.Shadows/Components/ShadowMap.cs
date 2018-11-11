﻿using Microsoft.Xna.Framework.Graphics;
using MiniEngine.Primitives.Cameras;

namespace MiniEngine.Pipeline.Shadows.Components
{
    public sealed class ShadowMap
    {
        public ShadowMap(GraphicsDevice device, int depthMapResolution, int cascades, params IViewPoint[] viewPoint)
        {
            this.ViewPoints = viewPoint;
            this.Cascades = cascades;
            this.DepthMap = new RenderTarget2D(
                device,
                depthMapResolution,
                depthMapResolution,
                false,
                SurfaceFormat.Single,
                DepthFormat.Depth24,
                0,
                RenderTargetUsage.DiscardContents,
                false,
                cascades);

            this.ColorMap = new RenderTarget2D(
                device,
                depthMapResolution,
                depthMapResolution,
                false,
                SurfaceFormat.Color,
                DepthFormat.None,
                0,
                RenderTargetUsage.DiscardContents,
                false,
                cascades);
        }

        public int Cascades { get; }
        public RenderTarget2D DepthMap { get; }
        public RenderTarget2D ColorMap { get; }
        public IViewPoint[] ViewPoints { get; }
    }
}
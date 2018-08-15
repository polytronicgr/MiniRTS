﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiniEngine.Rendering.Cameras;

namespace MiniEngine.Rendering.Components
{
    public sealed class CascadedShadowMap
    {
        public CascadedShadowMap(GraphicsDevice device, int resolution, int cascades)
        {
            this.ShadowCameras = new IViewPoint[4];
            this.CascadeSplits = new float[4];
            this.CascadeOffsets = new Vector4[4];
            this.CascadeScales = new Vector4[4];
            this.GlobalShadowMatrix = Matrix.Identity;

            this.RenderTarget = new RenderTarget2D(
                device,
                resolution,
                resolution,
                false,
                SurfaceFormat.Single,
                DepthFormat.Depth24,
                0,
                RenderTargetUsage.DiscardContents,
                false,
                cascades);
        }

        public RenderTarget2D RenderTarget { get; }
        public IViewPoint[] ShadowCameras { get; }
        public float[] CascadeSplits { get; }
        public Vector4[] CascadeOffsets { get; }
        public Vector4[] CascadeScales { get; }
        public Matrix GlobalShadowMatrix { get; set; }
    }
}
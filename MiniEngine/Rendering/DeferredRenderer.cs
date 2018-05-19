﻿using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MiniEngine.Rendering.Cameras;
using MiniEngine.Rendering.Primitives;
using MiniEngine.Rendering.Systems;
using MiniEngine.Scenes;

namespace MiniEngine.Rendering
{
    public sealed class DeferredRenderer
    {
        public readonly AmbientLightSystem AmbientLightSystem;
        public readonly ModelSystem ModelSystem;
        public readonly DirectionalLightSystem DirectionalLightSystem;
        public readonly PointLightSystem PointLightSystem;
        public readonly ShadowCastingLightSystem ShadowCastingLightSystem;
        public readonly SunlightSystem SunlightSystem;

        private readonly GraphicsDevice Device;
        private readonly Effect ClearEffect;                
        private readonly Effect CombineEffect;
        private readonly Effect PostProcessEffect;
        private readonly Quad Quad;
        private readonly RenderTarget2D ColorTarget;
        private readonly RenderTarget2D NormalTarget;
        private readonly RenderTarget2D DepthTarget;
        private readonly RenderTarget2D LightTarget;
        private readonly RenderTarget2D CombineTarget;
       
        public DeferredRenderer(GraphicsDevice device, ContentManager content, IScene scene)
        {            
            this.Device = device;
            this.ClearEffect = content.Load<Effect>("Clear");            
            this.CombineEffect = content.Load<Effect>("Combine");
            this.PostProcessEffect = content.Load<Effect>("PostProcess");

            this.Scene = scene;

            this.Quad = new Quad();

            var width = device.PresentationParameters.BackBufferWidth;
            var height = device.PresentationParameters.BackBufferHeight;

            // Do not enable AA as we use FXAA during post processing
            const int aaSamples = 0;

            this.ColorTarget = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24, aaSamples, RenderTargetUsage.DiscardContents);
            this.NormalTarget = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.None, aaSamples, RenderTargetUsage.DiscardContents);
            this.DepthTarget  = new RenderTarget2D(device, width, height, false, SurfaceFormat.Single, DepthFormat.None, aaSamples, RenderTargetUsage.DiscardContents);
            this.LightTarget  = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.None, aaSamples, RenderTargetUsage.DiscardContents);
            this.CombineTarget = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.None, aaSamples, RenderTargetUsage.DiscardContents);

            this.AmbientLightSystem = new AmbientLightSystem();
            this.ModelSystem = new ModelSystem(device);
            this.DirectionalLightSystem = new DirectionalLightSystem(device, content.Load<Effect>("DirectionalLight"));
            this.PointLightSystem = new PointLightSystem(device, content.Load<Effect>("PointLight"), content.Load<Model>("Sphere"));
            this.ShadowCastingLightSystem = new ShadowCastingLightSystem(device, content.Load<Effect>("ShadowMap"), content.Load<Effect>("ShadowCastingLight"), this.ModelSystem);
            this.SunlightSystem = new SunlightSystem(device, content.Load<Effect>("ShadowMap"), content.Load<Effect>("Sunlight"), this.ModelSystem);
        }

        public bool EnableFXAA { get; set; } = true;

        public IScene Scene { get; set; }        

        public RenderTarget2D[] GetIntermediateRenderTargets() => new[]
        {            
            this.ColorTarget,
            this.NormalTarget,
            this.DepthTarget,
            this.LightTarget,
            this.CombineTarget
        };

        public void Render(PerspectiveCamera perspectiveCamera)
        {
            RenderGBuffer(perspectiveCamera);

            RenderLights(perspectiveCamera);            

            Combine();

            PostProcess();
        }   

        private void PostProcess()
        {            
            using (this.Device.PostProcessState())
            {
                // Post process the image
                foreach (var pass in this.PostProcessEffect.Techniques[0].Passes)
                {
                    this.PostProcessEffect.Parameters["ScaleX"].SetValue(1.0f / this.CombineTarget.Width);
                    this.PostProcessEffect.Parameters["ScaleY"].SetValue(1.0f / this.CombineTarget.Height);
                    this.PostProcessEffect.Parameters["ColorMap"].SetValue(this.CombineTarget);
                    this.PostProcessEffect.Parameters["NormalMap"].SetValue(this.NormalTarget);                                        
                    this.PostProcessEffect.Parameters["Strength"].SetValue(this.EnableFXAA? 2.0f : 0.0f);                    
                    pass.Apply();

                    this.Quad.Render(this.Device);
                }
            }            
        }        

        private void Combine()
        {
            this.Device.SetRenderTarget(this.CombineTarget);

            using (this.Device.PostProcessState())
            {
                // Combine everything
                foreach (var pass in this.CombineEffect.Techniques[0].Passes)
                {
                    this.CombineEffect.Parameters["ColorMap"].SetValue(this.ColorTarget);
                    this.CombineEffect.Parameters["LightMap"].SetValue(this.LightTarget);                    

                    pass.Apply();
                    this.Quad.Render(this.Device);
                }
            }

            this.Device.SetRenderTarget(null);
        }

        private void RenderLights(PerspectiveCamera perspectiveCamera)
        {                                    
            this.ShadowCastingLightSystem.RenderShadowMaps(this.Scene);
            this.SunlightSystem.RenderShadowMaps(this.Scene, perspectiveCamera);

            this.Device.SetRenderTarget(this.LightTarget);
            
            this.Device.Clear(this.AmbientLightSystem.ComputeAmbientLightZeroAlpha());            
                                   
            this.DirectionalLightSystem.Render(perspectiveCamera, this.ColorTarget, this.NormalTarget, this.DepthTarget);
            this.PointLightSystem.Render(perspectiveCamera, this.ColorTarget, this.NormalTarget, this.DepthTarget);

            this.ShadowCastingLightSystem.RenderLights(perspectiveCamera, this.ColorTarget, this.NormalTarget, this.DepthTarget);
            this.SunlightSystem.RenderLights(perspectiveCamera, this.ColorTarget, this.NormalTarget, this.DepthTarget);            
            

            this.Device.SetRenderTarget(null);
        }

        private void RenderGBuffer(IViewPoint viewPoint)
        {
            this.Device.SetRenderTargets(this.ColorTarget, this.NormalTarget, this.DepthTarget);

            using (this.Device.PostProcessState())
            {

                foreach (var pass in this.ClearEffect.Techniques[0].Passes)
                {
                    pass.Apply();
                    this.Quad.Render(this.Device);
                }
            }
            
            using (this.Device.GeometryState())
            {
                this.ModelSystem.DrawModels(viewPoint);
                //this.Scene.Draw(viewPoint);
            }

            this.Device.SetRenderTargets(null);
        }
    }
}
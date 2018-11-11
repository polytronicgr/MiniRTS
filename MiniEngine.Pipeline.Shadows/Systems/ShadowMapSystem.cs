﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiniEngine.Primitives.Cameras;
using MiniEngine.Systems;
using MiniEngine.Effects.Techniques;
using MiniEngine.Effects.DeviceStates;
using MiniEngine.Pipeline.Models.Systems;
using MiniEngine.Pipeline.Particles.Systems;
using MiniEngine.Pipeline.Shadows.Components;
using MiniEngine.Telemetry;

namespace MiniEngine.Pipeline.Shadows.Systems
{
    public sealed class ShadowMapSystem : ISystem
    {
        private const string ShadowMapCounter = "shadow_pipeline_shadow_map_counter";
        private const string ShadowMapCascadesCounter = "shadow_pipeline_shadow_map_cascades_counter";
        private const string ShadowMapTotal = "shadow_pipeline_total_render_time";
        private const string ShadowMapStep = "shadow_pipeline_step_render_time";

        private const int DefaultResolution = 1024;

        private readonly GraphicsDevice Device;
        private readonly ModelSystem ModelSystem;
        private readonly ParticleSystem ParticleSystem;
        private readonly Dictionary<Entity, ShadowMap> ShadowMaps;
        private readonly IMeterRegistry MeterRegistry;

        public ShadowMapSystem(            
            GraphicsDevice device,
            ModelSystem modelSystem,
            ParticleSystem particleSystem,
            IMeterRegistry meterRegistry)
        {            
            this.Device = device;
            this.ModelSystem = modelSystem;
            this.ParticleSystem = particleSystem;
            this.MeterRegistry = meterRegistry;


            this.MeterRegistry.CreateGauge(ShadowMapCounter);
            this.MeterRegistry.CreateGauge(ShadowMapCascadesCounter);
            this.MeterRegistry.CreateGauge(ShadowMapTotal);
            this.MeterRegistry.CreateGauge(ShadowMapStep, "step");

            this.ShadowMaps = new Dictionary<Entity, ShadowMap>();
        }

        public bool Contains(Entity entity) => this.ShadowMaps.ContainsKey(entity);

        public string Describe(Entity entity)
        {
            var shadowMap = this.ShadowMaps[entity];
            return $"shadow map, dimensions: {shadowMap.DepthMap.Width}x{shadowMap.DepthMap.Height}";
        }

        public void Remove(Entity entity) => this.ShadowMaps.Remove(entity);

        public void Add(Entity entity, IViewPoint viewPoint, int resolution = DefaultResolution) => this.ShadowMaps.Add(entity, new ShadowMap(this.Device, resolution, 1, viewPoint));

        public void Add(Entity entity, IViewPoint[] viewPoints, int cascades, int resolution = DefaultResolution) => this.ShadowMaps.Add(entity, new ShadowMap(this.Device, resolution, cascades, viewPoints));

        public ShadowMap Get(Entity entity) => this.ShadowMaps[entity];

        public void RenderShadowMaps()
        {
            var cascades = 0;
            this.MeterRegistry.SetGauge(ShadowMapCounter, this.ShadowMaps.Count);
            this.MeterRegistry.StartGauge(ShadowMapTotal);
            foreach (var shadowMap in this.ShadowMaps.Values)
            {
                cascades += shadowMap.Cascades;                
                for (var i = 0; i < shadowMap.Cascades; i++)
                {                                        
                    var modelBatchList = this.ModelSystem.ComputeBatches(shadowMap.ViewPoints[i]);
                    var particleBatchList = this.ParticleSystem.ComputeBatches(shadowMap.ViewPoints[i]);

                    this.MeterRegistry.StartGauge(ShadowMapStep);
                    {
                        // First compute the shadow maps
                        this.Device.SetRenderTarget(shadowMap.DepthMap, i);
                        this.Device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
                        
                        using (this.Device.ShadowMapState())
                        {
                            modelBatchList.OpaqueBatch.Draw(RenderEffectTechniques.ShadowMap);
                        }
                    }
                    this.MeterRegistry.StopGauge(ShadowMapStep, "opaque");


                    this.MeterRegistry.StartGauge(ShadowMapStep);
                    {
                        // Read the depth buffer and render objects that are partially
                        // occluding, like a stained glass window
                        this.Device.SetRenderTarget(shadowMap.ColorMap, i);
                        this.Device.Clear(ClearOptions.Target, Color.White, 1.0f, 0);

                        
                        using (this.Device.AlphaBlendOccluderState())
                        {
                            foreach (var batch in modelBatchList.TransparentBatches)
                            {
                                batch.Draw(RenderEffectTechniques.Textured);
                            }
                        }

                    }
                    this.MeterRegistry.StopGauge(ShadowMapStep, "transparent");

                    this.MeterRegistry.StartGauge(ShadowMapStep);
                    {
                        // Read the depth buffer and render occluding particles
                        using (this.Device.AdditiveBlendOccluderState())
                        {
                            foreach (var batch in particleBatchList.Batches)
                            {
                                batch.Draw(RenderEffectTechniques.GrayScale);
                            }
                        }
                    }
                    this.MeterRegistry.StopGauge(ShadowMapStep, "particles");

                    // TODO: if a particle is behind a stained glass window
                    // it will shadow as if its in front of it. 
                }
            }
            this.MeterRegistry.SetGauge(ShadowMapCascadesCounter, cascades);
            this.MeterRegistry.StopGauge(ShadowMapTotal);
        }
    }
}
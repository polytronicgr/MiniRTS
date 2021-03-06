﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiniEngine.Effects;
using MiniEngine.Effects.DeviceStates;
using MiniEngine.Effects.Techniques;
using MiniEngine.Effects.Wrappers;
using MiniEngine.Pipeline.Debug.Components;
using MiniEngine.Primitives;
using MiniEngine.Primitives.Cameras;
using MiniEngine.Systems.Annotations;
using MiniEngine.Systems.Containers;

namespace MiniEngine.Pipeline.Debug.Systems
{
    public sealed class IconSystem : DebugSystem
    {
        private const float Scale = 0.05f;

        private readonly GraphicsDevice Device;
        private readonly TextureEffect Effect;
        private readonly IconLibrary Library;
        private readonly UnitQuad Quad;

        public IconSystem(GraphicsDevice device, EffectFactory effectFactory, IComponentContainer<DebugInfo> debugInfos, IList<IComponentContainer> containers, IconLibrary library)
            : base(debugInfos, containers)
        {
            this.Device = device;
            this.Effect = effectFactory.Construct<TextureEffect>();
            this.Library = library;
            this.Quad = new UnitQuad(device);
        }

        public void RenderIcons(PerspectiveCamera viewPoint, GBuffer gBuffer)
        {
            this.Effect.World      = Matrix.Identity;
            this.Effect.View       = Matrix.Identity;
            this.Effect.Projection = Matrix.Identity;
            this.Effect.DepthMap   = gBuffer.DepthTarget;            

            this.Device.PostProcessState();

            foreach ((var entity, var component, var info, var property, var attribute) in this.EnumerateAttributes<IconAttribute>())
            {                
                var position = (Vector3)property.GetGetMethod().Invoke(component, null);
                
                if(viewPoint.Frustum.Contains(position) != ContainmentType.Disjoint)
                {
                    var screenPosition = ProjectionMath.WorldToView(position, viewPoint.ViewProjection);

                    this.Effect.World                 = Matrix.CreateScale(new Vector3(Scale / viewPoint.AspectRatio, Scale, Scale)) * Matrix.CreateTranslation(new Vector3(screenPosition, 0));
                    this.Effect.Texture               = this.Library.GetIcon(attribute.Type);
                    this.Effect.WorldPosition         = position;
                    this.Effect.CameraPosition        = viewPoint.Position;
                    this.Effect.InverseViewProjection = viewPoint.InverseViewProjection;
                    this.Effect.VisibleTint           = info.VisibileIconTint;
                    this.Effect.ClippedTint           = info.ClippedIconTint;

                    this.Effect.Apply(TextureEffectTechniques.TexturePointDepthTest);

                    this.Quad.Render();
                }                
            }
        }
    }
}

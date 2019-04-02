﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiniEngine.Systems.Components;

namespace MiniEngine.Pipeline.Particles.Components
{
    public sealed class AveragedEmitter : AEmitter
    {
        public AveragedEmitter(Vector3 position, Texture2D texture, int rows, int columns, float scale)
            : base(position, texture, rows, columns, scale) { }

        public override ComponentDescription Describe()
        {
            var description = new ComponentDescription("Averaged Emitter");
            this.Describe(description);            
            return description;
        }        
}
}
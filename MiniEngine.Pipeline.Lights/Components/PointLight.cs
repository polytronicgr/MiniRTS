﻿using Microsoft.Xna.Framework;
using MiniEngine.Systems.Components;

namespace MiniEngine.Pipeline.Lights.Components
{
    public sealed class PointLight : IComponent
    {
        public PointLight(Vector3 position, Color color, float radius, float intensity)
        {
            this.Position = position;
            this.Color = color;
            this.Radius = radius;
            this.Intensity = intensity;
        }

        public Vector3 Position { get; set; }
        public Color Color { get; set; }
        public float Radius { get; set; }
        public float Intensity { get; set; }

        public override string ToString() => $"point light, position: {this.Position}, color: {this.Color}, radius: {this.Radius}";
    }
}
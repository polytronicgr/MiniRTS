﻿using MiniEngine.Rendering.Cameras;
using MiniEngine.Units;

namespace MiniEngine.Systems
{
    public interface IUpdatableSystem : ISystem
    {
        void Update(PerspectiveCamera perspectiveCamera, Seconds elapsed);
    }
}

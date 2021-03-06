﻿using LightInject;
using MiniEngine.Pipeline.Models.Components;
using MiniEngine.Pipeline.Models.Factories;
using MiniEngine.Pipeline.Models.Systems;
using MiniEngine.Systems.Containers;

namespace MiniEngine.Pipeline.Models
{
    public sealed class ModelsCompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            serviceRegistry.Register<IComponentContainer<OpaqueModel>, ComponentList<OpaqueModel>>();
            serviceRegistry.Register<IComponentContainer<TransparentModel>, ComponentList<TransparentModel>>();

            serviceRegistry.Register<OpaqueModelFactory>();
            serviceRegistry.Register<TransparentModelFactory>();

            serviceRegistry.Register<ModelSystem>();
        }
    }
}

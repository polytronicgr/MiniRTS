﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiniEngine.Effects.Techniques;
using MiniEngine.Effects.Wrappers;
using MiniEngine.Pipeline.Models.Components;
using MiniEngine.Primitives.Cameras;

namespace MiniEngine.Pipeline.Models.Batches
{
    public sealed class ModelRenderBatch
    {
        private static Matrix[] SharedBoneMatrix;
        private readonly RenderEffect Effect;

        private readonly IReadOnlyList<AModel> Models;
        private readonly IViewPoint ViewPoint;
        private readonly TextureCube Skybox;

        private readonly Matrix InverseViewProjection;

        public ModelRenderBatch(IReadOnlyList<AModel> models, IViewPoint viewPoint, TextureCube skybox)
        {
            this.Models = models;
            this.ViewPoint = viewPoint;
            this.Skybox = skybox;
            this.Effect = new RenderEffect();

            var viewProjection = viewPoint.View * viewPoint.Projection;
            this.InverseViewProjection = Matrix.Invert(viewProjection);
        }

        public void Draw(RenderEffectTechniques technique)
        {
            for (var i = 0; i < this.Models.Count; i++)
            {
                var modelPose = this.Models[i];
                this.DrawModel(technique, modelPose.Model, modelPose.WorldMatrix, this.ViewPoint);
            }
        }

        private void DrawModel(RenderEffectTechniques technique, Model model, Matrix world, IViewPoint viewPoint)
        {
            var bones = model.Bones.Count;
            if (SharedBoneMatrix is null || SharedBoneMatrix.Length < bones)
            {
                SharedBoneMatrix = new Matrix[bones];
            }

            model.CopyAbsoluteBoneTransformsTo(SharedBoneMatrix);

            for (var iMesh = 0; iMesh < model.Meshes.Count; iMesh++)
            {
                var mesh = model.Meshes[iMesh];

                for (var iEffect = 0; iEffect < mesh.Effects.Count; iEffect++)
                {
                    var effect = mesh.Effects[iEffect];
                    this.Effect.Wrap(effect);

                    this.Effect.World = SharedBoneMatrix[mesh.ParentBone.Index] * world;
                    this.Effect.View = viewPoint.View;
                    this.Effect.Projection = viewPoint.Projection;
                    this.Effect.InverseViewProjection = InverseViewProjection;
                    this.Effect.Skybox = this.Skybox;
                    this.Effect.CameraPosition = viewPoint.Position;

                    this.Effect.Apply(technique);
                }

                mesh.Draw();
            }
        }
    }
}
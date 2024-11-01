using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TGC.MonoGame.TP
{
    internal class Tree : WorldEntity
    {
        public static Model Model;

        public Tree(Vector3 position, Vector3 scale, float yaw) : base(position, scale, yaw, Model)
        {
            _world = Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateScale(scale) * Matrix.CreateRotationY(yaw) * Matrix.CreateTranslation(position);
            _defaultColors = GetDefaultColors(Model.Meshes.Count);
        }

        public static void LoadContent(ContentManager Content, Effect Effect)
        {
            Model = LoadContent(Content, "tree/tree", Effect);
        }

        protected void Update(GameTime gameTime)
        {
            // ¿¿??
        }

        public override (Vector3 center, Vector3 radius) GetLocalBoundingBox(Model model)
        {
            Vector3 center = new Vector3(-0.09898247f, 7.900349819f, 0.064710855f);
            Vector3 radius = new Vector3(0.542698812f, 7.915688181f, 0.542698812f);
            return (center, radius);
        }

        public override Vector3[] GetDefaultColors(int meshes)
        {
            float rb = (float)(Random.NextDouble() * 0.1f) + 0.05f;
            Vector3 green = new Vector3(rb, (float)(Random.NextDouble() * 0.2f) + 0.33f, rb);

            float gb = (float)(Random.NextDouble() * 0.12f) + 0.05f;
            Vector3 brown = new Vector3((float)(Random.NextDouble() * 0.11f) + 0.25f, gb, gb);

            Vector3[] colors = new Vector3[meshes];
            colors[0] = brown;
            colors[1] = brown;
            colors[2] = brown;
            colors[3] = green;
            colors[4] = green;
            colors[5] = green;
            return colors;
        }

        public override void Draw(Matrix view, Matrix projection, Effect effect)
        {
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);

            Model.Root.Transform = _world;
            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

            for (int i = 0; i < Model.Meshes.Count; i++)
            {
                var relativeTransform = modelMeshesBaseTransforms[Model.Meshes[i].ParentBone.Index];
                effect.Parameters["World"].SetValue(relativeTransform);
                effect.Parameters["DiffuseColor"].SetValue(_defaultColors[i]);
                Model.Meshes[i].Draw();
            }
        }
    }
}

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
            _defaultColor = GetDefaultColor();
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

        public override Vector3 GetDefaultColor()
        {
            float r = (float)(Random.NextDouble() * 0.2f) + 0.2f;
            float g = (float)(Random.NextDouble() * 0.2f) + 0.8f;
            float b = (float)(Random.NextDouble() * 0.5f);

            return new Vector3(r, g, b);
        }

        public override void Draw(Matrix view, Matrix projection, Effect effect)
        {
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["DiffuseColor"].SetValue(_defaultColor);

            Model.Root.Transform = _world;
            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
            foreach (var mesh in Model.Meshes)
            {
                var relativeTransform = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                effect.Parameters["World"].SetValue(relativeTransform);
                mesh.Draw();
            }
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TGC.MonoGame.TP
{
    internal class Bush : WorldEntity
    {
        public static Model Model;

        public Bush(Vector3 position, Vector3 scale, float yaw) : base(position, scale, yaw, Model)
        {
            _world = Matrix.CreateScale(0.01f) * Matrix.CreateTranslation(0,-0.909759561f, 0) * Matrix.CreateScale(scale) * Matrix.CreateRotationY(yaw) * Matrix.CreateTranslation(position);
            _defaultColors = GetDefaultColors(Model.Meshes.Count);
      
        }

        public static void LoadContent(ContentManager Content, Effect Effect)
        {
            Model = LoadContent(Content, "bush/bush1", Effect);
        }

        protected void Update(GameTime gameTime)
        {
            // ¿¿??
        }

        public override BoundingBoxLocalCoordinates GetLocalBoundingBox(Model model)
        {
            Vector3 min = new Vector3(-1.55790175f, -0.743808021f, -1.55288585f);
            Vector3 max = new Vector3(1.76620225f, 2.097365639f, 1.77121815f);
            BoundingBoxLocalCoordinates localBox = new BoundingBoxLocalCoordinates(min, max);
            return localBox;
        }

        public override Vector3[] GetDefaultColors(int meshes)
        {
            Vector3 green = new Vector3(0.050980392f, 0.180392157f, 0.109803922f);
            Vector3 brown = new Vector3(0.105882353f, 0.074509804f, 0.050980392f);

            Vector3[] colors = new Vector3[meshes];
            colors[0] = green;
            colors[1] = brown;
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

using BepuPhysics.Constraints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TGC.MonoGame.TP.Cameras;

namespace TGC.MonoGame.TP
{
    internal class Tree
    {
        public static Model Model;
        public static Random Random;

        private Matrix _world;
        private Vector3 _position;
        private float _yaw;
        private Vector3 _defaultColor;

        public Tree(Vector3 position, float yaw)
        {
            _position = position;
            _yaw = yaw;
            _world = Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateRotationY(yaw) * Matrix.CreateTranslation(position);

            _defaultColor = GetDefaultColor();
        }

        public void LoadContent()
        {
            // TODO cargar modelo del árbol
        }

        protected void Update(GameTime gameTime)
        {
            // ¿¿??
        }

        public void Draw(Matrix view, Matrix projection, Effect effect)
        {
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["DiffuseColor"].SetValue(_defaultColor);

            foreach (var mesh in Model.Meshes)
            {
                effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * _world);
                mesh.Draw();
            }
        }

        public Vector3 GetDefaultColor()
        {
            float r = (float)(Random.NextDouble() * 0.2f) + 0.2f;
            float g = (float)(Random.NextDouble() * 0.2f) + 0.8f;
            float b = (float)(Random.NextDouble() * 0.5f);

            return new Vector3(r, g, b);
        }



    }
}

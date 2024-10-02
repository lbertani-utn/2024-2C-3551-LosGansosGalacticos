using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TGC.MonoGame.TP
{
    internal class Rock : WorldEntity
    {
        public static Model Model;
        private readonly float moduloCorrimiento = 10.75f;
        private readonly float anguloCorrimiento = 0.1f;
        private readonly float alturaCorrimiento = 0.0f;

        public Rock(Vector3 position, Vector3 scale, float yaw) : base(position, scale, yaw, Model)
        {
            // Modelo local está descentrado del origen de coordenadas
            _yaw = yaw - MathHelper.Pi - anguloCorrimiento;
            _world = Matrix.CreateScale(scale) * Matrix.CreateRotationY(yaw) * Matrix.CreateTranslation(position - new Vector3(-(float)Math.Cos(yaw),alturaCorrimiento,(float)Math.Sin(yaw)) * moduloCorrimiento);

            // Console.WriteLine(_world.Translation.ToString());

            _defaultColor = GetDefaultColor();
        }

        public static void LoadContent(ContentManager Content, Effect Effect)
        {
            Model = LoadContent(Content, "rock/rock", Effect);
        }

        protected void Update(GameTime gameTime)
        {
            // ¿¿??
        }

        public override Vector3 GetDefaultColor()
        {
            var num = Random.NextDouble() * 0.3f + 0.3f;
            float r = (float)(num + Random.NextDouble() * 0.05f);
            float g = (float)(num + Random.NextDouble() * 0.05f);
            float b = (float)(num + Random.NextDouble() * 0.05f);

            return new Vector3(r, g, b);
        }

        public override void Draw(Matrix view, Matrix projection, Effect effect)
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
    }
}

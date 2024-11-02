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
            _world = Matrix.CreateScale(scale) * Matrix.CreateRotationY(yaw) * Matrix.CreateTranslation(position - new Vector3(-(float)Math.Cos(yaw), alturaCorrimiento, (float)Math.Sin(yaw)) * moduloCorrimiento);

            // Console.WriteLine(_world.Translation.ToString());

            _defaultColors = GetDefaultColors(Model.Meshes.Count);
        }

        public static void LoadContent(ContentManager Content, Effect Effect)
        {
            Model = LoadContent(Content, "rock/rock", Effect);
        }

        protected void Update(GameTime gameTime)
        {
            // ¿¿??
        }

        //public override (Vector3 center, Vector3 radius) GetLocalBoundingBox(Model model)
        //{
        //    Vector3 center = new Vector3(-10.194115f, 0.9169125f, 1.0564915f);
        //    Vector3 radius = new Vector3(2.141759265f, 0.8824215f, 2.141759265f);
        //    return (center, radius);
        //}

        public override Vector3[] GetDefaultColors(int meshes)
        {
            var num = Random.NextDouble() * 0.3f + 0.3f;
            float r = (float)(num + Random.NextDouble() * 0.05f);
            float g = (float)(num + Random.NextDouble() * 0.05f);
            float b = (float)(num + Random.NextDouble() * 0.05f);

            Vector3[] colors = new Vector3[meshes];
            colors[0] = new Vector3(r, g, b);
            return colors;
        }

        public override void Draw(Matrix view, Matrix projection, Effect effect)
        {
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);

            for (int i = 0; i < Model.Meshes.Count; i++)
            {
                effect.Parameters["DiffuseColor"].SetValue(_defaultColors[i]);
                effect.Parameters["World"].SetValue(Model.Meshes[i].ParentBone.Transform * _world);
                Model.Meshes[i].Draw();
            }
        }
    }
}

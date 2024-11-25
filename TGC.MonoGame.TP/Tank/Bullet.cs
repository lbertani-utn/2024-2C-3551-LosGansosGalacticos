using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Collisions;

namespace TGC.MonoGame.TP.Tank
{
    internal class Bullet : WorldEntity
    {
        private static Model Model;
        private static Texture[] Textures;
        private static BoundingBoxHelper ModelBoundingBox;
        public bool Active {get; set;}
        private Vector3 _direction;

        public Bullet() : base(Vector3.Zero, Vector3.One/4, 0f, Model)
        {
            Active = false;
            _world = Matrix.CreateScale(_scale) * Matrix.CreateTranslation(_position);
            _defaultColors = GetDefaultColors(Model.Meshes.Count);
        }

        public static void LoadContent(ContentManager Content, Effect Effect)
        {
            Model = LoadContent(Content, "geometries/sphere", Effect);

            Textures = new Texture[Model.Meshes.Count];
            Textures[0] = Content.Load<Texture2D>("Textures/metal");

            Vector3 min = new Vector3(-2f, -2f, -2f);
            Vector3 max = new Vector3(2f, 2f, 2f);
            ModelBoundingBox = new BoundingBoxHelper(min, max);
        }

        public void ResetValues(Vector3 position, Vector3 direction)
        {
            _position = position;
            _direction = direction;
            Active = true;
        }

        public override void Update(float elapsedTime)
        {
            _position += _direction * elapsedTime;
            _world = Matrix.CreateScale(_scale) * Matrix.CreateTranslation(_position);
        }

        protected override BoundingBox CreateBoundingBox(Model model, Vector3 position, Vector3 scale)
        {
            return ModelBoundingBox.GetBoundingBox(position, scale);
        }


        protected override Vector3[] GetDefaultColors(int meshes)
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
            base.Draw(view, projection, effect, Model, Textures);
        }
    }

}

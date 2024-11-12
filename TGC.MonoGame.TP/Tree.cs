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

        public override BoundingBoxLocalCoordinates GetLocalBoundingBox(Model model)
        {
            Vector3 min = new Vector3(-0.48368357f, -0.015338364f, -0.44941229f);
            Vector3 max = new Vector3(0.28571863f, 15.816038f, 0.31998991f);
            BoundingBoxLocalCoordinates localBox = new BoundingBoxLocalCoordinates(min, max);
            return localBox;
        }

        public override Vector3[] GetDefaultColors(int meshes)
        {
            float g = (float)(Random.NextDouble() * 0.2f) + 0.33f;
            float rb = (float)(Random.NextDouble() * 0.1f) + 0.05f;
            Vector3 green = new Vector3(rb, g, rb);

            float r = (float)(Random.NextDouble() * 0.11f) + 0.25f;
            float gb = (float)(Random.NextDouble() * 0.12f) + 0.05f;
            Vector3 brown = new Vector3(r, gb, gb);

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
            base.Draw(view, projection, effect, Model);
        }
    }
}

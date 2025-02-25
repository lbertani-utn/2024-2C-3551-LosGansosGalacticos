using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Collisions;
using TGC.MonoGame.TP.Materials;
using TGC.MonoGame.TP.Scenes.Entities;

namespace TGC.MonoGame.TP.Scenes.Battlefield
{
    internal class Tree : WorldEntity
    {
        private static Model Model;
        private static Texture[] Textures;
        private static Material[] Materials;
        private static BoundingBoxHelper ModelBoundingBox;
        private static BoundingBoxHelper ModelDrawBox;
        private BoundingBox _drawBox;

        public Tree(Vector3 position, Vector3 scale, float yaw) : base(position, scale, yaw, Model)
        {
            _world = Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateScale(scale) * Matrix.CreateRotationY(yaw) * Matrix.CreateTranslation(position);
            _drawBox = ModelDrawBox.GetBoundingBox(position, scale);
        }

        public static void LoadContent(ContentManager Content, Effect Effect)
        {
            Model = LoadContent(Content, "tree/tree", Effect);
            Textures = new Texture[Model.Meshes.Count];
            Texture wood = Content.Load<Texture2D>("Models/tree/bark_loo");
            Texture leaf = Content.Load<Texture2D>("Models/tree/leaf");
            Textures[0] = wood;
            Textures[1] = wood;
            Textures[2] = wood;
            Textures[3] = wood;
            Textures[4] = leaf;
            Textures[5] = leaf;

            Materials = new Material[Model.Meshes.Count];
            Material bark = new Bark();
            Material foliage = new Foliage();
            Materials[0] = bark;
            Materials[1] = bark;
            Materials[2] = bark;
            Materials[3] = bark;
            Materials[4] = foliage;
            Materials[5] = foliage;

            Vector3 min = new Vector3(-0.48368357f, -0.015338364f, -0.44941229f);
            Vector3 max = new Vector3(0.28571863f, 15.816038f, 0.31998991f);
            ModelBoundingBox = new BoundingBoxHelper(min, max);

            Vector3 drawMin = new Vector3(-6.42365616f, -0.015338f, -5.749832324f);
            Vector3 drawMax = new Vector3(5.19512384f, 15.816038f, 5.868947676f);
            ModelDrawBox = new BoundingBoxHelper(drawMin, drawMax);
        }

        public override void Update(float elapsedTime)
        {
            // ¿¿??
        }

        protected override BoundingBox CreateBoundingBox(Model model, Vector3 position, Vector3 scale)
        {
            return ModelBoundingBox.GetBoundingBox(position, scale);
        }

        public override BoundingBox GetDrawBox()
        {
            return _drawBox;
        }

        public override void DrawBoundingBox(Gizmos.Gizmos gizmos)
        {
            gizmos.DrawCube((_boundingBox.Max + _boundingBox.Min) / 2f, _boundingBox.Max - _boundingBox.Min, Color.Red);
            gizmos.DrawCube((_drawBox.Max + _drawBox.Min) / 2f, _drawBox.Max - _drawBox.Min, Color.Blue);
        }

        public override void Draw(Matrix view, Matrix projection, Effect effect)
        {
            base.Draw(view, projection, effect, Model, Textures, Materials);
        }

        public override void DrawShadowMap(Matrix view, Matrix projection, Effect effect)
        {
            base.DrawShadowMap(view, projection, effect, Model);
        }

    }
}

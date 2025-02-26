using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Collisions;
using TGC.MonoGame.TP.Materials;
using TGC.MonoGame.TP.Scenes.Entities;

namespace TGC.MonoGame.TP.Scenes.Battlefield
{
    internal class Bush : WorldEntity
    {
        private static Model Model;
        private static Texture[] Textures;
        private static Material[] Materials;
        private static BoundingBoxHelper ModelBoundingBox;

        public Bush(Vector3 position, Vector3 scale, float yaw) : base(position, scale, yaw, Model)
        {
            _world = Matrix.CreateScale(0.01f) * Matrix.CreateTranslation(0,-0.909759561f, 0) * Matrix.CreateScale(scale) * Matrix.CreateRotationY(yaw) * Matrix.CreateTranslation(position);
        }

        public static void LoadContent(ContentManager Content, Effect Effect)
        {
            Model = LoadContent(Content, "bush/bush1", Effect);
            Textures = new Texture[Model.Meshes.Count];
            Texture branch = Content.Load<Texture2D>("Models/bush/branch1 2");
            Texture leaf = Content.Load<Texture2D>("Models/bush/leaf2");
            Textures[0] = leaf;
            Textures[1] = branch;

            Materials = new Material[Model.Meshes.Count];
            Material bark = new Bark();
            Material foliage = new Foliage();
            Materials[0] = foliage;
            Materials[1] = bark;

            Vector3 min = new Vector3(-1.55790175f, -0.743808021f, -1.55288585f);
            Vector3 max = new Vector3(1.76620225f, 2.097365639f, 1.77121815f);
            ModelBoundingBox = new BoundingBoxHelper(min, max);
        }

        public override void Update(float elapsedTime)
        {
            // ¿¿??
        }

        protected override BoundingBox CreateBoundingBox(Model model, Vector3 position, Vector3 scale)
        {
            return ModelBoundingBox.GetBoundingBox(position, scale);
        }

        public override void Draw(Matrix view, Matrix projection, Effect effect)
        {
            base.Draw(view, projection, effect, Model, Textures, Materials); 
        }

        public override void DrawDepthPass(Effect effect, TargetCamera lightCamera)
        {
            base.DrawDepthPass(effect, lightCamera, Model);
        }

        public override void DrawShadowed(Matrix view, Matrix projection, Effect effect)
        {
            base.Draw(view, projection, effect, Model, Textures, Materials);
        }

        public override void DrawShadowMap(Matrix view, Matrix projection, Effect effect)
        {
            base.DrawShadowMap(view, projection, effect, Model);
        }

    }
}

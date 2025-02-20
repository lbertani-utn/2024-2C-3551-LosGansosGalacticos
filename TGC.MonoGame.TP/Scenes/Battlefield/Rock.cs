using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Collisions;
using TGC.MonoGame.TP.Materials;

namespace TGC.MonoGame.TP.Scenes.Battlefield
{
    internal class Rock : WorldEntity
    {
        private static Model Model;
        private static Texture[] Textures;
        private static Material[] Materials;
        private static BoundingBoxHelper ModelBoundingBox;

        public Rock(Vector3 position, Vector3 scale, float yaw) : base(position, scale, yaw, Model)
        {
            _world = Matrix.CreateTranslation(new Vector3(10.194115f, -0.923026f, -1.0564915f)) * Matrix.CreateScale(scale) * Matrix.CreateRotationY(yaw) * Matrix.CreateTranslation(position);
        }

        public static void LoadContent(ContentManager Content, Effect Effect)
        {
            Model = LoadContent(Content, "rock/rock", Effect);

            Textures = new Texture[Model.Meshes.Count];
            Textures[0] = Content.Load<Texture2D>("Models/rock/initialShadingGroup_Base_Color");

            Materials = new Material[Model.Meshes.Count];
            Materials[0] = new DefaultMaterial();

            Vector3 min = new Vector3(-11.78202851f, 0.034491f, -0.53142201f);
            Vector3 max = new Vector3(-8.60620149f, 1.799334f, 2.64440501f);
            Vector3 optbc = new Vector3(0f, -0.0061135f, 0f);
            ModelBoundingBox = new BoundingBoxHelper(min, max, optbc);
         
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

        public override void DrawShadowMap(Matrix view, Matrix projection, Effect effect)
        {
            base.DrawShadowMap(view, projection, effect, Model);
        }

    }
}

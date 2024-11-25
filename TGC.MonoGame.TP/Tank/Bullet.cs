using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Collisions;
using TGC.MonoGame.TP.Materials;

namespace TGC.MonoGame.TP.Tank
{
    internal class Bullet : WorldEntity
    {
        private static Model Model;
        private static Texture[] Textures;
        private static Material[] Materials;
        private static BoundingBoxHelper ModelBoundingBox;
        public bool Active {get; set;}
        private Vector3 _direction;

        public Bullet() : base(Vector3.Zero, Vector3.One/4, 0f, Model)
        {
            Active = false;
            _world = Matrix.CreateScale(_scale) * Matrix.CreateTranslation(_position);
        }

        public static void LoadContent(ContentManager Content, Effect Effect)
        {
            Model = LoadContent(Content, "geometries/sphere", Effect);

            Textures = new Texture[Model.Meshes.Count];
            Textures[0] = Content.Load<Texture2D>("Textures/metal");

            Materials = new Material[Model.Meshes.Count];
            Materials[0] = new DefaultMaterial();

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

        public override void Draw(Matrix view, Matrix projection, Effect effect)
        {
            base.Draw(view, projection, effect, Model, Textures, Materials); 
        }
    }

}

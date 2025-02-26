using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Collisions;
using TGC.MonoGame.TP.Materials;
using TGC.MonoGame.TP.Scenes.Entities;

namespace TGC.MonoGame.TP.Scenes.Headquarters
{
    internal class Table : WorldEntity
    {
        private static Model Model;
        private static Texture[] Textures;
        private static Texture[] Normals;
        private static Material[] Materials;
        private static BoundingBoxHelper ModelBoundingBox;

        public bool Active {get; set;}
        private Vector3 _direction;
        private Vector3 _lastPosition;
        private float _time;
        private Ray _movementRay;

        public Table(Vector3 position, Vector3 scale, float yaw) : base(position, scale, yaw, Model)
        {
            Active = false;
            _world = Matrix.CreateScale(_scale * 0.5f) * Matrix.CreateTranslation(_position);
            _time = 0f;
        }

        public static void LoadContent(ContentManager Content, Effect Effect)
        {
            Model = LoadContent(Content, "geometries/cube", Effect);

            Textures = new Texture[Model.Meshes.Count];
            Textures[0] = Content.Load<Texture2D>("Textures/Wood/Wood_027_basecolor");

            Normals = new Texture[Model.Meshes.Count];
            Normals[0] = Content.Load<Texture2D>("Textures/Wood/Wood_027_normal");

            Materials = new Material[Model.Meshes.Count];
            Materials[0] = new DefaultMaterial(); // Bark();

            ModelBoundingBox = new BoundingBoxHelper(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));
        }

        public override void Update(float elapsedTime)
        {
            // ¿¿??
        }

        protected override BoundingBox CreateBoundingBox(Model model, Vector3 position, Vector3 scale)
        {
            return ModelBoundingBox.GetBoundingBox(position, scale);
        }

        public override void DrawDepthPass(Effect effect, TargetCamera lightCamera)
        {
            base.DrawDepthPass(effect, lightCamera, Model);
        }

        public override void DrawShadowed(Matrix view, Matrix projection, Effect effect)
        {
            base.DrawWithNormalMap(view, projection, effect, Model, Textures, Normals, Materials);
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

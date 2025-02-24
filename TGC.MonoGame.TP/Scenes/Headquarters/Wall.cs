using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Materials;

namespace TGC.MonoGame.TP.Scenes.Headquarters
{
    internal class Wall : WorldEntity
    {
        private static Model Model;
        private static Texture[] Textures;
        private static Texture[] Normals;
        private static Material[] Materials;
        private static Vector3 BoxSize;

        public bool Active {get; set;}
        private Vector3 _direction;
        private Vector3 _lastPosition;
        private float _time;
        private Ray _movementRay;

        public Wall(Vector3 position, Vector3 scale, float yaw) : base(position, scale, yaw, Model)
        {
            Active = false;
            _world = Matrix.CreateScale(_scale) * Matrix.CreateTranslation(_position);
            _time = 0f;
        }

        public static void LoadContent(ContentManager Content, Effect Effect)
        {
            Model = LoadContent(Content, "geometries/cube", Effect);

            Textures = new Texture[Model.Meshes.Count];
            Textures[0] = Content.Load<Texture2D>("Textures/Brick Wall/brick_wall2-diff-512");

            Normals = new Texture[Model.Meshes.Count];
            Normals[0] = Content.Load<Texture2D>("Textures/Brick Wall/brick_wall2-nor-512");

            Materials = new Material[Model.Meshes.Count];
            Materials[0] = new DefaultMaterial();

            BoxSize = new Vector3(1f, 1f, 1f);
        }

        public override void Draw(Matrix view, Matrix projection, Effect effect)
        {
            Draw(view, projection, effect, Model, Textures, Materials); 
        }
    }
}

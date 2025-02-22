using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Materials;

namespace TGC.MonoGame.TP.Scenes.Battlefield
{
    internal class Box : WorldEntity
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

        public Box() : base(Vector3.Zero, Vector3.One, 0f, Model)
        {
            Active = false;
            _world = Matrix.CreateScale(_scale) * Matrix.CreateTranslation(_position);
            _time = 0f;
        }

        public static void LoadContent(ContentManager Content, Effect Effect)
        {
            Model = LoadContent(Content, "geometries/cube", Effect);

            Textures = new Texture[Model.Meshes.Count];
            Textures[0] = Content.Load<Texture2D>("Textures/Wood Crate/Wood_Crate_001_basecolor");

            Normals = new Texture[Model.Meshes.Count];
            Normals[0] = Content.Load<Texture2D>("Textures/Wood Crate/Wood_Crate_001_normal");

            Materials = new Material[Model.Meshes.Count];
            Materials[0] = new Bark();

            BoxSize = new Vector3(1f, 1f, 1f);
        }

        public void Update(float elapsedTime, SimpleTerrain terrain, List<WorldEntity> Entities, Tank[] Enemies)
        {
        }

        public override void Draw(Matrix view, Matrix projection, Effect effect)
        {
            Draw(view, projection, effect, Model, Textures, Normals, Materials); 
        }

    }
}

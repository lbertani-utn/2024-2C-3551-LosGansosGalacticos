﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Materials;
using TGC.MonoGame.TP.Scenes.Entities;

namespace TGC.MonoGame.TP.Scenes.Battlefield
{
    internal class Bullet : WorldEntity
    {
        private static Model Model;
        private static Texture[] Textures;
        private static Material[] Materials;
        private static Vector3 BoxSize;
        private const float gravity = 9.8f;

        public bool Active {get; set;}
        private Vector3 _direction;
        private Vector3 _lastPosition;
        private float _time;
        private Ray _movementRay;

        public Bullet() : base(Vector3.Zero, Vector3.One/4, 0f, Model)
        {
            Active = false;
            _world = Matrix.CreateScale(_scale) * Matrix.CreateTranslation(_position);
            _time = 0f;
        }

        public static void LoadContent(ContentManager Content, Effect Effect)
        {
            Model = LoadContent(Content, "geometries/sphere", Effect);

            Textures = new Texture[Model.Meshes.Count];
            Textures[0] = Content.Load<Texture2D>("Textures/metal");

            Materials = new Material[Model.Meshes.Count];
            Materials[0] = new DefaultMaterial();

            BoxSize = new Vector3(0.25f, 0.25f, 0.25f);
        }

        public void ResetValues(Vector3 position, Vector3 direction)
        {
            _lastPosition = position;
            _position = position;
            _boundingBox.Min = _position - BoxSize;
            _boundingBox.Max = _position + BoxSize;
            _movementRay.Position = _position;
            _movementRay.Direction = _direction;
            _direction = direction;
            _time = 0f;
            Active = true;
        }

        public void Update(float elapsedTime, SimpleTerrain terrain, List<WorldEntity> Entities, Tank[] Enemies)
        {
            // actualizo posición
            _lastPosition = _position;
            Vector3 move = _direction * elapsedTime + Vector3.Down * gravity * elapsedTime * _time * _time;
            _position += move;

            // actualizo bounding box
            _boundingBox.Min = _position - BoxSize;
            _boundingBox.Max = _position + BoxSize;

            _world = Matrix.CreateScale(_scale) * Matrix.CreateTranslation(_position);
            _time += elapsedTime;

            if (terrain.Height(_position.X, _position.Z) > _position.Y)
            {
                // TODO explosión
                Active = false;
                return;
            }

            float distance = Vector3.Distance(_lastPosition , _position);
            _movementRay = new Ray(_lastPosition, move);

            // colisiones con objetos del escenario
            // TODO destruir objetos
            foreach (WorldEntity e in Entities)
            {
                if (e.Status != WorldEntityStatus.Destroyed)
                {
                    if (_movementRay.Intersects(e.GetHitBox()) < distance)
                    {
                        // TODO explosión
                        e.Status = WorldEntityStatus.Destroyed;
                        Active = false;
                        return;
                    }
                }
            }

            // colisiones con tanques enemigos
            // TODO deformar tanques
            foreach (Tank t in Enemies)
            {
                if (t.Status != WorldEntityStatus.Destroyed)
                {
                    float? intersect = t.Intersects(_movementRay);
                    if (intersect != null && intersect.Value < distance)
                    {
                        // TODO explosión
                        t.Status = WorldEntityStatus.Destroyed;
                        Active = false;
                        return;
                    }
                }
            }

        }

        protected override BoundingBox CreateBoundingBox(Model model, Vector3 position, Vector3 scale)
        {
            return _boundingBox;
        }

        public override void Draw(Matrix view, Matrix projection, Effect effect)
        {
            Draw(view, projection, effect, Model, Textures, Materials); 
        }

        public override void DrawShadowMap(Matrix view, Matrix projection, Effect effect)
        {
            DrawShadowMap(view, projection, effect, Model);
        }

        public override void DrawBoundingBox(Gizmos.Gizmos gizmos)
        {
            gizmos.DrawCube((_boundingBox.Max + _boundingBox.Min) / 2f, _boundingBox.Max - _boundingBox.Min, Color.Red);
            gizmos.DrawLine(_lastPosition, _position, Color.White);
        }

    }
}

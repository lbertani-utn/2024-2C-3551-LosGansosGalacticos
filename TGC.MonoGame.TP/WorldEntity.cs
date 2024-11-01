using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP {
    public class WorldEntity {
        private const string ContentFolder3D = "Models/";
        public static Random Random;
        protected Vector3 _position;
        protected BoundingBox _boundingBox;
        protected Vector3[] _defaultColors;
        protected Matrix _world;
        protected Vector3 _scale;
        protected float _yaw;
        protected ((int gridX, int gridZ) Min, (int gridX, int gridZ) Max) gridIndices = new();

        public WorldEntity(Vector3 position, Vector3 scale, float yaw, Model model) {
            _position = position;
            _scale = scale;
            _yaw = yaw;

            (Vector3 center, Vector3 radius) localBox = GetLocalBoundingBox(model);
            _boundingBox.Min = position + (localBox.center - localBox.radius) * scale;
            _boundingBox.Max = position + (localBox.center + localBox.radius) * scale;
        }

        public Vector3 GetPosition() {
            return _position;
        }

        public static Model LoadContent(ContentManager Content, string modelRelativePath, Effect Effect)
        {
            // Cargo el modelo
            Model model = Content.Load<Model>(ContentFolder3D + modelRelativePath);
            Random = new Random();
            // Asigno el efecto que cargue a cada parte del mesh.
            // Un modelo puede tener mas de 1 mesh internamente.
            foreach (var mesh in model.Meshes)
            {
                // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Effect;
                }
            }
            
            return model;
        }

        public virtual void Draw(Matrix view, Matrix projection, Effect effect) {}
        public void DrawBoundingBox(Gizmos.Gizmos gizmos)
        {
            gizmos.DrawCube((_boundingBox.Max + _boundingBox.Min) / 2f, _boundingBox.Max - _boundingBox.Min, Color.Red);
        }
        public void DrawPosition(Gizmos.Gizmos gizmos)
        {
            gizmos.DrawSphere(_position, Vector3.One, Color.White);
        }

        public virtual Vector3[] GetDefaultColors(int meshes) {
            Vector3[] colors = new Vector3[meshes];
            for (int i = 0; i < meshes; i++) 
            {
                colors[i] = new Vector3((float)Random.NextDouble(), (float)Random.NextDouble(), (float)Random.NextDouble());
            }
            return colors;
        }

        public void DebugCollision(CollisionData data) {
            var x = data.gridPosition.x / (float) data.gridSize.x;
            var z = data.gridPosition.z / (float) data.gridSize.z;

            for (int i = 0; i < _defaultColors.Length; i++)
            {
                _defaultColors[i] = new Vector3(x, 0, z);
            }
        }

        public BoundingBox GetBoundingBox() {
            return _boundingBox;
        }

        public virtual (Vector3 center, Vector3 radius) GetLocalBoundingBox(Model model)
        {
            Vector3 center = model.Meshes[0].BoundingSphere.Center;
            Vector3 radius = new Vector3(model.Meshes[0].BoundingSphere.Radius, model.Meshes[0].BoundingSphere.Radius, model.Meshes[0].BoundingSphere.Radius);
            return (center, radius);
        }

        public ((int gridX, int gridZ) Min, (int gridX, int gridZ) Max) GetGridIndices() {
            return gridIndices;
        }

        public void SetGridMinIndex(int gridMinX, int gridMinZ) {
            gridIndices.Min.gridX = gridMinX;
            gridIndices.Min.gridZ = gridMinZ;
        }

        public void SetGridMaxIndex(int gridMaxX, int gridMaxZ) {
            gridIndices.Max.gridX = gridMaxX;
            gridIndices.Max.gridZ = gridMaxZ;
        }

        public void SetGridIndices(int gridMinX, int gridMinZ, int gridMaxX, int gridMaxZ) {
            SetGridMinIndex(gridMinX, gridMinZ);
            SetGridMaxIndex(gridMaxX, gridMaxZ);
        }
    }
}

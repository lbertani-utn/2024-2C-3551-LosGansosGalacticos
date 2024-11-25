using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Collisions;
using TGC.MonoGame.TP.Materials;

namespace TGC.MonoGame.TP {

    public class WorldEntity {
        private const string ContentFolder3D = "Models/";
        protected static Random Random;
        protected Vector3 _position;
        protected BoundingBox _boundingBox;
        protected Vector3[] _defaultColors;
        protected Matrix _world;
        protected Vector3 _scale;
        protected float _yaw;
        protected WorldEntityStatus _status;
        protected ((int gridX, int gridZ) Min, (int gridX, int gridZ) Max) gridIndices = new();

        public WorldEntityStatus Status
        {
            get => _status;
            set => _status = value;
        }

        public WorldEntity(Vector3 position, Vector3 scale, float yaw, Model model) {
            _status = WorldEntityStatus.Intact;
            _position = position;
            _scale = scale;
            _yaw = yaw;

            _boundingBox = CreateBoundingBox(model, position, scale);
        }

        public Vector3 GetPosition() {
            return _position;
        }

        public static Model LoadContent(ContentManager Content, string modelRelativePath, Effect Effect)
        {
            // Cargo el modelo
            Stopwatch sw = Stopwatch.StartNew();
            Model model = Content.Load<Model>(ContentFolder3D + modelRelativePath);
            sw.Stop();
            Debug.WriteLine("Load model {0}: {1} milliseconds", modelRelativePath, sw.ElapsedMilliseconds);


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

        public virtual void Update(float elapsedTime) { }

        public virtual void Draw(Matrix view, Matrix projection, Effect effect) {}

        protected void Draw(Matrix view, Matrix projection, Effect effect, Model model, Texture[] textures)
        {
            model.Root.Transform = _world;
            var modelMeshesBaseTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

            for (int i = 0; i < model.Meshes.Count; i++)
            {
                var relativeTransform = modelMeshesBaseTransforms[model.Meshes[i].ParentBone.Index];
                effect.Parameters["World"].SetValue(relativeTransform);
                effect.Parameters["WorldViewProjection"].SetValue(relativeTransform * view * projection);
                effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(relativeTransform)));
                effect.Parameters["baseTexture"].SetValue(textures[i]);
                model.Meshes[i].Draw();
            }
        }

        protected void Draw(Matrix view, Matrix projection, Effect effect, Model model, Texture[] textures, Material[] materials)
        {
            model.Root.Transform = _world;
            var modelMeshesBaseTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

            for (int i = 0; i < model.Meshes.Count; i++)
            {
                var relativeTransform = modelMeshesBaseTransforms[model.Meshes[i].ParentBone.Index];
                effect.Parameters["World"].SetValue(relativeTransform);
                effect.Parameters["WorldViewProjection"].SetValue(relativeTransform * view * projection);
                effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(relativeTransform)));
                effect.Parameters["baseTexture"].SetValue(textures[i]);
                effect.Parameters["ambientColor"].SetValue(materials[i].AmbientColor);
                effect.Parameters["diffuseColor"].SetValue(materials[i].DiffuseColor);
                effect.Parameters["specularColor"].SetValue(materials[i].SpecularColor);
                effect.Parameters["KAmbient"].SetValue(materials[i].KAmbient);
                effect.Parameters["KDiffuse"].SetValue(materials[i].KDiffuse);
                effect.Parameters["KSpecular"].SetValue(materials[i].KSpecular);
                effect.Parameters["shininess"].SetValue(materials[i].Shininess);
                model.Meshes[i].Draw();
            }
        }

        public virtual void DrawBoundingBox(Gizmos.Gizmos gizmos)
        {
            gizmos.DrawCube((_boundingBox.Max + _boundingBox.Min) / 2f, _boundingBox.Max - _boundingBox.Min, Color.Red);
        }

        public void DrawPosition(Gizmos.Gizmos gizmos)
        {
            gizmos.DrawSphere(_position, Vector3.One, Color.White);
        }

        public void DebugCollision(CollisionData data) {
            var x = data.gridPosition.x / (float) data.gridSize.x;
            var z = data.gridPosition.z / (float) data.gridSize.z;

            for (int i = 0; i < _defaultColors.Length; i++)
            {
                _defaultColors[i] = new Vector3(x, 0, z);
            }
        }

        public BoundingBox GetHitBox() {
            return _boundingBox;
        }
        public virtual BoundingBox GetDrawBox()
        {
            return _boundingBox;
        }

        protected virtual BoundingBox CreateBoundingBox(Model model, Vector3 position, Vector3 scale)
        {
            BoundingBoxHelper boxHelper =new BoundingBoxHelper(model.Meshes[0].BoundingSphere.Center, model.Meshes[0].BoundingSphere.Radius);
            return boxHelper.GetBoundingBox(position, scale);
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

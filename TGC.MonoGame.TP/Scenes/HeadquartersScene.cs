using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Scenes.Headquarters;

namespace TGC.MonoGame.TP.Scenes
{
    class HeadquartersScene : Scene
    {
        private Effect ObjectEffect;

        public HeadquartersScene(GraphicsDeviceManager graphics, ContentManager content) : base(graphics, content)
        {

        }

        public override void Initialize()
        {
            StaticObjects = new List<WorldEntity>();
        }

        #region Load
        public override void LoadContent()
        {
            Floor.LoadContent(content, ObjectEffect);
            Wall.LoadContent(content, ObjectEffect);
            Box.LoadContent(content, ObjectEffect);
            Table.LoadContent(content, ObjectEffect);

            LoadGizmos();
            LoadSceneObjects();
        }

        protected override void LoadSceneObjects()
        {
            // Vector3 position, Vector3 scale, float yaw
            StaticObjects.Add(new Wall(new Vector3(4.85f, 1.5f, 0f), new Vector3(0.3f, 3f, 10f), 0f));
            StaticObjects.Add(new Wall(new Vector3(-0.15f, 1.5f, 4.85f), new Vector3(9.7f, 3f, 0.3f), 0f));
            StaticObjects.Add(new Floor(new Vector3(0f, -0.01f, 0f), new Vector3(10f, 0.02f, 10f), 0f));
            StaticObjects.Add(new Table(new Vector3(-1f, 0.4f, -1f), new Vector3(2f, 0.8f, 2f), 0f));
            StaticObjects.Add(new Box(new Vector3(4.20f, 0.5f, 4.20f), new Vector3(1f, 1f, 1f), 0f));
            StaticObjects.Add(new Box(new Vector3(4.20f, 0.5f, 3.20f), new Vector3(1f, 1f, 1f), 0f));
            StaticObjects.Add(new Box(new Vector3(4.20f, 0.5f, 2.20f), new Vector3(1f, 1f, 1f), 0f));

            // Mapa
            // Tanques 1 jugador + 5 enemigos
        }
        #endregion

        public override void Update(float elapsedTime, UserInput input)
        {
            throw new NotImplementedException();
        }

        public override void Draw(CameraType SelectedCamera, bool drawBoundingBoxes, bool drawPositions, bool drawShadowMap)
        {
            SelectCamera(SelectedCamera);

            throw new NotImplementedException();
        }

        protected override void DrawGizmos(bool drawBoundingBoxes, bool drawPositions)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }


    }
}

using System;
using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.Scenes.Headquarters;

namespace TGC.MonoGame.TP.Scenes
{
    class HeadquartersScene : Scene
    {
        public HeadquartersScene()
        {
            // Vector3 position, Vector3 scale, float yaw
            Wall wall1 = new Wall(new Vector3(4.85f, 1.5f, 0f), new Vector3(0.3f, 3f, 10f), 0f);
            Wall wall2 = new Wall(new Vector3(0f, 1.5f, 4.85f), new Vector3(10f, 3f, 0.3f), 0f);
            Floor floor = new Floor(new Vector3(0f, -0.01f, 0f), new Vector3(10f, 0.02f, 10f), 0f); ;
            Table table = new Table(new Vector3(-3f, 0.4f, -3f), new Vector3(2f, 0.8f, 2f), 0f); ;
            Box box1 = new Box(new Vector3(4.35f, 0.5f, 4.35f), new Vector3(1f, 1f, 1f), 0f); ;
            Box box2 = new Box(new Vector3(4.35f, 0.5f, 3.35f), new Vector3(1f, 1f, 1f), 0f); ;
            Box box3 = new Box(new Vector3(4.35f, 0.5f, 2.35f), new Vector3(1f, 1f, 1f), 0f); ;
            
            // Mapa
            // Tanques 1 jugador + 5 enemigos

        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override void Draw()
        {
            throw new NotImplementedException();
        }

        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        public override void LoadContent()
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

    }
}

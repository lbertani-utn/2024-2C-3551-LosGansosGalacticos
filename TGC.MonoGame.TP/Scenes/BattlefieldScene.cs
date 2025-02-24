using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.Scenes.Battlefield;

namespace TGC.MonoGame.TP.Scenes
{
    class BattlefieldScene : Scene
    {
        SimpleTerrain Terrain;
        SkyBox Sky;

        public BattlefieldScene()
        {
            StaticObjects = new List<WorldEntity>();
            DynamicObjects = new List<WorldEntity>();
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
            //throw new NotImplementedException();
        }

        public override void LoadObjects()
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

    }
}

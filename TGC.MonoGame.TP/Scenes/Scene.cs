using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.Cameras;

namespace TGC.MonoGame.TP.Scenes
{
    class Scene : BasicObject
    {
        protected List<BasicObject> StaticObjects;
        protected List<BasicObject> DynamicObjects;
        protected Camera MainCamera;
        protected Camera LightCamera;
        protected Camera DebugCamera;
        protected Vector3 LightPosition;

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

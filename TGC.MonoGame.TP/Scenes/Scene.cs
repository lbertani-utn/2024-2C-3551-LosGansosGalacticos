using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.Cameras;

namespace TGC.MonoGame.TP.Scenes
{
    abstract class Scene
    {
        protected List<WorldEntity> StaticObjects;
        protected List<WorldEntity> DynamicObjects;
        protected Camera MainCamera;
        protected Camera LightCamera;
        protected Camera DebugCamera;
        protected Vector3 LightPosition;

        public abstract void Initialize();
        public abstract void LoadContent();
        public abstract void LoadObjects();
        public abstract void Update();
        public abstract void Draw();
        public abstract void Dispose();

    }
}

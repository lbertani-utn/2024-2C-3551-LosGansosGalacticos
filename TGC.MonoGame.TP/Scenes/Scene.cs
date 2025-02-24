using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.Cameras;

namespace TGC.MonoGame.TP.Scenes
{
    abstract class Scene : BasicObject
    {
        protected List<BasicObject> StaticObjects;
        protected List<BasicObject> DynamicObjects;
        protected Camera MainCamera;
        protected Camera LightCamera;
        protected Camera DebugCamera;
        protected Vector3 LightPosition;


    }
}

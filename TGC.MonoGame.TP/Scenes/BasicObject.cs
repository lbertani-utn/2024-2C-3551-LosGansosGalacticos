using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.MonoGame.TP.Scenes
{
    abstract class BasicObject
    {
        public abstract void Initialize();
        public abstract void LoadContent();
        public abstract void Update();
        public abstract void Draw();
        public abstract void Dispose();
    }
}

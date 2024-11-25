using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.Materials
{
    public class Bark : Material
    {
        public override Vector3 AmbientColor => throw new NotImplementedException();

        public override Vector3 DiffuseColor => throw new NotImplementedException();

        public override Vector3 SpecularColor => throw new NotImplementedException();

        public override float KAmbient => 1f;

        public override float KDiffuse => 1f;

        public override float KSpecular => 0f;

        public override float Shininess => 1f;
    }
}

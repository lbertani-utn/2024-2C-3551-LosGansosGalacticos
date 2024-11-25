using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.Materials
{
    public class Chrome : Material
    {
        public override Vector3 AmbientColor => new Vector3(0.25f, 0.25f, 0.25f);

        public override Vector3 DiffuseColor => new Vector3(0.4f, 0.4f, 0.4f);

        public override Vector3 SpecularColor => new Vector3(0.774597f, 0.774597f, 0.774597f);

        public override float KAmbient => 1.5f;

        public override float KDiffuse => 0.4f;

        public override float KSpecular => 0.05f;

        public override float Shininess => 76.8f;
    }
}

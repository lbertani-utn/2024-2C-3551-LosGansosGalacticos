using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.Materials
{
    public class Brass : Material
    {
        public override Vector3 AmbientColor => new Vector3(0.329412f, 0.223529f, 0.027451f);

        public override Vector3 DiffuseColor => new Vector3(0.780392f, 0.568627f, 0.113725f);

        public override Vector3 SpecularColor => new Vector3(0.992157f, 0.941176f, 0.807843f);

        public override float KAmbient => 1.2f;

        public override float KDiffuse => 0.4f;

        public override float KSpecular => 0.05f;

        public override float Shininess => 27.8974f;
    }
}

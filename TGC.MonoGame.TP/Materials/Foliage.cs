using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.Materials
{
    public class Foliage : Material
    {
        public override Vector3 AmbientColor => new Vector3(0.05f, 0.05f, 0.05f);

        public override Vector3 DiffuseColor => new Vector3(0.1f, 0.35f, 0.1f);

        public override Vector3 SpecularColor => new Vector3(0.45f, 0.55f, 0.45f);

        public override float KAmbient => 0.8f;

        public override float KDiffuse => 0.8f;

        public override float KSpecular => 0.1f;

        public override float Shininess => 16f;
    }
}

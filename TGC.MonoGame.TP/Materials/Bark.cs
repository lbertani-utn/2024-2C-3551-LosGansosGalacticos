using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.Materials
{
    public class Bark : Material
    {
        public override Vector3 AmbientColor => new Vector3(0.45f, 0.33f, 0.25f);

        public override Vector3 DiffuseColor => new Vector3(0.45f, 0.33f, 0.25f);

        public override Vector3 SpecularColor => Vector3.Zero;

        public override float KAmbient => 0.2f;

        public override float KDiffuse => 0.1f;

        public override float KSpecular => 0f;

        public override float Shininess => 1f;
    }
}

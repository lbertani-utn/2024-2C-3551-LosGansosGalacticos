using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.Materials
{
    public class DefaultMaterial : Material
    {
        public override Vector3 AmbientColor => new Vector3(1f, 1f, 1f);

        public override Vector3 DiffuseColor => new Vector3(0.7f, 0.7f, 0.7f);

        public override Vector3 SpecularColor => new Vector3(1f, 1f, 1f);

        public override float KAmbient => 0.3f;

        public override float KDiffuse => 0.4f;

        public override float KSpecular => 0.1f;

        public override float Shininess => 16f;
    }
}

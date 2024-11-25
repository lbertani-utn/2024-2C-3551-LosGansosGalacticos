using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.Materials
{
    public abstract class Material
    {
        public abstract Vector3 AmbientColor { get; }
        public abstract Vector3 DiffuseColor { get; }
        public abstract Vector3 SpecularColor { get; }
        public abstract float KAmbient { get; }
        public abstract float KDiffuse { get; }
        public abstract float KSpecular { get; }
        public abstract float Shininess { get; }

    }
}

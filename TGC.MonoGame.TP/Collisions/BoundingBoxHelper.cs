using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.Collisions
{
    public class BoundingBoxHelper
    {
        private Vector3 _center;
        private Vector3 _distance;
        private Vector3 _min;
        private Vector3 _max;
        private Vector3 _size;

        private Vector3 _objectPositionToBoxCenter;
        public Vector3 ObjectPositionToBoxCenter
        {
            get => _objectPositionToBoxCenter;
            set => _objectPositionToBoxCenter = value;
        }

        public BoundingBoxHelper(Vector3 center, float radius)
        {
            _center = center;
            _distance = new Vector3(radius, radius, radius);
            _min = center - _distance;
            _max = center + _distance;
            _size = _distance * 2;
            _objectPositionToBoxCenter = new Vector3(0, center.Y, 0);
        }

        public BoundingBoxHelper(Vector3 min, Vector3 max)
        {
            _min = min;
            _max = max;
            _center = (max + min) / 2;
            _distance = (max - min) / 2;
            _size = _distance * 2;
            _objectPositionToBoxCenter = new Vector3(0, _center.Y, 0);
        }

        public BoundingBox GetBoundingBox(Vector3 position, Vector3 scale)
        {
            return new BoundingBox(position + (_objectPositionToBoxCenter - _distance) * scale, position + (_objectPositionToBoxCenter + _distance) * scale);
        }

        public BoundingCylinder GetBoundingCylinder(Vector3 position, Vector3 scale)
        {
            return new BoundingCylinder(position + _objectPositionToBoxCenter * scale, _distance.X * scale.X, _distance.Y * scale.Y);
        }

    }
}

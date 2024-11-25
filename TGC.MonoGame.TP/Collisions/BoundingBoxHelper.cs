using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.Collisions
{
    public class BoundingBoxHelper
    {
        public Vector3 Min { get => _min;}
        public Vector3 Max { get => _max; }

        private Vector3 _center;
        private Vector3 _distance;
        private Vector3 _min;
        private Vector3 _max;
        private Vector3 _size;
        private Vector3 _objectPositionToBoxCenter;

        public BoundingBoxHelper(Vector3 center, float radius) : this(center, radius, new Vector3(0, center.Y, 0)) { }

        public BoundingBoxHelper(Vector3 center, float radius, Vector3 objectPositionToBoxCenter)
        {
            _center = center;
            _distance = new Vector3(radius, radius, radius);
            _min = center - _distance;
            _max = center + _distance;
            _size = _distance * 2;
            _objectPositionToBoxCenter = objectPositionToBoxCenter;
        }

        public BoundingBoxHelper(Vector3 min, Vector3 max) : this(min, max, new Vector3(0, (min.Y + max.Y)/2, 0)) { }

        public BoundingBoxHelper(Vector3 min, Vector3 max, Vector3 objectPositionToBoxCenter)
        {
            _min = min;
            _max = max;
            _center = (max + min) / 2;
            _distance = (max - min) / 2;
            _size = _distance * 2;
            _objectPositionToBoxCenter = objectPositionToBoxCenter;
        }

        public BoundingBox GetBoundingBox(Vector3 position, Vector3 scale)
        {
            return new BoundingBox(position + (_objectPositionToBoxCenter - _distance) * scale, position + (_objectPositionToBoxCenter + _distance) * scale);
        }

        public OrientedBoundingBox GetOrientedBoundingBox(Vector3 position, Vector3 scale)
        {
            return new OrientedBoundingBox(position + _objectPositionToBoxCenter * scale, _distance * scale);
        }

        public void UpdatePosition(Vector3 newPosition)
        {
            _center = newPosition;
            _min = _center - _distance;
            _max = _center + _distance;
        }

    }
}

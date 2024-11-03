using BepuPhysics.Constraints;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP
{
    public class BoundingBoxLocalCoordinates
    {
        private Vector3 _center;
        public Vector3 Center
        {
            get => _center;
        }

        private Vector3 _distance;
        public Vector3 Distance
        {
            get => _distance;
        }

        private Vector3 _min;
        public Vector3 Min
        {
            get => _min;
        }

        private Vector3 _max;
        public Vector3 Max
        {
            get => _max;
        }

        private Vector3 _size;
        public Vector3 Size
        {
            get => _size;
        }

        private Vector3 _objectPositionToBoxCenter;
        public Vector3 ObjectPositionToBoxCenter
        {
            get => _objectPositionToBoxCenter;
        }

        public BoundingBoxLocalCoordinates(Vector3 center, float radius)
        {
            _center = center;
            _distance = new Vector3(radius, radius, radius);
            _min = center - _distance;
            _max = center + _distance;
            _size = _distance * 2;
            _objectPositionToBoxCenter = new Vector3(0, center.Y, 0);
        }

        public BoundingBoxLocalCoordinates(Vector3 min, Vector3 max)
        {
            _min = min;
            _max = max;
            _center = (max + min) / 2;
            _distance = (max - min) / 2;
            _size = _distance * 2;
            _objectPositionToBoxCenter = new Vector3(0, _center.Y, 0);
        }

    }
}

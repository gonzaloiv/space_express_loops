using UnityEngine;

namespace DigitalLove.Input
{
    public enum HandSide
    {
        Left,
        Right
    }

    public readonly struct HandPinchState
    {
        public readonly bool IsTracked;
        public readonly bool IsPinching;
        public readonly Vector3 Point;
        public readonly Vector3 StartPoint;

        public HandPinchState(bool isTracked, bool isPinching, Vector3 point, Vector3 startPoint)
        {
            IsTracked = isTracked;
            IsPinching = isPinching;
            Point = point;
            StartPoint = startPoint;
        }
    }

    public readonly struct TwoHandGestureState
    {
        public readonly HandPinchState Left;
        public readonly HandPinchState Right;
        public readonly bool IsActive;
        public readonly float Distance;
        public readonly float DistanceSpeed;

        public TwoHandGestureState(
            HandPinchState left,
            HandPinchState right,
            bool isActive,
            float distance,
            float distanceSpeed)
        {
            Left = left;
            Right = right;
            IsActive = isActive;
            Distance = distance;
            DistanceSpeed = distanceSpeed;
        }

        public bool IsLeftPinching => Left.IsPinching;
        public bool IsRightPinching => Right.IsPinching;
        public bool IsSingleHandPinching => IsLeftPinching != IsRightPinching;
        public bool IsAnyPinching => IsLeftPinching || IsRightPinching;
        public Vector3 DragPoint => IsLeftPinching ? Left.Point : Right.Point;
        public Vector3 DragStartPoint => IsLeftPinching ? Left.StartPoint : Right.StartPoint;
    }
}

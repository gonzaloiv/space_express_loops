using System;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    internal sealed class SpaceShapingFirstUse
    {
        private const float ScaleEpsilon = 0.001f;

        private bool pinchCommitted;
        private bool cubeMoved;
        private float scaleAtBegin;

        public event Action PinchCommitted;

        public event Action CubeMoved;

        public void Reset()
        {
            pinchCommitted = false;
            cubeMoved = false;
            scaleAtBegin = 0f;
        }

        public void ScalingStarted(float scale) => scaleAtBegin = scale;

        public void ScalingEnded(float scale)
        {
            if (pinchCommitted || Mathf.Abs(scale - scaleAtBegin) <= ScaleEpsilon)
                return;

            pinchCommitted = true;
            PinchCommitted?.Invoke();
        }

        public void Moved()
        {
            if (cubeMoved)
                return;

            cubeMoved = true;
            CubeMoved?.Invoke();
        }
    }
}
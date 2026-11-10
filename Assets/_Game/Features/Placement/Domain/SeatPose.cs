using UnityEngine;

namespace DigitalLove.Game.Placement
{
    // Where the field goes for a seated player: in front of the head along its yaw (pitch ignored, so
    // looking down at launch does not tilt the field) at seated reach. Height comes from the hands when
    // they rest in front (they find the desk), else a fixed drop below the eyes; always clamped to a
    // comfortable band. Forward points away from the player.
    public readonly struct SeatPose
    {
        public readonly Vector3 Position;
        public readonly Vector3 Forward;

        private SeatPose(Vector3 position, Vector3 forward)
        {
            Position = position;
            Forward = forward;
        }

        // Hands are palm positions; null when the hand is not tracked.
        public static SeatPose From(Vector3 head, Quaternion headRotation, Vector3? left, Vector3? right, SeatSettings settings)
        {
            Vector3 forward = YawForward(headRotation);
            Vector3 position = head + forward * settings.distance;
            float drop = settings.defaultDrop;
            if (TryRestHeight(head, forward, new Hands(left, right), settings, out float handsY))
                drop = head.y - (handsY + settings.aboveHands);

            position.y = head.y - Mathf.Clamp(drop, settings.minDrop, settings.maxDrop);
            return new SeatPose(position, forward);
        }

        private static Vector3 YawForward(Quaternion headRotation)
        {
            Vector3 forward = Vector3.ProjectOnPlane(headRotation * Vector3.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.ProjectOnPlane(headRotation * Vector3.up, Vector3.up);
            return forward.normalized;
        }

        private static bool TryRestHeight(
            Vector3 head, Vector3 forward, Hands hands, SeatSettings settings, out float y)
        {
            RestHeight rest = default;
            AddIfResting(head, forward, hands.Left, settings, ref rest);
            AddIfResting(head, forward, hands.Right, settings, ref rest);
            y = rest.Count > 0 ? rest.Sum / rest.Count : 0f;
            return rest.Count > 0;
        }

        private static void AddIfResting(
            Vector3 head, Vector3 forward, Vector3? hand, SeatSettings settings, ref RestHeight rest)
        {
            if (!hand.HasValue || !IsResting(head, forward, hand.Value, settings))
                return;

            rest.Count++;
            rest.Sum += hand.Value.y;
        }

        private static bool IsResting(Vector3 head, Vector3 forward, Vector3 hand, SeatSettings settings)
        {
            Vector3 offset = hand - head;
            float reach = Vector3.Dot(offset, forward);
            float side = Mathf.Abs(Vector3.Dot(offset, Vector3.Cross(Vector3.up, forward)));
            float drop = -offset.y;
            return reach >= settings.restReach.x && reach <= settings.restReach.y
                && side <= settings.restSide
                && drop >= settings.restDrop.x && drop <= settings.restDrop.y;
        }

        private readonly struct Hands
        {
            public readonly Vector3? Left;
            public readonly Vector3? Right;

            public Hands(Vector3? left, Vector3? right)
            {
                Left = left;
                Right = right;
            }
        }

        private struct RestHeight
        {
            public int Count;
            public float Sum;
        }
    }
}

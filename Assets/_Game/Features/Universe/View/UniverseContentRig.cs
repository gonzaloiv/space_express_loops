using UnityEngine;

namespace DigitalLove.Game.Universe
{
    internal class UniverseContentRig
    {
        private readonly Transform contentRoot;
        private readonly Transform roomAnchor;
        private readonly Transform bubbleAnchor;
        private readonly UniverseBubbleSettings settings;

        private Vector3 contentCenterLocal;
        private float contentRadius = 2.5f;
        private Vector3 homePosition;
        private Quaternion homeRotation = Quaternion.identity;
        private Vector3 temporaryPosition;
        private Quaternion temporaryRotation;
        private float temporaryScale;

        public UniverseContentRig(
            Transform contentRoot,
            Transform roomAnchor,
            Transform bubbleAnchor,
            UniverseBubbleSettings settings)
        {
            this.contentRoot = contentRoot;
            this.roomAnchor = roomAnchor;
            this.bubbleAnchor = bubbleAnchor;
            this.settings = settings;
        }

        public bool HasRoomAnchor { get; private set; }
        public float Scale => contentRoot.lossyScale.x;
        public float MiniScale => Mathf.Clamp(settings.miniBubbleRadius / contentRadius, settings.minMiniScale, 1f);
        public float BubbleRadiusWorld => contentRadius * Scale;
        public Vector3 ContentPosition => contentRoot.position;
        public Vector3 CenterWorld => contentRoot.position + contentRoot.rotation * (Scale * contentCenterLocal);
        public Vector3 HomePosition => homePosition;
        public Quaternion HomeRotation => homeRotation;

        public void Initialize()
        {
            roomAnchor.SetPositionAndRotation(contentRoot.position, contentRoot.rotation);
            roomAnchor.localScale = Vector3.one;
            contentRoot.SetParent(roomAnchor, false);
            ApplyRoomPose();
        }

        public void SetRoomAnchor(Pose pose, Bounds roomBounds)
        {
            roomAnchor.SetPositionAndRotation(pose.position, YawOnly(pose.rotation));
            roomAnchor.localScale = Vector3.one;
            if (roomBounds.size.sqrMagnitude > 0.0001f)
                contentRadius = Mathf.Max(roomBounds.extents.magnitude, 0.1f);
            HasRoomAnchor = true;
        }

        public void ApplyRoomPose()
        {
            contentRoot.SetParent(roomAnchor, false);
            contentRoot.localPosition = Vector3.zero;
            contentRoot.localRotation = Quaternion.identity;
            contentRoot.localScale = Vector3.one;
        }

        public void EnterBubble()
        {
            PrepareBubble();
            ParentContentToBubble();
        }

        public void PrepareBubble()
        {
            homePosition = GetCameraFront();
            homeRotation = GetCameraYaw();
            bubbleAnchor.SetPositionAndRotation(homePosition, homeRotation);
            bubbleAnchor.localScale = Vector3.one * MiniScale;
        }

        public void ParentContentToBubble()
        {
            contentRoot.SetParent(bubbleAnchor, false);
            contentRoot.localPosition = -contentCenterLocal;
            contentRoot.localRotation = Quaternion.identity;
            contentRoot.localScale = Vector3.one;
        }

        public Vector3 GetBubbleContentPosition() =>
            bubbleAnchor.position - bubbleAnchor.rotation * (bubbleAnchor.localScale.x * contentCenterLocal);

        public void CompleteTransition(UniverseViewMode target)
        {
            if (target == UniverseViewMode.Bubble)
                ParentContentToBubble();
            else
                ApplyRoomPose();
        }

        public void BeginTemporaryRoomPose()
        {
            temporaryPosition = contentRoot.position;
            temporaryRotation = contentRoot.rotation;
            temporaryScale = Scale;
            ApplyRoomPose();
        }

        public void EndTemporaryRoomPose(UniverseViewMode mode, bool isTransitioning)
        {
            if (isTransitioning)
            {
                contentRoot.SetParent(roomAnchor, false);
                contentRoot.SetPositionAndRotation(temporaryPosition, temporaryRotation);
                contentRoot.localScale = Vector3.one * temporaryScale;
                return;
            }

            if (mode == UniverseViewMode.Bubble)
                ParentContentToBubble();
            else
                ApplyRoomPose();
        }

        public void SetContentCenterLocal(Vector3 centerLocal, bool applyToBubble)
        {
            contentCenterLocal = centerLocal;
            if (!applyToBubble)
                return;

            bubbleAnchor.localScale = Vector3.one * MiniScale;
            ParentContentToBubble();
        }

        public void RestorePlacement(Vector3 offsetLocal, Quaternion yaw, bool applyToBubble)
        {
            homeRotation = YawOnly(yaw);
            homePosition = GetCameraFront();
            bubbleAnchor.SetPositionAndRotation(
                homePosition + homeRotation * (offsetLocal * MiniScale),
                homeRotation);
            bubbleAnchor.localScale = Vector3.one * MiniScale;

            if (applyToBubble)
                ParentContentToBubble();
        }

        public BubblePlacement BuildPlacement() =>
            BubblePlacement.FromAnchor(bubbleAnchor.position, homePosition, homeRotation, Scale);

        public Vector3 WorldToContentLocal(Vector3 worldPosition) =>
            contentRoot.InverseTransformPoint(worldPosition);

        private Vector3 GetCameraFront()
        {
            Camera camera = Camera.main;
            if (camera == null)
                return roomAnchor.position + roomAnchor.forward * settings.bubbleDistance;

            return camera.transform.position
                + camera.transform.forward * settings.bubbleDistance
                + camera.transform.up * settings.bubbleHeightOffset;
        }

        private Quaternion GetCameraYaw()
        {
            Camera camera = Camera.main;
            return camera != null ? YawOnly(camera.transform.rotation) : YawOnly(roomAnchor.rotation);
        }

        private static Quaternion YawOnly(Quaternion rotation) =>
            Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
    }
}

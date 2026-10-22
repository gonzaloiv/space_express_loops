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
        private Vector3 focusOffsetLocal;
        private Vector3 centerWorld;
        private Quaternion homeRotation = Quaternion.identity;
        private bool bubbleCenterLocked;
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
        public float MiniScale => Mathf.Clamp(
            settings.miniBubbleRadius / Mathf.Max(settings.viewRadius, 0.01f),
            settings.minMiniScale,
            1f);
        public float BubbleRadiusWorld => Mathf.Max(settings.viewRadius, 0.01f) * Scale;
        public Vector3 ContentPosition => contentRoot.position;
        public Vector3 CenterWorld => centerWorld;
        public Quaternion HomeRotation => homeRotation;
        public Vector3 FocusOffsetLocal => focusOffsetLocal;

        private Vector3 FocusLocal => contentCenterLocal + focusOffsetLocal;

        public void Initialize()
        {
            roomAnchor.SetPositionAndRotation(contentRoot.position, contentRoot.rotation);
            roomAnchor.localScale = Vector3.one;
            contentRoot.SetParent(roomAnchor, false);
            ApplyCanonicalRoomPose();
        }

        public void SetRoomAnchor(Pose pose)
        {
            roomAnchor.SetPositionAndRotation(pose.position, YawOnly(pose.rotation));
            roomAnchor.localScale = Vector3.one;
            HasRoomAnchor = true;
        }

        public void RecaptureBubble()
        {
            centerWorld = GetCameraFront();
            homeRotation = GetCameraYaw();
            focusOffsetLocal = Vector3.zero;
        }

        public void EnterBubble()
        {
            bubbleCenterLocked = false;
            RecaptureBubble();
            ApplyBubblePose();
        }

        public void ApplyBubblePose()
        {
            bubbleAnchor.SetPositionAndRotation(centerWorld, homeRotation);
            bubbleAnchor.localScale = Vector3.one * MiniScale;
            ParentContentToDisplay();
        }

        public void ApplyRoomPose()
        {
            bubbleAnchor.SetPositionAndRotation(centerWorld, homeRotation);
            bubbleAnchor.localScale = Vector3.one;
            ParentContentToDisplay();
        }

        public void ApplyCanonicalRoomPose()
        {
            contentRoot.SetParent(roomAnchor, false);
            contentRoot.localPosition = Vector3.zero;
            contentRoot.localRotation = Quaternion.identity;
            contentRoot.localScale = Vector3.one;
        }

        public void CompleteTransition(UniverseViewMode target)
        {
            if (target == UniverseViewMode.Bubble)
                ApplyBubblePose();
            else
                ApplyRoomPose();
        }

        public Vector3 GetDisplayContentPosition(float scale) =>
            centerWorld - homeRotation * (scale * FocusLocal);

        public void SetContentCenterLocal(Vector3 centerLocal, bool applyToBubble)
        {
            contentCenterLocal = centerLocal;
            if (!applyToBubble)
                return;

            if (!bubbleCenterLocked)
            {
                bubbleCenterLocked = true;
                RecaptureBubble();
            }

            ApplyBubblePose();
        }

        public void SetFocusOffset(Vector3 offsetLocal)
        {
            focusOffsetLocal = offsetLocal;
            ApplyFocus();
        }

        public void ApplyDrag(Vector3 startOffsetLocal, Vector3 worldOffset)
        {
            focusOffsetLocal = startOffsetLocal
                - Quaternion.Inverse(homeRotation) * worldOffset / Mathf.Max(MiniScale, 0.0001f);
            ApplyFocus();
        }

        public void BeginTemporaryRoomPose()
        {
            temporaryPosition = contentRoot.position;
            temporaryRotation = contentRoot.rotation;
            temporaryScale = Scale;
            ApplyCanonicalRoomPose();
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
                ApplyBubblePose();
            else
                ApplyRoomPose();
        }

        public BubblePlacement BuildPlacement() => new(focusOffsetLocal, homeRotation);

        public void RestorePlacement(Vector3 offsetLocal, Quaternion yaw, bool applyToBubble)
        {
            centerWorld = GetCameraFront();
            homeRotation = YawOnly(yaw);
            focusOffsetLocal = offsetLocal;
            bubbleCenterLocked = true;
            if (applyToBubble)
                ApplyBubblePose();
        }

        public Vector3 WorldToContentLocal(Vector3 worldPosition) =>
            contentRoot.InverseTransformPoint(worldPosition);

        private void ParentContentToDisplay()
        {
            contentRoot.SetParent(bubbleAnchor, false);
            contentRoot.localPosition = -FocusLocal;
            contentRoot.localRotation = Quaternion.identity;
            contentRoot.localScale = Vector3.one;
        }

        private void ApplyFocus()
        {
            contentRoot.localPosition = -FocusLocal;
        }

        private Vector3 GetCameraFront()
        {
            Camera camera = Camera.main;
            if (camera == null)
                return roomAnchor.position + roomAnchor.forward * settings.bubbleDistance;

            return camera.transform.position
                + camera.transform.forward * settings.bubbleDistance
                + Vector3.up * settings.bubbleHeightOffset;
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

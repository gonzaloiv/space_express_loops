using UnityEngine;

namespace DigitalLove.Game.Levels
{
    public class UniversePoseState
    {
        private readonly Transform contentRoot;
        private readonly float bubbleDistance;
        private readonly float bubbleHeightOffset;

        private UniverseView.UniversePose roomPose;
        private Vector3 contentCenter;
        private float contentRadius = 2.5f;
        private bool hasRoomAnchor;
        private Vector3 bubbleOffsetLocal;
        private Vector3 bubbleCenterWorld;
        private bool hasBubbleCenter;
        private Quaternion bubbleYaw = Quaternion.identity;
        private bool isPreviewActive;
        private UniverseView.UniversePose previewStartPose;
        private Vector3 previewStartOffsetLocal;
        private Vector3 previewStartFocusLocal;
        private Quaternion previewStartYaw;

        public UniversePoseState(Transform contentRoot, float bubbleDistance, float bubbleHeightOffset)
        {
            this.contentRoot = contentRoot;
            this.bubbleDistance = bubbleDistance;
            this.bubbleHeightOffset = bubbleHeightOffset;
            roomPose = UniverseView.UniversePose.FromTransform(contentRoot);
        }

        public bool HasRoomAnchor => hasRoomAnchor;
        public float ContentRadius => contentRadius;
        public Vector3 CenterLocal => contentCenter + bubbleOffsetLocal;
        public Vector3 CenterWorld => bubbleCenterWorld;
        public UniverseView.UniversePose RoomPose => roomPose;
        public UniverseView.UniversePose CurrentPose => UniverseView.UniversePose.FromTransform(contentRoot);
        public UniverseView.UniversePose PreviewStartPose => previewStartPose;
        public bool IsPreviewActive => isPreviewActive;
        public Vector3 BubbleOffsetLocal => bubbleOffsetLocal;
        public Quaternion BubbleYaw => bubbleYaw;

        public void SetRoomAnchor(Pose pose, Bounds bounds)
        {
            Quaternion yaw = YawOnly(pose.rotation);
            roomPose = new UniverseView.UniversePose(pose.position, yaw, 1f);
            if (bounds.size.sqrMagnitude > 0.0001f)
            {
                contentCenter = Quaternion.Inverse(yaw) * (bounds.center - pose.position);
                contentRadius = Mathf.Max(bounds.extents.magnitude, 0.1f);
            }

            bubbleOffsetLocal = Vector3.zero;
            bubbleYaw = yaw;
            hasRoomAnchor = true;
        }

        public void SetContentCenterLocal(Vector3 centerLocal) =>
            contentCenter = centerLocal;

        public bool HasBubbleCenter => hasBubbleCenter;

        public void SetBubbleCenter(Vector3 centerWorld)
        {
            bubbleCenterWorld = centerWorld;
            hasBubbleCenter = true;
        }

        public void CaptureBubbleCenter(UniverseView.UniversePose pose) =>
            SetBubbleCenter(pose.position + pose.rotation * (pose.scale * CenterLocal));

        public void ApplyBubblePose(float scale)
        {
            ApplyPose(hasBubbleCenter
                ? BuildMiniPoseAroundCenter(scale)
                : BuildMiniPose(scale));
        }

        public float GetMiniScale(float miniBubbleRadius, float minMiniScale) =>
            Mathf.Clamp(miniBubbleRadius / contentRadius, minMiniScale, 1f);

        public UniverseView.UniversePose BuildMiniPose(float scale)
        {
            Vector3 position = GetBubbleAnchor() - bubbleYaw * (scale * CenterLocal);
            return new UniverseView.UniversePose(position, bubbleYaw, scale);
        }

        public UniverseView.UniversePose BuildMiniPoseAroundCenter(float scale)
        {
            Vector3 position = bubbleCenterWorld - bubbleYaw * (scale * CenterLocal);
            return new UniverseView.UniversePose(position, bubbleYaw, scale);
        }

        public UniverseView.UniversePose BuildRoomPose()
        {
            Vector3 focusLocal = CenterLocal;
            Vector3 target = CenterWorld;
            return new UniverseView.UniversePose(target - bubbleYaw * focusLocal, bubbleYaw, 1f);
        }

        public void BeginPreview()
        {
            if (isPreviewActive)
                return;

            previewStartPose = CurrentPose;
            previewStartOffsetLocal = bubbleOffsetLocal;
            previewStartFocusLocal = contentCenter + bubbleOffsetLocal;
            previewStartYaw = bubbleYaw;
            isPreviewActive = true;
        }

        public void ApplyPreview(Vector3 offsetFromStart, Quaternion rotation, float scale)
        {
            if (!isPreviewActive)
                return;

            Quaternion yaw = YawOnly(rotation);
            Vector3 focusLocal = previewStartFocusLocal
                - Quaternion.Inverse(yaw) * (offsetFromStart / scale);

            bubbleYaw = yaw;
            bubbleOffsetLocal = focusLocal - contentCenter;

            Vector3 position = GetBubbleAnchor() - yaw * (scale * focusLocal);
            ApplyPose(new UniverseView.UniversePose(position, yaw, scale));
        }

        public void CancelPreview()
        {
            if (!isPreviewActive)
                return;

            isPreviewActive = false;
            bubbleOffsetLocal = previewStartOffsetLocal;
            bubbleYaw = previewStartYaw;
        }

        public void CommitPreview(float scale)
        {
            if (!isPreviewActive)
                return;

            CaptureCurrent(scale);
            isPreviewActive = false;
        }

        public void EndPreview() => isPreviewActive = false;

        public void RestoreBubbleTransform(Vector3 offsetLocal, Quaternion yaw)
        {
            bubbleOffsetLocal = offsetLocal;
            bubbleYaw = YawOnly(yaw);
        }

        public void ApplyTransitionPose(
            UniverseView.UniversePose start,
            UniverseView.UniversePose target,
            Vector3 centerLocal,
            Vector3 startCenter,
            Vector3 endCenter,
            float t)
        {
            float scale = Mathf.Lerp(start.scale, target.scale, t);
            Quaternion rotation = Quaternion.Slerp(start.rotation, target.rotation, t);
            Vector3 center = Vector3.Lerp(startCenter, endCenter, t);
            ApplyPose(new UniverseView.UniversePose(
                center - rotation * (scale * centerLocal),
                rotation,
                scale));
        }

        public void ApplyPose(UniverseView.UniversePose pose)
        {
            contentRoot.position = pose.position;
            contentRoot.rotation = pose.rotation;
            contentRoot.localScale = Vector3.one * pose.scale;
        }

        private void CaptureCurrent(float scale)
        {
            Quaternion yaw = YawOnly(contentRoot.rotation);
            Vector3 canonical = GetBubbleAnchor() - yaw * (scale * contentCenter);
            Vector3 offsetWorld = contentRoot.position - canonical;
            offsetWorld.y = 0f;

            bubbleYaw = yaw;
            bubbleOffsetLocal = Quaternion.Inverse(yaw) * offsetWorld / scale;
        }

        private Vector3 GetBubbleAnchor()
        {
            Camera camera = Camera.main;
            return camera != null
                ? camera.transform.position + camera.transform.forward * bubbleDistance
                    + camera.transform.up * bubbleHeightOffset
                : contentRoot.position + Vector3.forward * bubbleDistance;
        }

        private static Quaternion YawOnly(Quaternion rotation)
        {
            Vector3 forward = rotation * Vector3.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(forward.normalized, Vector3.up)
                : Quaternion.identity;
        }
    }
}

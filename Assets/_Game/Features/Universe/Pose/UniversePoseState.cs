using DG.Tweening;
using UnityEngine;

namespace DigitalLove.Game.Universe
{
    internal class UniversePoseState
    {
        private readonly Transform contentRoot;
        private readonly float bubbleDistance;
        private readonly float bubbleHeightOffset;
        private readonly float miniBubbleRadius;
        private readonly float minMiniScale;

        private UniversePose roomPose;
        private Vector3 contentCenter;
        private float contentRadius = 2.5f;
        private bool hasRoomAnchor;
        private Vector3 bubbleOffsetLocal;
        private Vector3 bubbleCenterWorld;
        private bool hasBubbleCenter;
        private Quaternion bubbleYaw = Quaternion.identity;
        private bool isPreviewActive;
        private Tween recenterTween;
        private UniversePose previewStartPose;
        private Vector3 previewStartOffsetLocal;
        private Vector3 previewStartFocusLocal;
        private Quaternion previewStartYaw;

        public UniversePoseState(Transform contentRoot, float bubbleDistance, float bubbleHeightOffset,
            float miniBubbleRadius, float minMiniScale)
        {
            this.contentRoot = contentRoot;
            this.bubbleDistance = bubbleDistance;
            this.bubbleHeightOffset = bubbleHeightOffset;
            this.miniBubbleRadius = miniBubbleRadius;
            this.minMiniScale = minMiniScale;
            roomPose = UniversePose.FromTransform(contentRoot);
        }

        public bool HasRoomAnchor => hasRoomAnchor;
        public float Scale => contentRoot.localScale.x;
        public float MiniScale => Mathf.Clamp(miniBubbleRadius / contentRadius, minMiniScale, 1f);
        public float BubbleRadiusWorld => contentRadius * Scale;
        public Vector3 CenterLocal => contentCenter + bubbleOffsetLocal;
        public Vector3 CenterWorld => bubbleCenterWorld;
        public UniversePose RoomPose => roomPose;
        public UniversePose CurrentPose => UniversePose.FromTransform(contentRoot);
        public UniversePose PreviewStartPose => previewStartPose;
        public bool IsPreviewActive => isPreviewActive;
        public Vector3 BubbleOffsetLocal => bubbleOffsetLocal;
        public Quaternion BubbleYaw => bubbleYaw;

        public void SetRoomAnchor(Pose pose, Bounds bounds)
        {
            Quaternion yaw = YawOnly(pose.rotation);
            roomPose = new UniversePose(pose.position, yaw, 1f);
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

        public void ResetBubbleOffset() => bubbleOffsetLocal = Vector3.zero;

        public void RecenterBubble(float scale, float secs)
        {
            recenterTween?.Kill();
            if (secs <= 0f)
            {
                bubbleOffsetLocal = Vector3.zero;
                ApplyPose(BuildMiniPoseAroundCenter(scale));
                return;
            }

            recenterTween = BuildRecenterTween(scale, secs)
                .OnComplete(() => { bubbleOffsetLocal = Vector3.zero; recenterTween = null; });
        }

        public void KillRecenter()
        {
            recenterTween?.Kill();
            recenterTween = null;
        }

        public void SetBubbleYawToCamera()
        {
            Camera camera = Camera.main;
            if (camera != null)
                bubbleYaw = YawOnly(camera.transform.rotation);
        }

        public void CaptureBubbleCenter(UniversePose pose)
        {
            bubbleCenterWorld = pose.position + pose.rotation * (pose.scale * CenterLocal);
            hasBubbleCenter = true;
        }

        public void ApplyBubblePose(float scale)
        {
            ApplyPose(hasBubbleCenter
                ? BuildMiniPoseAroundCenter(scale)
                : BuildMiniPose(scale));
        }

        public UniversePose BuildMiniPose(float scale)
        {
            Vector3 position = GetBubbleAnchor() - bubbleYaw * (scale * CenterLocal);
            return new UniversePose(position, bubbleYaw, scale);
        }

        public UniversePose BuildMiniPoseAroundCenter(float scale)
        {
            Vector3 position = bubbleCenterWorld - bubbleYaw * (scale * CenterLocal);
            return new UniversePose(position, bubbleYaw, scale);
        }

        public UniversePose BuildRoomPose()
        {
            Quaternion yaw = YawOnly(contentRoot.rotation);
            return new UniversePose(CenterWorld - yaw * CenterLocal, yaw, 1f);
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

        public void ApplyDrag(Vector3 worldOffset)
        {
            if (!isPreviewActive)
                return;

            float scale = Mathf.Max(previewStartPose.scale, 0.0001f);
            contentRoot.position = previewStartPose.position + worldOffset;
            contentRoot.rotation = previewStartPose.rotation;
            contentRoot.localScale = Vector3.one * scale;

            Vector3 focusLocal = previewStartFocusLocal
                - Quaternion.Inverse(bubbleYaw) * (worldOffset / scale);
            bubbleOffsetLocal = focusLocal - contentCenter;
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
            UniversePose start,
            UniversePose target,
            Vector3 centerLocal,
            Vector3 startCenter,
            float t)
        {
            float scale = Mathf.Lerp(start.scale, target.scale, t);
            Quaternion rotation = Quaternion.Slerp(start.rotation, target.rotation, t);
            Vector3 endCenter = target.position + target.rotation * (target.scale * centerLocal);
            Vector3 center = Vector3.Lerp(startCenter, endCenter, t);
            ApplyPose(new UniversePose(
                center - rotation * (scale * centerLocal),
                rotation,
                scale));
        }

        public void ApplyPose(UniversePose pose)
        {
            contentRoot.position = pose.position;
            contentRoot.rotation = pose.rotation;
            contentRoot.localScale = Vector3.one * pose.scale;
        }

        private Tween BuildRecenterTween(float scale, float secs)
        {
            Vector3 center = CenterWorld;
            Vector3 from = Quaternion.Inverse(bubbleYaw) * (center - CurrentPose.position) / scale;
            Vector3 to = contentCenter;
            return DOTween.To(
                    () => 0f,
                    t => ApplyPose(new UniversePose(
                        center - bubbleYaw * (scale * Vector3.Lerp(from, to, t)),
                        bubbleYaw,
                        scale)),
                    1f,
                    secs)
                .SetEase(Ease.InOutSine);
        }

        private void CaptureCurrent(float scale)
        {
            Quaternion yaw = YawOnly(contentRoot.rotation);
            Vector3 canonical = bubbleCenterWorld - yaw * (scale * contentCenter);
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

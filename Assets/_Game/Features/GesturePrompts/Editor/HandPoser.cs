using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.GesturePrompts.Editor
{
    // Poses one Meta OpenXR hand between a relaxed open hand and the SDK's pinch clip, and
    // places it so its pinch point (between thumb and index tips) lands where the motion asks.
    public class HandPoser
    {
        private const float Relaxed = 0.3f;

        private readonly Transform root;
        private readonly Transform poseRoot;
        private readonly Transform indexTip;
        private readonly Transform thumbTip;
        private readonly Transform wrist;
        private readonly Transform middleTip;
        private readonly List<Transform> bones = new();
        private readonly List<Quaternion> open = new();
        private readonly List<Quaternion> pinch = new();
        private readonly Vector3 pinchLocal;

        public HandPoser(GameObject instance, AnimationClip pinchClip)
        {
            root = instance.transform;
            wrist = Find("XRHand_Wrist");
            poseRoot = wrist.parent;
            indexTip = Find("XRHand_IndexTip");
            thumbTip = Find("XRHand_ThumbTip");
            middleTip = Find("XRHand_MiddleTip");
            poseRoot.GetComponentsInChildren(bones);
            Record(open);
            pinchClip.SampleAnimation(poseRoot.gameObject, 0f);
            Record(pinch);
            pinchLocal = root.InverseTransformPoint((indexTip.position + thumbTip.position) * 0.5f);
            // Only the fingers open; the wrist keeps the clip's orientation so the hand does not swing.
            for (int i = 0; i < bones.Count; i++)
            {
                bool finger = bones[i] != poseRoot && bones[i] != wrist;
                open[i] = finger ? Quaternion.Slerp(open[i], pinch[i], Relaxed) : pinch[i];
            }
        }

        public float Length => Vector3.Distance(wrist.position, middleTip.position);

        public Renderer[] Renderers => root.GetComponentsInChildren<Renderer>(true);

        public void Pose(float grip, Vector3 pinchPoint)
        {
            for (int i = 0; i < bones.Count; i++)
                bones[i].localRotation = Quaternion.Slerp(open[i], pinch[i], grip);

            // The pinch point is taken from the closed pose, so the hand does not drift as it closes.
            root.position += pinchPoint - root.TransformPoint(pinchLocal);
        }

        public void SetVisible(bool value)
        {
            foreach (Renderer renderer in Renderers)
                renderer.enabled = value;
        }

        private void Record(List<Quaternion> into)
        {
            into.Clear();
            for (int i = 0; i < bones.Count; i++)
                into.Add(bones[i].localRotation);
        }

        private Transform Find(string bone)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == bone)
                    return child;
            }

            throw new MissingReferenceException($"Hand bone {bone} not found on {root.name}.");
        }
    }
}

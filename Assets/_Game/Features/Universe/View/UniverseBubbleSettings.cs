using System;
using DG.Tweening;
using UnityEngine;

namespace DigitalLove.Game.Universe
{
    [Serializable]
    public class UniverseBubbleSettings
    {
        [Header("Bubble")]
        [Min(0.05f)] public float miniBubbleRadius = 0.28f;
        [Min(0.1f)] public float viewRadius = 2.5f;
        [Min(0.1f)] public float bubbleDistance = 0.45f;
        public float bubbleHeightOffset = -0.1f;
        [Min(0.001f)] public float minMiniScale = 0.02f;

        [Header("Scope")]
        [Range(0.1f, 2f)] public float scopeScale = 1.1f;
        public Vector3 scopeRadii = Vector3.one;

        [Header("Transition")]
        [Min(0f)] public float transitionSecs = 0.6f;
        public Ease transitionEase = Ease.InOutSine;
    }
}

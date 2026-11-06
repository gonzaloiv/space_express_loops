using System;
using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    internal class MapPose
    {
        private const float DragLimitFactor = 2.5f;
        private const float DragResistance = 0.25f;
        private const float DragMaxFactor = 1.5f;

        private readonly Transform mapRoot;
        private readonly Transform selectedSlot;
        private readonly Transform cube;
        private readonly ViewPool<RunSlotView> fields;
        private readonly float viewRadius;

        private bool panning;
        private Vector3 panStart;
        private Vector3 panTarget;
        private float panElapsed;
        private float panSecs;
        private Vector3 dragBase;
        private Vector3 dragSlotBase;
        private Vector3 fadeFocus;
        private readonly Action<RunSlotView> applyFieldFade;

        public bool ShowAll { get; set; }

        public float FadeFar => viewRadius * mapRoot.lossyScale.x;

        public float FadeNear => FadeFar * 0.55f;

        public MapPose(
            Transform mapRoot,
            Transform selectedSlot,
            Transform cube,
            ViewPool<RunSlotView> fields,
            float viewRadius)
        {
            this.mapRoot = mapRoot;
            this.selectedSlot = selectedSlot;
            this.cube = cube;
            this.fields = fields;
            this.viewRadius = viewRadius;
            applyFieldFade = ApplyFieldFade;
        }

        public Vector3 MapOffset(Vector3 focus) =>
            -Vector3.Scale(mapRoot.localScale, focus);

        public void SetPose(Vector3 mapFocus, Vector3 slotFocus)
        {
            panning = false;
            mapRoot.localPosition = MapOffset(mapFocus);
            selectedSlot.localPosition = slotFocus;
            ApplyFade();
        }

        public void BeginPan(Vector3 mapFocus, Vector3 slotFocus, float secs)
        {
            panning = false;
            selectedSlot.localPosition = slotFocus;
            ApplyFade();
            if (secs <= 0.0001f)
            {
                SetPose(mapFocus, slotFocus);
                return;
            }

            panStart = mapRoot.localPosition;
            panTarget = MapOffset(mapFocus);
            panElapsed = 0f;
            panSecs = secs;
            panning = true;
        }

        public void Tick(float deltaSecs)
        {
            if (!panning)
                return;

            panElapsed += deltaSecs;
            float t = Mathf.Clamp01(panElapsed / panSecs);
            float eased = t * t * (3f - 2f * t);
            mapRoot.localPosition = Vector3.LerpUnclamped(panStart, panTarget, eased);
            ApplyFade();
            if (t < 1f)
                return;

            panning = false;
            mapRoot.localPosition = panTarget;
            ApplyFade();
        }

        public void StopPan() => panning = false;

        public void BeginDrag()
        {
            panning = false;
            dragBase = mapRoot.localPosition;
            dragSlotBase = selectedSlot.localPosition;
        }

        public void ApplyDrag(Vector3 worldOffset)
        {
            Vector3 offset = mapRoot.InverseTransformVector(worldOffset);
            offset.y = 0f;
            mapRoot.localPosition = dragBase + LimitDrag(offset);
            selectedSlot.localPosition = dragSlotBase;
            ApplyFade();
        }

        private Vector3 LimitDrag(Vector3 offset)
        {
            float limit = viewRadius * DragLimitFactor;
            float magnitude = offset.magnitude;
            if (magnitude <= limit)
                return offset;

            float compressed = Mathf.Min(
                limit + (magnitude - limit) * DragResistance,
                limit * DragMaxFactor);
            return offset * (compressed / magnitude);
        }

        public void ApplyFade()
        {
            if (fields == null)
                return;

            if (ShowAll)
            {
                fields.ForEachFrom(0, field => field.ClearMapFade());
                return;
            }

            fadeFocus = cube.position;
            fields.ForEachFrom(0, applyFieldFade);
        }

        private void ApplyFieldFade(RunSlotView field) =>
            field.SetMapFade(fadeFocus, FadeNear, FadeFar);
    }
}

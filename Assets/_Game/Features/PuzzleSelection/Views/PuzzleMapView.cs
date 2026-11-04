using System;
using System.Collections;
using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.Input;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public class PuzzleMapView : MonoBehaviour
    {
        [SerializeField] private Transform mapRoot;
        [SerializeField] private RunSlotView selectedSlot;
        [SerializeField] private float viewRadius = 0.45f;
        [SerializeField] private float panSecs = 0.25f;

        private readonly List<JobEntry> visibleJobs = new();
        private ViewPool<RunSlotView> fields;
        private RunCarouselGesture gesture;
        private PuzzleMap map;
        private Coroutine panRoutine;
        private bool isDragging;

        public event Action EnterRequested;

        private void Awake()
        {
            fields = new ViewPool<RunSlotView>(selectedSlot, mapRoot, "Map Field", null);
            gesture = new RunCarouselGesture(CarouselGestureSettings.Default);
        }

        public void Show(PuzzleMap map)
        {
            this.map = map;
            gameObject.SetActive(true);

            StopPan();
            isDragging = false;
            RenderFields();
            SetPose(Focus, Focus);
        }

        public void ApplyGesture(TwoHandGestureState state)
        {
            if (map == null)
                return;

            CarouselGestureOutcome outcome = gesture.Update(
                state, transform.right, transform.up, Time.unscaledTime);
            if (outcome.Enter)
            {
                StopPan();
                isDragging = false;
                EnterRequested?.Invoke();
                return;
            }

            if (outcome.IsDragging)
            {
                StopPan();
                isDragging = true;
                ApplyDrag(outcome.DragOffset);
                return;
            }

            if (outcome.FocusDelta != 0)
            {
                isDragging = false;
                if (map.SelectNext(outcome.FocusDelta))
                    RenderFields();
                AnimateTo(Focus);
                return;
            }

            if (isDragging)
            {
                isDragging = false;
                AnimateTo(Focus);
            }
        }

        public void Refuse() => selectedSlot.PlayRefusal();

        public void Hide()
        {
            StopPan();
            isDragging = false;
            gesture.Reset();
            map = null;
            selectedSlot.Hide();
            fields.ForEachFrom(0, field => field.Hide());
            gameObject.SetActive(false);
        }

        private Vector3 Focus => map.Position(map.SelectedIndex);

        private void RenderFields()
        {
            Vector3 focus = Focus;
            selectedSlot.ShowMapField(
                map.SelectedIndex, map.SelectedPuzzle, map.Selected.State, map.Selected.Stars, true);

            map.VisibleJobs(focus, viewRadius, visibleJobs);
            int slot = 0;
            for (int i = 0; i < visibleJobs.Count; i++)
            {
                JobEntry entry = visibleJobs[i];
                if (entry.Index == map.SelectedIndex)
                    continue;

                RunSlotView field = fields.EnsureSlot(slot++);
                field.transform.localPosition = map.Position(entry.Index);
                field.ShowMapField(
                    entry.Index, entry.Puzzle, entry.State, entry.Stars, false);
            }

            fields.ForEachFrom(slot, field => field.Hide());
        }

        private void ApplyDrag(Vector3 worldOffset)
        {
            Vector3 offset = mapRoot.InverseTransformVector(worldOffset);
            offset.y = 0f;
            mapRoot.localPosition = -Focus + offset;
            selectedSlot.transform.localPosition = Focus;
        }

        private void AnimateTo(Vector3 focus)
        {
            StopPan();
            if (panSecs <= 0.0001f)
            {
                SetPose(focus, focus);
                return;
            }

            panRoutine = StartCoroutine(PanRoutine(focus));
        }

        private IEnumerator PanRoutine(Vector3 focus)
        {
            Vector3 mapStart = mapRoot.localPosition;
            Vector3 slotStart = selectedSlot.transform.localPosition;
            Vector3 mapTarget = -focus;
            float elapsed = 0f;
            while (elapsed < panSecs)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / panSecs);
                float eased = t * t * (3f - 2f * t);
                mapRoot.localPosition = Vector3.LerpUnclamped(mapStart, mapTarget, eased);
                selectedSlot.transform.localPosition =
                    Vector3.LerpUnclamped(slotStart, focus, eased);
                yield return null;
            }

            SetPose(focus, focus);
            panRoutine = null;
        }

        private void SetPose(Vector3 mapFocus, Vector3 slotFocus)
        {
            mapRoot.localPosition = -mapFocus;
            selectedSlot.transform.localPosition = slotFocus;
        }

        private void StopPan()
        {
            if (panRoutine == null)
                return;

            StopCoroutine(panRoutine);
            panRoutine = null;
        }
    }
}

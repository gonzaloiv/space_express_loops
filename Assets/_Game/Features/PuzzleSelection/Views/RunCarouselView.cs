using System;
using System.Collections;
using System.Collections.Generic;
using DigitalLove.Input;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public class RunCarouselView : MonoBehaviour
    {
        [Header("Arc")]
        [SerializeField] private Transform arcRoot;
        [SerializeField] private Transform leftAnchor;
        [SerializeField] private Transform centreAnchor;
        [SerializeField] private Transform rightAnchor;
        [SerializeField] private RunSlotView slotPrototype;
        [SerializeField] private GameObject contentRoot;
        [SerializeField] private float stepAngleDegrees = 35f;
        [SerializeField] private float defaultSnapSecs = 0.2f;
        [SerializeField] private float fastSnapSecs = 0.08f;
        [SerializeField] private float slowSnapSecs = 0.35f;
        [SerializeField] private float flickSpeed = 1f;

        [Header("Gesture")]
        [SerializeField] private float scaleInMinRatio = 1.15f;
        [SerializeField] private float scaleInMinDistance = 0.06f;
        [SerializeField] private float minSwipeDistance = 0.03f;
        [SerializeField] private float minSwipeSpeed = 0.25f;
        [SerializeField] private float swipeHorizontalDominance = 1.5f;
        [SerializeField] private float enterTransitionSecs = 0.2f;

        private readonly List<RunSlotView> slots = new();
        private RunCarouselGesture gesture;
        private Quaternion arcBaseRotation;
        private Coroutine snapRoutine;
        private Coroutine enterRoutine;
        private int focusedIndex = -1;
        private int jobCount;
        private float pendingSnapSpeed;
        private bool isEntering;

        public event Action<int> FocusRequested;

        public event Action EnterRequested;

        private void Awake()
        {
            arcBaseRotation = arcRoot.localRotation;
            gesture = new RunCarouselGesture(Settings());
            BuildSlots();
            contentRoot.SetActive(false);
        }

        public void Show(RunCarousel carousel)
        {
            StopEnterRoutine();
            contentRoot.SetActive(true);
            isEntering = false;
            int previous = focusedIndex;
            jobCount = carousel.Count;
            focusedIndex = carousel.FocusedIndex;
            RenderSlot(0, focusedIndex - 1, carousel);
            RenderSlot(1, focusedIndex, carousel, true);
            RenderSlot(2, focusedIndex + 1, carousel);
            if (previous >= 0 && previous != focusedIndex)
            {
                PlaySnap(focusedIndex - previous, pendingSnapSpeed);
                pendingSnapSpeed = 0f;
            }
        }

        public void ApplyGesture(TwoHandGestureState state)
        {
            if (jobCount == 0 || isEntering)
                return;

            CarouselGestureOutcome outcome = gesture.Update(
                state, transform.right, transform.up, Time.unscaledTime);
            if (outcome.Enter)
                BeginEnter();
            else if (outcome.FocusDelta != 0)
            {
                pendingSnapSpeed = outcome.Speed;
                RequestFocus(focusedIndex + outcome.FocusDelta);
            }
        }

        public void Refuse()
        {
            StopEnterRoutine();
            slots[1].ResetScale();
            slots[1].PlayRefusal();
        }

        private void BeginEnter()
        {
            isEntering = true;
            Debug.Log("[Carousel] begin enter", this);
            StopEnterRoutine();
            enterRoutine = StartCoroutine(EnterRoutine());
        }

        private IEnumerator EnterRoutine()
        {
            float duration = Mathf.Max(enterTransitionSecs, 0.0001f);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                slots[1].SetScale(Mathf.Pow(1f - t, 3f));
                yield return null;
            }

            slots[1].SetScale(0f);
            enterRoutine = null;
            isEntering = false;
            Debug.Log("[Carousel] enter requested", this);
            EnterRequested?.Invoke();
        }

        private void StopEnterRoutine()
        {
            if (enterRoutine == null)
                return;

            StopCoroutine(enterRoutine);
            enterRoutine = null;
        }

        public void Hide()
        {
            contentRoot.SetActive(false);
            ResetGesture();
        }

        private void BuildSlots()
        {
            slots.Add(CreateSlot(leftAnchor));
            slots.Add(CreateSlot(centreAnchor));
            slots.Add(CreateSlot(rightAnchor));
        }

        private RunSlotView CreateSlot(Transform anchor)
        {
            RunSlotView slot = Instantiate(slotPrototype, anchor);
            slot.transform.localPosition = Vector3.zero;
            slot.transform.localRotation = Quaternion.identity;
            slot.gameObject.SetActive(false);
            slot.Touched += () => OnSlotTouched(slot);
            return slot;
        }

        private void RenderSlot(int slotIndex, int jobIndex, RunCarousel carousel, bool focused = false)
        {
            if (jobIndex < 0 || jobIndex >= carousel.Count)
            {
                slots[slotIndex].Hide();
                return;
            }

            JobEntry entry = carousel[jobIndex];
            slots[slotIndex].Show(entry.Index, entry.Puzzle, entry.State, entry.Stars, focused);
        }

        private void OnSlotTouched(RunSlotView slot)
        {
            if (slot.JobIndex < 0)
                return;

            pendingSnapSpeed = 0f;
            FocusRequested?.Invoke(slot.JobIndex);
        }

        private void RequestFocus(int index)
        {
            int target = Mathf.Clamp(index, 0, jobCount - 1);
            if (target != focusedIndex)
                FocusRequested?.Invoke(target);
        }

        private void PlaySnap(int direction, float speed)
        {
            if (snapRoutine != null)
                StopCoroutine(snapRoutine);

            float duration = speed > 0.0001f
                ? Mathf.Lerp(slowSnapSecs, fastSnapSecs, Mathf.Clamp01(speed / Mathf.Max(flickSpeed, 0.0001f)))
                : defaultSnapSecs;
            snapRoutine = StartCoroutine(SnapRoutine(direction, duration));
        }

        private IEnumerator SnapRoutine(int direction, float duration)
        {
            float from = direction * -stepAngleDegrees;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(duration, 0.0001f));
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                float angle = Mathf.Lerp(from, 0f, eased);
                arcRoot.localRotation = arcBaseRotation * Quaternion.Euler(0f, angle, 0f);
                yield return null;
            }

            arcRoot.localRotation = arcBaseRotation;
            snapRoutine = null;
        }

        private void ResetGesture()
        {
            gesture.Reset();
            StopEnterRoutine();
            if (snapRoutine != null)
                StopCoroutine(snapRoutine);

            snapRoutine = null;
            arcRoot.localRotation = arcBaseRotation;
            focusedIndex = -1;
            jobCount = 0;
            pendingSnapSpeed = 0f;
            isEntering = false;
        }

        private CarouselGestureSettings Settings() => new(
            scaleInMinRatio,
            scaleInMinDistance,
            minSwipeDistance,
            minSwipeSpeed,
            swipeHorizontalDominance);
    }
}

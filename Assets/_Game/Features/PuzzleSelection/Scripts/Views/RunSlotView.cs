using System;
using System.Collections;
using DigitalLove.Game.SpaceShaping;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public class RunSlotView : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private TMP_Text numberLabel;
        [SerializeField] private TMP_Text starsLabel;
        [SerializeField] private GameObject currentState;
        [SerializeField] private GameObject solvedState;
        [SerializeField] private GameObject lockedState;
        [SerializeField] private GameObject focusedState;
        [SerializeField] private PuzzleDioramaView diorama;
        [SerializeField] private RunSlotTouch touch;

        private Coroutine refusalRoutine;

        public event Action Touched;

        public int JobIndex { get; private set; } = -1;

        private void OnEnable() => touch.Touched += OnTouched;

        private void OnDisable() => touch.Touched -= OnTouched;

        public void Show(int index, PuzzleData puzzle, JobState state, int stars, bool focused)
        {
            JobIndex = index;
            gameObject.SetActive(true);
            numberLabel.text = (index + 1).ToString();
            starsLabel.text = Stars(stars);
            currentState.SetActive(state == JobState.Current);
            solvedState.SetActive(state == JobState.Solved);
            lockedState.SetActive(state == JobState.Locked);
            focusedState.SetActive(focused);
            diorama.Render(puzzle);
        }

        public void Hide()
        {
            JobIndex = -1;
            gameObject.SetActive(false);
        }

        public void PlayRefusal()
        {
            if (refusalRoutine != null)
                StopCoroutine(refusalRoutine);

            refusalRoutine = StartCoroutine(RefusalRoutine());
        }

        private void OnTouched() => Touched?.Invoke();

        private IEnumerator RefusalRoutine()
        {
            Vector3 start = visualRoot.localScale;
            float elapsed = 0f;
            const float duration = 0.18f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float pulse = 1f + Mathf.Sin(t * Mathf.PI * 3f) * 0.04f * (1f - t);
                visualRoot.localScale = start * pulse;
                yield return null;
            }

            visualRoot.localScale = start;
            refusalRoutine = null;
        }

        private static string Stars(int count)
        {
            if (count <= 0)
                return string.Empty;

            return new string('*', count);
        }
    }
}

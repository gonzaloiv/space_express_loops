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

        private Coroutine refusalRoutine;
        private Vector3 homeLocalPosition;
        private Quaternion homeLocalRotation;

        public int JobIndex { get; private set; } = -1;
        public Transform DioramaTransform => diorama.transform;

        private void Awake()
        {
            homeLocalPosition = diorama.transform.localPosition;
            homeLocalRotation = diorama.transform.localRotation;
        }

        public void Show(int index, PuzzleData puzzle, JobState state, int stars, bool focused)
        {
            JobIndex = index;
            gameObject.SetActive(true);
            ResetVisual();
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
            ResetVisual();
            gameObject.SetActive(false);
        }

        public void ResetVisual()
        {
            diorama.transform.localPosition = homeLocalPosition;
            diorama.transform.localRotation = homeLocalRotation;
            diorama.transform.localScale = Vector3.one;
        }

        public void PlayRefusal()
        {
            if (refusalRoutine != null)
                StopCoroutine(refusalRoutine);

            refusalRoutine = StartCoroutine(RefusalRoutine());
        }

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

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
        [SerializeField] private GameObject selectionCube;
        [SerializeField] private PuzzleDioramaView diorama;

        private Coroutine refusalRoutine;
        private Vector3 homeLocalPosition;
        private Quaternion homeLocalRotation;
        private Vector3 homeVisualScale;

        private void Awake()
        {
            homeLocalPosition = diorama.transform.localPosition;
            homeLocalRotation = diorama.transform.localRotation;
            homeVisualScale = visualRoot.localScale;
        }

        public void ShowMapField(
            int index, PuzzleData puzzle, JobState state, int stars, bool selected)
        {
            diorama.SetCubesVisible(false);
            Show(index, puzzle, state, stars);
            SetChromeVisible(false);
            selectionCube.SetActive(selected);
        }

        public void Hide()
        {
            ResetVisual();
            gameObject.SetActive(false);
        }

        private void Show(int index, PuzzleData puzzle, JobState state, int stars)
        {
            gameObject.SetActive(true);
            ResetVisual();
            numberLabel.text = (index + 1).ToString();
            starsLabel.text = Stars(stars);
            currentState.SetActive(state == JobState.Current);
            solvedState.SetActive(state == JobState.Solved);
            lockedState.SetActive(state == JobState.Locked);
            diorama.Render(puzzle);
        }

        private void SetChromeVisible(bool visible)
        {
            numberLabel.gameObject.SetActive(visible);
            starsLabel.gameObject.SetActive(visible);
            currentState.SetActive(visible);
            solvedState.SetActive(visible);
            lockedState.SetActive(visible);
        }

        private void ResetVisual()
        {
            visualRoot.localScale = homeVisualScale;
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

using System.Collections;
using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public class RunSlotView : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform currentState;
        [SerializeField] private Transform solvedState;
        [SerializeField] private Transform lockedState;
        [SerializeField] private PuzzleDioramaView diorama;
        [SerializeField, Range(0f, 0.1f)] private float idleBreathAmplitude = 0.02f;
        [SerializeField, Min(0f)] private float idleBreathSpeed = 1.2f;

        public string SunName => diorama.SunName;
        public Vector3 SunLocalPosition =>
            transform.InverseTransformPoint(diorama.SunWorldPosition);
        public int MapIndex { get; private set; } = -1;

        public void SetMapFade(Vector3 focus, float near, float far) =>
            diorama.SetMapFade(focus, near, far);

        public void ClearMapFade() => diorama.ClearMapFade();

        private Coroutine refusalRoutine;
        private PuzzleData renderedPuzzle;
        private Vector3 homeLocalPosition;
        private Quaternion homeLocalRotation;
        private Vector3 homeVisualScale;
        private float idlePhase;

        private void Awake()
        {
            homeLocalPosition = diorama.transform.localPosition;
            homeLocalRotation = diorama.transform.localRotation;
            homeVisualScale = visualRoot.localScale;
            currentState.gameObject.SetActive(false);
            solvedState.gameObject.SetActive(false);
            lockedState.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (idleBreathAmplitude <= 0f)
                return;

            idlePhase += Time.unscaledDeltaTime * idleBreathSpeed;
            float breath = 1f + Mathf.Sin(idlePhase) * idleBreathAmplitude;
            diorama.transform.localScale = Vector3.one * breath;
        }

        public void ShowMapField(int index, PuzzleData puzzle)
        {
            bool unchanged = MapIndex == index && renderedPuzzle == puzzle;
            MapIndex = index;
            if (unchanged)
            {
                gameObject.SetActive(true);
                return;
            }

            diorama.SetCubesVisible(false);
            Show(index, puzzle);
        }

        public void SetZoom(float scale)
        {
            StopRefusal();
            visualRoot.localScale = homeVisualScale * Mathf.Max(scale, 0f);
        }

        public void Hide()
        {
            MapIndex = -1;
            renderedPuzzle = null;
            ResetVisual();
            gameObject.SetActive(false);
        }

        private void Show(int index, PuzzleData puzzle)
        {
            gameObject.SetActive(true);
            ResetVisual();
            idlePhase = index * 0.7f;
            diorama.Render(index, puzzle);
            renderedPuzzle = puzzle;
        }

        private void ResetVisual()
        {
            StopRefusal();
            visualRoot.localScale = homeVisualScale;
            diorama.transform.localPosition = homeLocalPosition;
            diorama.transform.localRotation = homeLocalRotation;
            diorama.transform.localScale = Vector3.one;
        }

        private void StopRefusal()
        {
            visualRoot.localPosition = Vector3.zero;
            if (refusalRoutine == null)
                return;

            StopCoroutine(refusalRoutine);
            refusalRoutine = null;
        }

        public void PlayRefusal()
        {
            StopRefusal();
            refusalRoutine = StartCoroutine(RefusalRoutine());
        }

        private IEnumerator RefusalRoutine()
        {
            Vector3 startScale = visualRoot.localScale;
            Vector3 startPosition = visualRoot.localPosition;
            float elapsed = 0f;
            const float duration = 0.26f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float pulse = 1f + Mathf.Sin(t * Mathf.PI * 3f) * 0.08f * (1f - t);
                float shake = Mathf.Sin(t * Mathf.PI * 8f) * 0.008f * (1f - t);
                visualRoot.localScale = startScale * pulse;
                visualRoot.localPosition = startPosition + new Vector3(shake, 0f, 0f);
                yield return null;
            }

            visualRoot.localScale = startScale;
            visualRoot.localPosition = startPosition;
            refusalRoutine = null;
        }

    }
}

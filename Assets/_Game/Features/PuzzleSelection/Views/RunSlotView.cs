using System.Collections;
using DigitalLove.Game.SpaceShaping;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public class RunSlotView : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private PuzzleDioramaView diorama;
        [SerializeField] private TMP_Text starsLabel;
        [SerializeField, Range(0f, 0.1f)] private float idleBreathAmplitude = 0.02f;
        [SerializeField, Min(0f)] private float idleBreathSpeed = 1.2f;

        public string SunName => diorama.SunName;
        // The sun in the map's space (this slot's parent).
        public Vector3 SunMapPosition =>
            transform.parent.InverseTransformPoint(diorama.SunWorldPosition);
        public int MapIndex { get; private set; } = -1;

        public void SetMapFade(Vector3 focus, float near, float far)
        {
            diorama.SetMapFade(focus, near, far);
            ApplyCulled(Vector3.Distance(transform.position, focus) >= far);
        }

        public void ClearMapFade()
        {
            diorama.ClearMapFade();
            ApplyCulled(false);
        }

        public void SetLabelColor(Color color) => diorama.SetLabelColor(color);

        private void ApplyCulled(bool value)
        {
            culled = value;
            starsLabel.gameObject.SetActive(hasStars && !culled);
        }

        private Coroutine refusalRoutine;
        private PuzzleData renderedPuzzle;
        private Vector3 homeLocalPosition;
        private Quaternion homeLocalRotation;
        private Quaternion homeVisualRotation;
        private Vector3 homeVisualScale;
        private float idlePhase;
        private bool culled;
        private bool hasStars;

        private void Awake()
        {
            homeLocalPosition = diorama.transform.localPosition;
            homeLocalRotation = diorama.transform.localRotation;
            homeVisualRotation = visualRoot.localRotation;
            homeVisualScale = visualRoot.localScale;
        }

        private void Update()
        {
            if (idleBreathAmplitude <= 0f)
                return;

            idlePhase += Time.unscaledDeltaTime * idleBreathSpeed;
            float breath = 1f + Mathf.Sin(idlePhase) * idleBreathAmplitude;
            diorama.transform.localScale = Vector3.one * breath;
        }

        public void ShowMapField(JobEntry entry, Color tint, bool showFrames)
        {
            bool unchanged = MapIndex == entry.Index && renderedPuzzle == entry.Puzzle;
            MapIndex = entry.Index;
            if (unchanged)
            {
                gameObject.SetActive(true);
            }
            else
            {
                Show(entry.Index, entry.Puzzle);
            }

            diorama.SetCubesVisible(showFrames);
            ShowMapState(entry, tint);
        }

        private void ShowMapState(JobEntry entry, Color tint)
        {
            hasStars = entry.ShowsStars;
            starsLabel.text = hasStars ? new string('★', entry.Stars) : string.Empty;
            ApplyCulled(culled);
            diorama.SetLabelColor(tint);
        }

        // Every sun sits at its node's height, so the map cube slides on one level over all jobs.
        public void PlaceSunAt(Vector3 node)
        {
            transform.localPosition = node;
            transform.localPosition += Vector3.up * (node.y - SunMapPosition.y);
        }

        public void SetZoom(float scale)
        {
            StopRefusal();
            float zoom = Mathf.Max(scale, 0f);
            visualRoot.localScale = homeVisualScale * zoom;
            visualRoot.localPosition = SunLocalAtRest() * (1f - zoom);
        }

        // The diorama grows around its sun, not the slot's origin, so the sun holds its node
        // while the field scales up to enter.
        private Vector3 SunLocalAtRest()
        {
            Vector3 inside = visualRoot.InverseTransformPoint(diorama.SunWorldPosition);
            return homeVisualRotation * Vector3.Scale(homeVisualScale, inside);
        }

        public void Hide()
        {
            MapIndex = -1;
            renderedPuzzle = null;
            hasStars = false;
            culled = false;
            starsLabel.gameObject.SetActive(false);
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

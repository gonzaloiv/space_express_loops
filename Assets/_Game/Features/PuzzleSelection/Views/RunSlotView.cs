using System.Collections;
using DigitalLove.Game.SpaceShaping;
using UnityEngine;
using UnityEngine.Serialization;

namespace DigitalLove.Game.PuzzleSelection
{
    public class RunSlotView : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private PuzzleDioramaView diorama;
        [SerializeField] private MapJobTag jobTag;
        [SerializeField, Range(0f, 0.1f)] private float idleBreathAmplitude = 0.02f;
        [SerializeField, Min(0f)] private float idleBreathSpeed = 1.2f;
        [SerializeField, Min(0f)] private float refusalSecs = 0.26f;
        [SerializeField, Range(0f, 1f)] private float lockedDim = 0.65f;
        [Tooltip("How far a job that is not selected greys its planets; its sun stays lit.")]
        [SerializeField, Range(0f, 1f)] private float backgroundDim = 0.6f;
        [Tooltip("Height of the job's tag (number and stars) over the sun, in this slot's space.")]
        [FormerlySerializedAs("starsAboveSun")]
        [SerializeField, Min(0f)] private float tagAboveSun = 0.03f;

        private Coroutine refusalRoutine;
        private PuzzleData renderedPuzzle;
        private Vector3 homeLocalPosition;
        private Quaternion homeLocalRotation;
        private Quaternion homeVisualRotation;
        private Vector3 homeVisualScale;
        private float idlePhase;
        private bool culled;
        private bool tagged;
        private bool breathe;

        // The sun in the map's space (this slot's parent).
        public Vector3 SunMapPosition =>
            transform.parent.InverseTransformPoint(diorama.SunWorldPosition);
        public int MapIndex { get; private set; } = -1;

        // The tag fades with the planets: full near the cube, gone where they are culled.
        public void SetMapFade(Vector3 focus, float near, float far)
        {
            diorama.SetMapFade(focus, near, far);
            float distance = Vector3.Distance(transform.position, focus);
            jobTag.SetAlpha(1f - Mathf.InverseLerp(near, far, distance));
            ApplyCulled(distance >= far);
        }

        public void ClearMapFade()
        {
            diorama.ClearMapFade();
            jobTag.SetAlpha(1f);
            ApplyCulled(false);
        }

        public void SetLabelColor(Color color) => jobTag.SetNumberColor(color);

        public void SetEndingLit(bool lit, Color color)
        {
            diorama.SetStateDim(lit ? 0f : lockedDim);
            jobTag.SetNumberColor(color);
            jobTag.SetVisible(tagged && lit && !culled);
        }

        private void ApplyCulled(bool value)
        {
            culled = value;
            jobTag.SetVisible(tagged && !culled);
        }

        private void Awake()
        {
            homeLocalPosition = diorama.transform.localPosition;
            homeLocalRotation = diorama.transform.localRotation;
            homeVisualRotation = visualRoot.localRotation;
            homeVisualScale = visualRoot.localScale;
        }

        private void Update()
        {
            if (!breathe || idleBreathAmplitude <= 0f)
            {
                diorama.transform.localScale = Vector3.one;
                return;
            }

            idlePhase += Time.unscaledDeltaTime * idleBreathSpeed;
            float breath = 1f + Mathf.Sin(idlePhase) * idleBreathAmplitude;
            diorama.transform.localScale = Vector3.one * breath;
        }

        // The tag rides over the sun through the breath and the enter pinch.
        private void LateUpdate()
        {
            if (!tagged || culled)
                return;

            jobTag.transform.position =
                diorama.SunWorldPosition + transform.TransformVector(Vector3.up * tagAboveSun);
        }

        // Every job carries its tag (faded by distance from the cube), stars unlit until earned; only
        // the selected job shows
        // its route ends' rings.
        public void ShowMapField(JobEntry entry, Color tint, bool showFrames, bool selected)
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

            ShowMapState(entry, tint, showFrames, selected);
        }

        private void ShowMapState(JobEntry entry, Color tint, bool showFrames, bool selected)
        {
            tagged = true;
            diorama.SetEndpointsMarked(selected);
            breathe = entry.State == JobState.Current;
            jobTag.Show(entry.Index + 1, entry.ShowsStars, entry.Stars, tint);
            ApplyCulled(culled);
            diorama.SetCubesVisible(showFrames);
            float stateDim = entry.State == JobState.Locked ? lockedDim : 0f;
            diorama.SetStateDim(selected ? stateDim : Mathf.Max(stateDim, backgroundDim), stateDim);
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
            tagged = false;
            breathe = false;
            culled = false;
            jobTag.SetVisible(false);
            ResetVisual();
            gameObject.SetActive(false);
        }

        private void Show(int index, PuzzleData puzzle)
        {
            gameObject.SetActive(true);
            ResetVisual();
            idlePhase = index * 0.7f;
            diorama.Render(puzzle);
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
            float duration = Mathf.Max(refusalSecs, 0.0001f);
            float elapsed = 0f;
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

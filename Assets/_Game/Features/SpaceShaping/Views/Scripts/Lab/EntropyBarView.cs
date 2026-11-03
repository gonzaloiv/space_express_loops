using DigitalLove.Global;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DigitalLove.Game.SpaceShaping
{
    public class EntropyBarView : MonoBehaviour
    {
        private const float Epsilon = 0.00005f;

        [SerializeField] private SpaceShapingView view;
        [SerializeField] private TMP_Text label;
        [SerializeField] private RectTransform bar;
        [SerializeField] private RectTransform committedFill;
        [SerializeField] private RectTransform lastEditFill;
        [SerializeField] private RectTransform pendingMarker;
        [SerializeField] private Image committedImage;
        [SerializeField] private Image lastEditImage;
        [SerializeField] private Image pendingImage;
        [SerializeField] private ColorValue accent;
        [SerializeField] private ColorValue neutral;
        [SerializeField] private ColorValue danger;
        [SerializeField] private float height = 0.24f;
        [SerializeField] private float markerWidth = 0.012f;

        private PuzzleReadout lastReadout;
        private bool hasRendered;

        private void OnEnable()
        {
            view.ReadoutChanged += OnReadoutChanged;
            Place();
            OnReadoutChanged(view.Readout);
        }

        private void OnDisable() => view.ReadoutChanged -= OnReadoutChanged;

        private void LateUpdate() => FaceCamera();

        private void OnReadoutChanged(PuzzleReadout readout)
        {
            if (hasRendered && readout.Equals(lastReadout))
                return;

            hasRendered = true;
            lastReadout = readout;
            Place();
            Render(readout);
        }

        private void Place()
        {
            PuzzleData puzzle = view.CurrentPuzzle;
            if (puzzle == null)
                return;

            transform.localPosition = puzzle.zoneCentre + Vector3.up * height;
        }

        private void Render(PuzzleReadout readout)
        {
            float budget = Mathf.Max(readout.EntropyBudget, 0.0001f);
            float spent = Mathf.Clamp01(readout.EntropySpent / budget);
            float remaining = 1f - spent;
            float last = readout.HasLastEdit ? Mathf.Clamp01(readout.LastEditCost / budget) : 0f;
            last = Mathf.Min(last, 1f - remaining);
            SetSegment(committedFill, 0f, remaining);
            SetSegment(lastEditFill, remaining, last);
            RenderPending(readout, budget);
            SetColor(committedImage, accent.value);
            SetColor(lastEditImage, neutral.value);
            RenderLabel(readout);
        }

        private void RenderPending(PuzzleReadout readout, float budget)
        {
            bool active = Mathf.Abs(readout.EntropyPending) > Epsilon;
            pendingMarker.gameObject.SetActive(active);
            if (!active)
                return;

            bool overflow = readout.EntropyBudget > 0f
                && readout.EntropySpent + readout.EntropyPending > readout.EntropyBudget;
            float pendingRemaining =
                1f - Mathf.Clamp01((readout.EntropySpent + readout.EntropyPending) / budget);
            SetSegment(pendingMarker, pendingRemaining - markerWidth * 0.5f, markerWidth);
            SetColor(pendingImage, overflow ? danger.value : accent.value);
        }

        private void RenderLabel(PuzzleReadout readout)
        {
            float remaining = readout.EntropyBudget - readout.EntropySpent;
            label.SetText(remaining < 0f ? "{0:0.00} OVER" : "{0:0.00} LEFT", Mathf.Abs(remaining));
        }

        private void FaceCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
                return;

            Vector3 forward = transform.position - camera.transform.position;
            if (forward.sqrMagnitude < 0.000001f)
                return;

            Vector3 up = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(forward.normalized, up)) > 0.999f)
                up = camera.transform.right;
            transform.rotation = Quaternion.LookRotation(forward, up);
        }

        private static void SetColor(Image target, Color color) => target.color = color;

        private void SetSegment(RectTransform segment, float from, float width)
        {
            float barWidth = bar.rect.width;
            Vector2 size = segment.sizeDelta;
            size.x = width * barWidth;
            segment.sizeDelta = size;
            Vector2 position = segment.anchoredPosition;
            position.x = from * barWidth;
            segment.anchoredPosition = position;
        }
    }
}

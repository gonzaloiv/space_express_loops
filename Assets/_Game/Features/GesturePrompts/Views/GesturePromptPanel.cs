using System.Collections;
using DigitalLove.Localization;
using DigitalLove.UI.Behaviours;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.GesturePrompts
{
    // Reusable prompt: the art, caption and timing come from the content asset, so one prototype
    // serves every gesture and a second instance can be up at the same time.
    public class GesturePromptPanel : MonoBehaviour
    {
        [SerializeField] private GesturePromptSource source;
        [SerializeField] private SpriteRenderer art;
        [SerializeField] private TMP_Text caption;
        [SerializeField] private LineRenderer leaderLine;
        [SerializeField] private Vector3 lineStart = new(0f, -2.56f, 0f);
        [SerializeField] private Transform terminalFrame;
        [SerializeField] private LookAtCameraOnY billboard;
        [Header("Caption only (a prompt with no art)")]
        [Tooltip("The frame shrinks to the caption and centres on it.")]
        [SerializeField] private Vector3 captionOnlyFramePosition = new(0f, -3f, 0f);
        [SerializeField] private Vector3 captionOnlyFrameScale = new(27.5f, 7.5f, 1f);
        [Tooltip("The leader leaves the shrunk frame's bottom edge.")]
        [SerializeField] private Vector3 captionOnlyLineStart = new(0f, -4.5f, 0f);
        [SerializeField, Min(0f)] private float bootSecs = 0.18f;
        [SerializeField, Min(0f)] private float closeSecs = 0.18f;

        private Vector3? fullScale;
        private Vector3? frameHomePosition;
        private Vector3? frameHomeScale;
        private Vector3? subject;
        private GesturePromptContent content;
        private float closeAt;
        private float nextFrameAt;
        private int frame;
        private bool closing;

        public bool IsShowing => isActiveAndEnabled;

        public bool IsFree => !isActiveAndEnabled;

        public GestureKind? Kind => content != null ? content.Kind : null;

        public void Show(
            GesturePromptContent value, Vector3 position, Quaternion? facing, Vector3? leaderSubject)
        {
            content = value;
            Place(position, facing);
            if (!facing.HasValue)
                billboard.LookAtCamera();
            Follow(leaderSubject);
            gameObject.SetActive(true);
        }

        public void Follow(Vector3? leaderSubject) => subject = leaderSubject;

        // A fixed facing holds the panel (Spend and Stars on the meter); without one it turns on Y to
        // face the player.
        public void Place(Vector3 position, Quaternion? facing)
        {
            transform.position = position;
            billboard.enabled = !facing.HasValue;
            if (facing.HasValue)
                transform.rotation = facing.Value;
        }

        public void Hide()
        {
            StopAllCoroutines();
            closing = false;
            content = null;
            subject = null;
            leaderLine.enabled = false;
            transform.localScale = FullScale;
            gameObject.SetActive(false);
        }

        public void Close()
        {
            if (closing || content == null)
                return;

            closing = true;
            StopAllCoroutines();
            StartCoroutine(CutOff());
        }

        // Captured before the first boot flattens it, so every reuse restores the authored size.
        private Vector3 FullScale => fullScale ??= transform.localScale;

        private Vector3 FrameHomePosition => frameHomePosition ??= terminalFrame.localPosition;

        private Vector3 FrameHomeScale => frameHomeScale ??= terminalFrame.localScale;

        private bool HasArt => content != null && content.FrameCount > 0;

        private Vector3 LineStart => HasArt ? lineStart : captionOnlyLineStart;

        private void OnEnable()
        {
            if (content == null)
            {
                gameObject.SetActive(false);
                return;
            }

            closing = false;
            frame = 0;
            closeAt = Time.unscaledTime + content.ShowSecs;
            nextFrameAt = Time.unscaledTime;
            source.Completed += OnGestureCompleted;
            ShowArt();
            transform.localScale = CrtTransition.Flat(FullScale);
            StartCoroutine(CrtTransition.Boot(transform, FullScale, bootSecs));
        }

        private void OnDisable() => source.Completed -= OnGestureCompleted;

        private void LateUpdate() => LayoutLeader();

        // The line leaves the panel's bottom edge and finds the subject wherever it is this frame.
        private void LayoutLeader()
        {
            if (content == null || !subject.HasValue)
            {
                leaderLine.enabled = false;
                return;
            }

            leaderLine.enabled = true;
            leaderLine.positionCount = 2;
            leaderLine.SetPosition(0, ToLine(transform.TransformPoint(LineStart)));
            leaderLine.SetPosition(1, ToLine(subject.Value));
        }

        private Vector3 ToLine(Vector3 worldPoint) =>
            leaderLine.transform.InverseTransformPoint(worldPoint);

        private void Update()
        {
            if (closing)
                return;

            if (Time.unscaledTime >= closeAt)
            {
                Close();
                return;
            }

            if (content.FrameCount > 1 && Time.unscaledTime >= nextFrameAt)
                AdvanceFrame();
        }

        private void OnGestureCompleted(GestureKind completed)
        {
            if (content != null && completed == content.Kind)
                Close();
        }

        private void ShowArt()
        {
            FitFrame();
            AdvanceFrame();

            string text = LocalizationUtil.GetValue(content.CaptionKey);
            caption.text = text;
            caption.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }

        // A prompt with no art drops the art's room, so the frame fits the caption alone.
        private void FitFrame()
        {
            bool hasArt = HasArt;
            terminalFrame.localPosition = hasArt ? FrameHomePosition : captionOnlyFramePosition;
            terminalFrame.localScale = hasArt ? FrameHomeScale : captionOnlyFrameScale;
        }

        private void AdvanceFrame()
        {
            nextFrameAt = Time.unscaledTime + content.FrameSecs;
            art.sprite = content.FrameAt(frame++);
        }

        private IEnumerator CutOff()
        {
            yield return CrtTransition.CutOff(transform, FullScale, closeSecs);
            Hide();
        }
    }
}
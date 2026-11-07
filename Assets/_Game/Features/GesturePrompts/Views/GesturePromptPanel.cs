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
        [SerializeField, Min(0f)] private float bootSecs = 0.18f;
        [SerializeField, Min(0f)] private float closeSecs = 0.18f;

        private Vector3? fullScale;
        private GesturePromptContent content;
        private float closeAt;
        private float nextFrameAt;
        private int frame;
        private bool closing;

        public bool IsShowing => isActiveAndEnabled;

        public bool IsFree => !isActiveAndEnabled;

        public GestureKind? Kind => content != null ? content.Kind : null;

        public void Show(GesturePromptContent value, Vector3 position)
        {
            content = value;
            transform.position = position;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            StopAllCoroutines();
            closing = false;
            content = null;
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
            AdvanceFrame();

            string text = LocalizationUtil.GetValue(content.CaptionKey);
            caption.text = text;
            caption.gameObject.SetActive(!string.IsNullOrEmpty(text));
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
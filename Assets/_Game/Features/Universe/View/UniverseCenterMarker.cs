using UnityEngine;

namespace DigitalLove.Game.Universe
{
    public class UniverseCenterMarker : MonoBehaviour
    {
        [SerializeField] private UniverseView universeView;
        [SerializeField] private LineRenderer ring;
        [SerializeField] private float authoredRadius = 0.04f;
        [SerializeField] private float ringRadiusFactor = 0.4f;
        [SerializeField] private float heightOffset;
        [SerializeField] private Color normalColor = Color.magenta;
        [SerializeField] private Color activeColor = Color.cyan;
        [SerializeField] private Color blockedColor = new(1f, 0.35f, 0.2f, 1f);
        [SerializeField] private Color commitColor = Color.green;

        private MaterialPropertyBlock colorBlock;

        private void Awake() => colorBlock = new MaterialPropertyBlock();

        private void LateUpdate()
        {
            BubbleViewState state = universeView.State;
            ring.enabled = state.IsBubble;
            if (!state.IsBubble)
                return;

            transform.position = state.CenterWorld + Vector3.up * heightOffset;
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            float radius = state.Metrics.BubbleRadiusWorld * ringRadiusFactor;
            transform.localScale = Vector3.one * (radius / authoredRadius);

            UniverseMapFeedback feedback = universeView.MapFeedback;
            Color color = feedback.IsBlocked
                ? blockedColor
                : feedback.IsActive
                    ? activeColor
                    : Color.Lerp(normalColor, commitColor, feedback.ScaleFeedback);
            ring.startColor = color;
            ring.endColor = color;
            colorBlock.SetColor("_BaseColor", color);
            colorBlock.SetColor("_Color", color);
            ring.SetPropertyBlock(colorBlock);
        }
    }
}

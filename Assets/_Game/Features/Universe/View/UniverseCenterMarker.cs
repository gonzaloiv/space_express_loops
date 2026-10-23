using UnityEngine;

namespace DigitalLove.Game.Universe
{
    public class UniverseCenterMarker : MonoBehaviour
    {
        [SerializeField] private UniverseView universeView;
        [SerializeField] private LineRenderer ring;
        [SerializeField] private float authoredRadius = 0.04f;
        [SerializeField, Range(0.1f, 1.5f)] private float sizeFactor = 0.6f;
        [SerializeField] private Color normalColor = Color.magenta;
        [SerializeField] private Color activeColor = Color.cyan;
        [SerializeField] private Color commitColor = Color.green;

        private MaterialPropertyBlock colorBlock;

        private void Awake() => colorBlock = new MaterialPropertyBlock();

        private void LateUpdate()
        {
            BubbleViewState state = universeView.State;
            ring.enabled = state.IsBubble;
            if (!state.IsBubble)
                return;

            Vector3 radii = state.Metrics.ScopeRadii;
            float radius = Mathf.Max(radii.x, Mathf.Max(radii.y, radii.z)) * sizeFactor;
            transform.position = state.CenterWorld;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one * (radius / Mathf.Max(authoredRadius, 0.0001f));

            UniverseMapFeedback feedback = universeView.MapFeedback;
            Color color = feedback.IsActive
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

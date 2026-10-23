using DigitalLove.Input;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class CubeScaleController : MonoBehaviour
    {
        [SerializeField] private TwoHandGestureInput gestureInput;
        [SerializeField] private SpaceZoneView zone;
        [SerializeField] private float scaleGain = 2f;
        [SerializeField] private float minScale = 0.4f;
        [SerializeField] private float maxScale = 2.5f;

        private float startHandDistance;
        private float startScale;

        private void OnEnable() => gestureInput.GestureStarted += OnGestureStarted;

        private void OnDisable() => gestureInput.GestureStarted -= OnGestureStarted;

        private void Update()
        {
            if (!gestureInput.State.IsActive || startHandDistance < 0.0001f)
                return;

            float ratio = gestureInput.State.Distance / startHandDistance;
            float scale = Mathf.Clamp(startScale * Mathf.Pow(ratio, scaleGain), minScale, maxScale);
            zone.SetScale(scale);
        }

        private void OnGestureStarted(TwoHandGestureState state)
        {
            startHandDistance = state.Distance;
            startScale = zone.Scale;
        }
    }
}

using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class ZoneContentScaleListener : MonoBehaviour
    {
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private SpaceZoneView zone;
        [SerializeField] private float scaleGain = 2f;
        [SerializeField] private float minScale = 0.4f;
        [SerializeField] private float maxScale = 2.5f;

        private float startHandDistance;
        private float startScale;
        private int lastSelectingCount;

        private void Update()
        {
            int count = grabbable.SelectingPointsCount;
            if (count >= 2 && lastSelectingCount < 2)
            {
                startHandDistance = HandDistance();
                startScale = zone.Scale;
            }

            lastSelectingCount = count;

            if (count < 2 || startHandDistance < 0.0001f)
                return;

            float ratio = HandDistance() / startHandDistance;
            zone.SetScale(Mathf.Clamp(startScale * Mathf.Pow(ratio, scaleGain), minScale, maxScale));
        }

        private float HandDistance()
        {
            if (grabbable.GrabPoints.Count < 2)
                return 0f;

            return Vector3.Distance(grabbable.GrabPoints[0].position, grabbable.GrabPoints[1].position);
        }
    }
}

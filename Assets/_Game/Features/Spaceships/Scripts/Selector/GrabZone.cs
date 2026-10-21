using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class GrabZone : MonoBehaviour
    {
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private LineRenderer lineToHost;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private float verticalOffset = 0.02f;

        private Vector3 targetLocalCenter;
        private bool hasTarget;
        private float baseWidthMultiplier = 1f;

        private void Awake()
        {
            lineToHost.useWorldSpace = false;
            baseWidthMultiplier = lineToHost.widthMultiplier;
        }

        private void LateUpdate()
        {
            lineToHost.widthMultiplier = baseWidthMultiplier * contentRoot.lossyScale.x;

            if (hasTarget)
                Refresh();
        }

        public void SetColor(Color color)
        {
            foreach (Renderer renderer in renderers)
                renderer.material.color = color;
        }

        public void PointAt(Vector3 center)
        {
            targetLocalCenter = contentRoot.InverseTransformPoint(center);
            hasTarget = true;
            Refresh();
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        private void Refresh()
        {
            float scale = contentRoot.lossyScale.x;
            lineToHost.widthMultiplier = baseWidthMultiplier * scale;

            Vector3 start = lineToHost.transform.position;
            Vector3 bend = start - new Vector3(0, verticalOffset * scale, 0);
            Vector3 targetCenter = contentRoot.TransformPoint(targetLocalCenter);
            lineToHost.SetPosition(0, lineToHost.transform.InverseTransformPoint(start));
            lineToHost.SetPosition(1, lineToHost.transform.InverseTransformPoint(bend));
            lineToHost.SetPosition(2, lineToHost.transform.InverseTransformPoint(targetCenter));
            lineToHost.enabled = true;
        }
    }
}

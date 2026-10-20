using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class GrabZone : MonoBehaviour
    {
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private LineRenderer lineToHost;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private float verticalOffset = 0.02f;

        private Vector3 targetCenter;
        private bool hasTarget;
        private float baseWidthMultiplier = 1f;

        private void Awake()
        {
            if (contentRoot == null)
                contentRoot = transform.root;
            if (lineToHost != null)
            {
                lineToHost.useWorldSpace = false;
                baseWidthMultiplier = lineToHost.widthMultiplier;
            }
        }

        private void LateUpdate()
        {
            if (lineToHost == null)
                return;

            float scale = contentRoot != null ? contentRoot.lossyScale.x : 1f;
            lineToHost.widthMultiplier = baseWidthMultiplier * scale;

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
            targetCenter = center;
            hasTarget = true;
            Refresh();
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        private void Refresh()
        {
            if (lineToHost == null)
                return;

            float scale = contentRoot != null ? contentRoot.lossyScale.x : 1f;
            lineToHost.widthMultiplier = baseWidthMultiplier * scale;

            Vector3 start = lineToHost.transform.position;
            Vector3 bend = start - new Vector3(0, verticalOffset * scale, 0);
            lineToHost.SetPosition(0, lineToHost.transform.InverseTransformPoint(start));
            lineToHost.SetPosition(1, lineToHost.transform.InverseTransformPoint(bend));
            lineToHost.SetPosition(2, lineToHost.transform.InverseTransformPoint(targetCenter));
            lineToHost.enabled = true;
        }
    }
}

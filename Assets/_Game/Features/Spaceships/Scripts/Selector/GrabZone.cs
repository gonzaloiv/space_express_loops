using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class GrabZone : MonoBehaviour
    {
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private LineRenderer lineToHost;
        [SerializeField] private float verticalOffset = 0.02f;

        public void SetColor(Color color)
        {
            foreach (Renderer renderer in renderers)
                renderer.material.color = color;
        }

        public void PointAt(Vector3 center)
        {
            lineToHost.SetPosition(0, lineToHost.transform.position);
            lineToHost.SetPosition(1, lineToHost.transform.position - new Vector3(0, verticalOffset, 0));
            lineToHost.SetPosition(2, center);
            lineToHost.enabled = true;
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}

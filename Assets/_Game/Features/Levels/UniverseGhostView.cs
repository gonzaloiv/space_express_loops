using UnityEngine;

namespace DigitalLove.Game.Levels
{
    public class UniverseGhostView : MonoBehaviour
    {
        [SerializeField] private LineRenderer[] circles;
        [SerializeField] private float lineWidthWorld = 0.004f;

        public void Show(Vector3 center, float radius)
        {
            gameObject.SetActive(true);

            float safeRadius = Mathf.Max(radius, 0.01f);
            transform.position = center;
            transform.localScale = Vector3.one * safeRadius;

            float width = lineWidthWorld / safeRadius;
            for (int i = 0; i < circles.Length; i++)
                circles[i].widthMultiplier = width;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnValidate()
        {
            lineWidthWorld = Mathf.Max(0.0001f, lineWidthWorld);
        }
    }
}

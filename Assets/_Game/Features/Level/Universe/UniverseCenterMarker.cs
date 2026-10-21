using UnityEngine;

namespace DigitalLove.Game.Levels
{
    public class UniverseCenterMarker : MonoBehaviour
    {
        [SerializeField] private UniverseView universeView;
        [SerializeField] private LineRenderer ring;
        [SerializeField] private float authoredRadius = 0.04f;

        private void LateUpdate()
        {
            bool visible = universeView.IsMini;
            ring.enabled = visible;
            if (!visible)
                return;

            transform.position = universeView.CenterWorld;
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            transform.localScale = Vector3.one * (universeView.BubbleRadiusWorld / authoredRadius);
        }
    }
}

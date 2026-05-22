using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class GrabZone : MonoBehaviour
    {
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Transform link;

        public void SetColor(Color color)
        {
            foreach (Renderer renderer in renderers)
                renderer.material.color = color;
        }

        public void LookAtStationCenter(Vector3 stationCenter)
        {
            if (link == null)
                return;

            Vector3 toCenter = stationCenter - link.position;
            toCenter.y = 0f;
            if (toCenter.sqrMagnitude < 0.0001f)
                return;

            link.rotation = Quaternion.LookRotation(toCenter.normalized, Vector3.up);
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}

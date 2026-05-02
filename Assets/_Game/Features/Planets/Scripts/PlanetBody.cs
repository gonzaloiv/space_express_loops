using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class PlanetBody : MonoBehaviour
    {
        [SerializeField] private Transform body;

        public float RadiusOffset => body.lossyScale.x;
        public Vector3 Position => body.position;
        
        public void SetRadius(float radius)
        {
            body.localScale = new Vector3(radius, radius, radius);
        }
    }
}

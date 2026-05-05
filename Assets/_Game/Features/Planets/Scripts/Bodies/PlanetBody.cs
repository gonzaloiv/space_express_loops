using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class PlanetBody : MonoBehaviour
    {
        [SerializeField] private Transform body;
        [SerializeField] private PlanetBodyRenderer[] rends;

        private int currentBodyIndex = 0;

        public float RadiusOffset => body.lossyScale.x;
        public Vector3 Position => body.position;

        public void Init(float radius)
        {
            body.localScale = new Vector3(radius, radius, radius);
            if (rends.Length > 1)
            {
                currentBodyIndex = Random.Range(0, rends.Length);
                for (int i = 0; i < rends.Length; i++)
                {
                    rends[i].SetActive(i == currentBodyIndex);
                }
            }
        }

        public void SetColor(Vector2 offset)
        {
            rends[currentBodyIndex].SetColor(offset);
        }
    }
}

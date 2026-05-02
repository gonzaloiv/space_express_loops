using UnityEngine;
using System.Linq;

namespace DigitalLove.Game.Planets
{
    public class PlanetBaseBehaviour : MonoBehaviour
    {
        [SerializeField] private StationBehaviour[] stations;
        [SerializeField] private PlanetBody planetBody;

        public PlanetBody PlanetBody => planetBody;
        public bool HasAvailableStations => stations.Any(s => !s.IsTaken);
        public string Id
        {
            get
            {
                PlanetBehaviour origin = GetComponent<PlanetBehaviour>();
                return origin != null ? origin.Id : string.Empty;
            }
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public StationBehaviour GetValidStation()
        {
            StationBehaviour[] availableStations = stations.Where(s => !s.IsTaken).ToArray();
            StationBehaviour station = availableStations[Random.Range(0, availableStations.Length)];
            return station;
        }
    }
}
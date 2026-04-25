using System.Collections.Generic;
using DigitalLove.Global;
using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class PlanetsSpawner : MonoBehaviour
    {
        [SerializeField] private BasePlanetBehaviour basePlanet;
        [SerializeField] private List<PlanetBehaviour> planets;
        [SerializeField] private LayerMask planetsLayerMask;

        public BasePlanetBehaviour BasePlanet => basePlanet;

        public void Spawn(PlanetsSeed seed)
        {
            DisableAll();
            basePlanet.Init();
            SpawnPlanets(seed);
        }

        private void SpawnPlanets(PlanetsSeed seed)
        {
            int count = seed.count.GetRandomValue();
            for (int i = 0; i < count; i++)
            {
                if (i >= planets.Count)
                {
                    PlanetBehaviour planet = Instantiate(planets[0], transform);
                    planet.SetActive(false);
                    planets.Add(planet);
                }
                float radius = seed.planetSeed.radius.GetRandomValue();
                Vector3 position = GetValidPosition(radius, seed.planetSeed.distanceToBase);
                planets[i].Setup(radius, position);
            }
        }

        private Vector3 GetValidPosition(float radius, MinMaxFloat distanceToBase)
        {
            int maxIterations = 333;
            Vector3 result = Vector3.zero;
            for (int i = 0; i < maxIterations && result == Vector3.zero; i++)
            {
                Vector3? candidate = MRUK.Instance.GetCurrentRoom().GenerateRandomPositionInRoom(radius * 2, true);
                if (candidate.HasValue)
                {
                    float distance = Vector3.Distance(candidate.Value, basePlanet.transform.position);
                    if (distance > distanceToBase.min && distance < distanceToBase.max)
                    {
                        Debug.LogWarning($"Generated candidate position: {candidate.Value} iteration: {i}");
                        Collider[] colliders = Physics.OverlapSphere(candidate.Value, radius * 2.5f, planetsLayerMask);
                        if (colliders.Length == 0)
                            result = candidate.Value;
                    }
                }
            }
            return result;
        }

        private void DisableAll()
        {
            basePlanet.SetActive(false);
            foreach (PlanetBehaviour planet in planets)
                planet.SetActive(false);
        }

        public PlanetBehaviour GetRandom()
        {
            return planets[Random.Range(0, planets.Count)];
        }
    }
}
using System.Collections.Generic;
using System.Linq;
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

        private IdCreator idCreator = new();

        public BasePlanetBehaviour BasePlanet => basePlanet;

        public List<PlanetData> Spawn(PlanetsSeed seed)
        {
            HideAll();
            basePlanet.Spawn(idCreator.NextId);
            return SpawnPlanets(seed);
        }

        private List<PlanetData> SpawnPlanets(PlanetsSeed seed)
        {
            List<PlanetData> result = new();
            int count = seed.count.GetRandomValue();
            for (int i = 0; i < count; i++)
            {
                PlanetData planetData = CreateDataFromSeed(idCreator.NextId, seed);
                result.Add(planetData);
            }
            SpawnPlanets(result);
            return result;
        }

        private PlanetData CreateDataFromSeed(string id, PlanetsSeed seed)
        {
            float radius = seed.planetSeed.radius.GetRandomValue();
            Vector3 localPosition = GetValidPosition(radius, seed.planetSeed.distanceToBase);
            PlanetData planetData = new()
            {
                id = id,
                radius = radius,
                localPosition = SerializableVector3.FromVector3(localPosition),
                lettersPerMinute = seed.planetSeed.lettersPerMinute.GetRandomValue()
            };
            return planetData;
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
                        {
                            Vector3 localPos = transform.InverseTransformPoint(candidate.Value);
                            result = localPos;
                        }
                    }
                }
            }
            return result;
        }

        public void Spawn(List<PlanetData> planets, int currentLetters)
        {
            HideAll();
            basePlanet.Spawn(0.ToString(), currentLetters);
            SpawnPlanets(planets);
        }

        private void SpawnPlanets(List<PlanetData> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (i >= planets.Count)
                    Instantiate();
                planets[i].Spawn(data[i]);
            }
        }

        private void Instantiate()
        {
            PlanetBehaviour planet = Instantiate(planets[0], transform);
            planet.SetActive(false);
            planets.Add(planet);
        }

        public void HideAll()
        {
            basePlanet.SetActive(false);
            foreach (PlanetBehaviour planet in planets)
                planet.SetActive(false);
        }

        public PlanetBehaviour GetRandom()
        {
            return planets[Random.Range(0, planets.Count)];
        }

        public PlanetBehaviour GetById(string id)
        {
            return planets.FirstOrDefault(p => string.Equals(p.Id, id));
        }
    }
}
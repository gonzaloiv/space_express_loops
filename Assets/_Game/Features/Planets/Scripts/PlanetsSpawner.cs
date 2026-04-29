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

        private IdCounter idCreator = new();

        public BasePlanetBehaviour BasePlanet => basePlanet;

        public List<PlanetData> GeneratePlanetDataFromPlanetsSeed(PlanetsSeed seed)
        {
            List<PlanetData> result = new();
            int count = seed.count.GetRandomValue();
            for (int i = 0; i < count; i++)
            {
                PlanetData planetData = CreateDataFromSeed(idCreator.NextId, seed, result);
                result.Add(planetData);
            }
            return result;
        }

        private PlanetData CreateDataFromSeed(string id, PlanetsSeed seed, List<PlanetData> otherPlanets)
        {
            float radius = seed.planetSeed.radius.GetRandomValue();
            Vector3 localPosition = GetValidPosition(radius, seed.planetSeed.distanceToBase, otherPlanets);
            PlanetData planetData = new()
            {
                id = id,
                radius = radius,
                localPosition = SerializableVector3.FromVector3(localPosition),
                lettersPerMinute = seed.planetSeed.lettersPerMinute.GetRandomValue(),
                maxLetters = seed.planetSeed.maxLetters.GetRandomValue()
            };
            return planetData;
        }

        private Vector3 GetValidPosition(float radius, MinMaxFloat distanceToBase, List<PlanetData> otherPlanets)
        {
            int maxIterations = 333;
            Vector3 result = Vector3.zero;
            for (int i = 0; i < maxIterations && result == Vector3.zero; i++)
            {
                Vector3? candidate = MRUK.Instance.GetCurrentRoom().GenerateRandomPositionInRoom(radius, true);
                if (candidate.HasValue)
                {
                    float distance = Vector3.Distance(candidate.Value, basePlanet.transform.position);
                    if (distance > distanceToBase.min && distance < distanceToBase.max)
                    {
                        if (otherPlanets.Any(p => Vector3.Distance(p.localPosition.ToVector3(), candidate.Value) < radius * 2f))
                            continue;
                        Debug.LogWarning($"Generated candidate position: {candidate.Value} iteration: {i}");
                        Vector3 localPos = transform.InverseTransformPoint(candidate.Value);
                        result = localPos;
                    }
                }
            }
            return result;
        }

        public void SpawnBase(int letters, int maxLetters)
        {
            basePlanet.Spawn(idCreator.NextId);
            basePlanet.ShowLetters(letters, maxLetters);
        }

        public void SpawnPlanets(List<PlanetData> data)
        {
            Debug.LogWarning($"Spawning {data.Count} planets");
            for (int i = 0; i < data.Count; i++)
            {
                if (i >= planets.Count)
                    Instantiate();
                if (!string.IsNullOrEmpty(planets[i].Id) && planets[i].Id.Equals(data[i].id) && planets[i].IsActive)
                {
                    continue;
                }
                else
                {
                    planets[i].Spawn(data[i]);
                }
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

        public PlanetBehaviour GetRandom(List<string> excludedIds = null)
        {
            List<PlanetBehaviour> selection = excludedIds != null ? planets.Where(p => !excludedIds.Contains(p.Id)).ToList() : planets;
            return selection[Random.Range(0, selection.Count)];
        }

        public PlanetBehaviour GetById(string id)
        {
            return planets.FirstOrDefault(p => string.Equals(p.Id, id));
        }
    }
}
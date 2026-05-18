using System;
using System.Collections.Generic;
using System.Linq;
using DigitalLove.Global;
using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class PlanetsSpawner : MonoBehaviour
    {
        [SerializeField] private List<PlanetBehaviour> planets;
        [SerializeField] private LayerMask planetsLayerMask;

        private IdCounter idCreator = new();

        public List<PlanetBehaviour> All => planets;

        public Action planetFull = () => { };

        public void SyncIdsFromSnapshot(IEnumerable<string> existingIds) => idCreator.SyncFromExistingIds(existingIds);

        public List<PlanetData> GeneratePlanetDataFromPlanetsSeed(PlanetsSeed seed, List<PlanetData> initialPlanets)
        {
            List<PlanetData> roundPlanets = new();
            int count = seed.count.GetRandomValue();
            for (int i = 0; i < count; i++)
            {
                List<PlanetData> allPlanets = initialPlanets.Concat(roundPlanets).ToList();
                PlanetData planetData = CreateDataFromSeed(idCreator.NextId, seed, allPlanets);
                roundPlanets.Add(planetData);
            }
            return roundPlanets;
        }

        private PlanetData CreateDataFromSeed(string id, PlanetsSeed seed, List<PlanetData> allPlanets)
        {
            float radius = seed.planetSeed.radius.GetRandomValue();
            Vector3 localPosition = GetValidPosition(radius, seed.planetSeed.maxDistanceBetweenPlanets.value, allPlanets);
            int lettersPerMinute = seed.planetSeed.lettersPerMinute.GetRandomValue();
            int maxLetters = (int)(seed.planetSeed.maxLettersMultiplier.GetRandomValue() * lettersPerMinute);
            PlanetData planetData = new()
            {
                id = id,
                radius = radius,
                localPosition = SerializableVector3.FromVector3(localPosition),
                lettersPerMinute = lettersPerMinute,
                maxLetters = maxLetters
            };
            return planetData;
        }

        private Vector3 GetValidPosition(float radius, float maxDistanceBetweenPlanets, List<PlanetData> allPlanets)
        {
            int maxIterations = 333;
            Vector3 result = Vector3.zero;
            for (int i = 0; i < maxIterations && result == Vector3.zero; i++)
            {
                Vector3? candidate = MRUK.Instance.GetCurrentRoom().GenerateRandomPositionInRoom(radius, true);
                if (candidate.HasValue)
                {
                    float distance = Vector3.Distance(candidate.Value, transform.position);
                    if (distance > radius * 3 && distance < maxDistanceBetweenPlanets)
                    {
                        Vector3 localPos = transform.InverseTransformPoint(candidate.Value);
                        if (allPlanets.Any(p => Vector3.Distance(p.localPosition.ToVector3(), localPos) < radius + p.radius))
                            continue;
                        Debug.LogWarning($"Generated candidate position: {localPos} iteration: {i}");
                        result = localPos;
                    }
                }
            }
            if (result == Vector3.zero)
                Debug.LogWarning("Failed to find a valid planet position; defaulting to local origin.");
            return result;
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
                    planets[i].Spawn(data[i], OnPlanetFull);
                }
            }
        }

        private void OnPlanetFull() => planetFull.Invoke();

        private void Instantiate()
        {
            PlanetBehaviour planet = Instantiate(planets[0], transform);
            planet.SetActive(false);
            planets.Add(planet);
        }

        public void HideAll()
        {
            foreach (PlanetBehaviour planet in planets)
                planet.SetActive(false);
        }

        public PlanetBehaviour GetRandom(List<string> excludedIds = null)
        {
            List<PlanetBehaviour> selection = excludedIds != null ? planets.Where(p => !excludedIds.Contains(p.Id)).ToList() : planets;
            return selection[UnityEngine.Random.Range(0, selection.Count)];
        }

        public PlanetBehaviour GetById(string id)
        {
            return planets.FirstOrDefault(p => string.Equals(p.Id, id));
        }
    }
}
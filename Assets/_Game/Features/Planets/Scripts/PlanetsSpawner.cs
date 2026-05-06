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

        public Action<string> planetSetColorButtonClicked = id => { };

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
                        if (allPlanets.Any(p => Vector3.Distance(p.localPosition.ToVector3(), candidate.Value) < radius * 2f))
                            continue;
                        Debug.LogWarning($"Generated candidate position: {candidate.Value} iteration: {i}");
                        Vector3 localPos = transform.InverseTransformPoint(candidate.Value);
                        result = localPos;
                    }
                }
            }
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
                    planets[i].Spawn(data[i], OnPlanetSetColorButtonClicked);
                }
            }
        }

        private void OnPlanetSetColorButtonClicked(string id)
        {
            planetSetColorButtonClicked(id);
        }

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
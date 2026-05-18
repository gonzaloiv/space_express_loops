using System;
using System.Collections.Generic;
using System.Linq;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class PlanetsSpawner : MonoBehaviour
    {
        [SerializeField] private MrukRoomLocalPlacement roomPlacement;
        [SerializeField] private List<PlanetBehaviour> planets;
        [SerializeField] private LayerMask planetsLayerMask;

        private IdCounter idCreator = new();

        public List<PlanetBehaviour> All => planets;

        public Action planetFull = () => { };

        public void SyncIdsFromSnapshot(IEnumerable<string> existingIds) => idCreator.SyncFromExistingIds(existingIds);

        public List<PlanetData> GeneratePlanetDataFromPlanetsSeed(int planetsToAdd, PlanetSeedData seed)
        {
            List<PlanetData> roundPlanets = new();
            for (int i = 0; i < planetsToAdd; i++)
            {
                PlanetData planetData = CreateDataFromSeed(idCreator.NextId, seed);
                roundPlanets.Add(planetData);
            }
            return roundPlanets;
        }

        private PlanetData CreateDataFromSeed(string id, PlanetSeedData seed)
        {
            float radius = seed.radius.GetRandomValue();
            Vector3 localPosition = roomPlacement.GetValidLocalPosition(radius, seed.maxDistanceBetweenPlanets.value);
            roomPlacement.Register(localPosition, radius);
            int lettersPerMinute = seed.lettersPerMinute.GetRandomValue();
            int maxLetters = (int)(seed.maxLettersMultiplier.GetRandomValue() * lettersPerMinute);
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
using System;
using System.Collections.Generic;
using System.Linq;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public class PlanetsSpawner : MonoBehaviour
    {
        [SerializeField] private MrukRoomLocalPlacement roomPlacement;
        [SerializeField] private List<PlanetBehaviour> planets;

        private IdCounter idCreator = new();
        private PlanetHandlers handlers;

        public List<PlanetBehaviour> All => planets;

        public void SetHandlers(PlanetHandlers planetHandlers)
        {
            handlers = planetHandlers;
            ApplyHandlersToAll();
        }

        public void ClearHandlers()
        {
            handlers = default;
            ApplyHandlersToAll();
        }

        public void ResetPool()
        {
            InitializePool();
            HideAll();
        }

        public void InitializePool() =>
            planets.ForEachInPool(planet => planet.Initialize());

        public void SyncIdsFromSnapshot(IEnumerable<string> existingIds) =>
            idCreator.SyncFromExistingIds(existingIds);

        public List<PlanetData> GeneratePlanetDataFromPlanetsSeed(
            int planetsToAdd,
            PlanetSeedData seed,
            float distanceBetweenPlanetsMultiplier)
        {
            List<PlanetData> roundPlanets = new(planetsToAdd);
            for (int i = 0; i < planetsToAdd; i++)
                roundPlanets.Add(CreateDataFromSeed(idCreator.NextId, seed, distanceBetweenPlanetsMultiplier));
            return roundPlanets;
        }

        public void SpawnPlanets(List<PlanetData> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                PlanetBehaviour planet = planets.EnsureSlot(i, transform);
                if (IsAlreadySpawned(planet, data[i]))
                    continue;

                planet.Configure(handlers);
                planet.Spawn(data[i]);
            }
        }

        public void HideAll() =>
            planets.ForEachInPool(planet => planet.Hide());

        public void UnlockPlanetStores() =>
            planets.ForEachInPool(planet =>
            {
                if (planet.IsActive)
                    planet.BeginStoring();
            });

        public PlanetBehaviour GetRandom(List<string> excludedIds = null)
        {
            IEnumerable<PlanetBehaviour> selection = excludedIds != null
                ? planets.Where(p => !excludedIds.Contains(p.Id))
                : planets;
            List<PlanetBehaviour> list = selection.ToList();
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public PlanetBehaviour GetById(string id) =>
            planets.FirstOrDefault(p => string.Equals(p.Id, id));

        private void ApplyHandlersToAll() =>
            planets.ForEachInPool(planet => planet.Configure(handlers));

        private static bool IsAlreadySpawned(PlanetBehaviour planet, PlanetData data) =>
            !string.IsNullOrEmpty(planet.Id) && planet.Id.Equals(data.id) && planet.IsActive;

        private PlanetData CreateDataFromSeed(string id, PlanetSeedData seed, float distanceBetweenPlanetsMultiplier)
        {
            float radius = seed.radius.GetRandomValue();
            float maxDistanceToOtherPlanet = seed.maxDistanceToOtherPlanet.value * distanceBetweenPlanetsMultiplier;
            Vector3 localPosition = roomPlacement.GetValidLocalPosition(radius, maxDistanceToOtherPlanet);
            roomPlacement.Register(localPosition, radius);
            int lettersPerMinute = seed.lettersPerMinute.GetRandomValue();
            int maxLetters = (int)(seed.maxLettersMultiplier.GetRandomValue() * lettersPerMinute);
            return new PlanetData
            {
                id = id,
                radius = radius,
                localPosition = SerializableVector3.FromVector3(localPosition),
                lettersPerMinute = lettersPerMinute,
                maxLetters = maxLetters
            };
        }
    }
}

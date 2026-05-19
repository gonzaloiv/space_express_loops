using DigitalLove.Game.Planets;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using DigitalLove.Global;
using UnityEngine.Assertions;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipsSpawner : MonoBehaviour
    {
        [SerializeField] private List<SpaceshipBehaviour> spaceships;
        [SerializeField] private ColorIsAvailablePair[] colors;

        private IdCounter idCounter = new();

        public List<SpaceshipBehaviour> All => spaceships;

        public Action<LoopEventArgs> loopChanged = args => { };
        public Action<LoopEventArgs> loopEditionButtonClicked = args => { };
        public Action<LoopCompleteEventArgs> loopComplete = args => { };

        public void InitializePool()
        {
            foreach (SpaceshipBehaviour spaceship in spaceships)
            {
                if (spaceship != null)
                    spaceship.Initialize();
            }
        }

        public void SyncIdsFromSnapshot(IEnumerable<string> existingIds) => idCounter.SyncFromExistingIds(existingIds);

        public SpaceshipBehaviour GetActiveById(string id) =>
            spaceships.FirstOrDefault(s => s.IsActive && string.Equals(s.Id, id));

        public SpaceshipBehaviour SpawnNew(HubBehaviour basePlanet)
        {
            return SpawnSpaceship(idCounter.NextId, basePlanet);
        }

        private SpaceshipBehaviour SpawnSpaceship(string id, HubBehaviour basePlanet, string colorCode = null)
        {
            SpaceshipBehaviour spaceship = GetOrInstantiate();
            ColorIsAvailablePair colorPair = ResolveColorPair(colorCode);
            colorPair.isTaken = true;
            SpaceshipData data = new()
            {
                id = id,
                colorCode = colorPair.Code,
                hubId = basePlanet != null ? basePlanet.Id : null
            };
            spaceship.Spawn(data, colorPair.color.value, basePlanet);
            spaceship.SetOnLoopChanged(args => loopChanged(args));
            spaceship.SetOnLoopComplete(args => loopComplete(args));
            spaceship.SetOnLoopEditionButtonClicked(args => loopEditionButtonClicked(args));
            return spaceship;
        }

        private ColorIsAvailablePair ResolveColorPair(string colorCode)
        {
            if (!string.IsNullOrEmpty(colorCode))
            {
                ColorIsAvailablePair savedPair = GetColorPair(colorCode);
                if (savedPair != null)
                    return savedPair;
            }

            return GetRandomAvailableColorPair();
        }

        public bool TryGetRouteColor(string colorCode, out Color color)
        {
            ColorIsAvailablePair pair = GetColorPair(colorCode);
            if (pair?.color == null)
            {
                color = default;
                return false;
            }

            color = pair.color.value;
            return true;
        }

        private ColorIsAvailablePair GetColorPair(string colorCode)
        {
            return colors.FirstOrDefault(c => string.Equals(c.Code, colorCode, StringComparison.OrdinalIgnoreCase));
        }

        private SpaceshipBehaviour GetOrInstantiate()
        {
            SpaceshipBehaviour spaceship = spaceships.FirstOrDefault(s => !s.IsActive);
            if (spaceship == null)
            {
                spaceship = Instantiate(spaceships[0], transform);
                spaceship.Initialize();
                spaceships.Add(spaceship);
                spaceship.Hide();
            }

            return spaceship;
        }

        private ColorIsAvailablePair GetRandomAvailableColorPair()
        {
            ColorIsAvailablePair[] availableColors = colors.Where(c => !c.isTaken).ToArray();
            Assert.AreNotEqual(availableColors.Length, 0);
            return availableColors[UnityEngine.Random.Range(0, availableColors.Length)];
        }

        public void SpawnIdle(string id, HubBehaviour basePlanet, string colorCode)
        {
            SpawnSpaceship(id, basePlanet, colorCode);
        }

        public void SpawnFromLoop(string id, HubBehaviour basePlanet, IReadOnlyList<PlanetBehaviour> destinationPlanets, string colorCode)
        {
            SpaceshipBehaviour spaceship = SpawnSpaceship(id, basePlanet, colorCode);
            spaceship.SetRoute(destinationPlanets);
        }

        public void HideAll()
        {
            foreach (SpaceshipBehaviour spaceship in spaceships)
                spaceship.Hide();
        }

        // ! DEBUG

        public SpaceshipBehaviour GetRandomActive()
        {
            List<SpaceshipBehaviour> active = spaceships.Where(s => s != null && s.IsActive).ToList();
            if (active.Count == 0)
                return null;

            return active[UnityEngine.Random.Range(0, active.Count)];
        }

        public List<SpaceshipBehaviour> GetAll() => spaceships.ToList();
    }

    [Serializable]
    public class ColorIsAvailablePair
    {
        public ColorValue color;
        public bool isTaken;

        public string Code => color != null ? color.name.Replace("Color.", string.Empty) : string.Empty;
    }
}

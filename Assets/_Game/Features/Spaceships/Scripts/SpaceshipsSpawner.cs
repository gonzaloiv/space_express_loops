using DigitalLove.Game.Nodes;
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
        private LoopHandlers handlers;
        private ILoopPlanetAvailability planetAvailability;

        public List<SpaceshipBehaviour> All => spaceships;

        public void SetPlanetAvailability(ILoopPlanetAvailability availability)
        {
            planetAvailability = availability;
            foreach (SpaceshipBehaviour spaceship in spaceships)
            {
                if (spaceship != null)
                    WireHandlers(spaceship);
            }
        }

        public void SetHandlers(LoopHandlers handlers)
        {
            this.handlers = handlers;
            foreach (SpaceshipBehaviour spaceship in spaceships)
            {
                if (spaceship != null)
                    WireHandlers(spaceship);
            }
        }

        public void ClearHandlers()
        {
            handlers = default;
            foreach (SpaceshipBehaviour spaceship in spaceships)
            {
                if (spaceship != null)
                    WireHandlers(spaceship);
            }
        }

        public void ResetPool()
        {
            InitializePool();
            HideAll();
        }

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
            SpaceshipBehaviour spaceship = SpawnSpaceship(idCounter.NextId, basePlanet);
            spaceship.BeginIdle();
            return spaceship;
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
            WireHandlers(spaceship);
            return spaceship;
        }

        public void WireLoopHandlers(SpaceshipBehaviour spaceship) => WireHandlers(spaceship);

        private void WireHandlers(SpaceshipBehaviour spaceship) =>
            spaceship.Configure(handlers, planetAvailability);

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

        public void SpawnRestored(string id, HubBehaviour hub, IReadOnlyList<PlanetBehaviour> destinations, string colorCode)
        {
            SpaceshipBehaviour spaceship = SpawnSpaceship(id, hub, colorCode);
            if (destinations is { Count: > 0 })
                spaceship.SetRoute(destinations);
            else
                spaceship.BeginIdle();
        }

        public void HideAll()
        {
            foreach (SpaceshipBehaviour spaceship in spaceships)
                spaceship.Hide();
        }

        public void RefreshGrabbablesAtStation()
        {
            foreach (SpaceshipBehaviour spaceship in spaceships)
            {
                if (spaceship != null)
                    spaceship.RefreshGrabbableAtStation();
            }
        }

        #region Debug

        public SpaceshipBehaviour GetRandomActive()
        {
            List<SpaceshipBehaviour> active = spaceships.Where(s => s != null && s.IsActive).ToList();
            if (active.Count == 0)
                return null;

            return active[UnityEngine.Random.Range(0, active.Count)];
        }

        public List<SpaceshipBehaviour> GetAll() => spaceships.ToList();

        #endregion
    }

    [Serializable]
    public class ColorIsAvailablePair
    {
        public ColorValue color;
        public bool isTaken;

        public string Code => color != null ? color.name.Replace("Color.", string.Empty) : string.Empty;
    }
}

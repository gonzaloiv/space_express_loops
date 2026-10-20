using DigitalLove.Game.Nodes;
using UnityEngine;
using System;
using System.Collections.Generic;
using DigitalLove.Global;

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
            ResetColors();
            InitializePool();
            HideAll();
        }

        private void ResetColors()
        {
            if (colors == null)
                return;

            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i] != null)
                    colors[i].isTaken = false;
            }
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

        public SpaceshipBehaviour GetActiveById(string id)
        {
            for (int i = 0; i < spaceships.Count; i++)
            {
                SpaceshipBehaviour spaceship = spaceships[i];
                if (spaceship != null && spaceship.IsActive && string.Equals(spaceship.Id, id))
                    return spaceship;
            }

            return null;
        }

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
            if (colors == null)
                return null;

            for (int i = 0; i < colors.Length; i++)
            {
                ColorIsAvailablePair pair = colors[i];
                if (pair != null && string.Equals(pair.Code, colorCode, StringComparison.OrdinalIgnoreCase))
                    return pair;
            }

            return null;
        }

        private SpaceshipBehaviour GetOrInstantiate()
        {
            for (int i = 0; i < spaceships.Count; i++)
            {
                SpaceshipBehaviour pooled = spaceships[i];
                if (pooled != null && !pooled.IsActive)
                    return pooled;
            }

            SpaceshipBehaviour spaceship = Instantiate(spaceships[0], transform);
            spaceship.Initialize();
            spaceships.Add(spaceship);
            spaceship.Hide();
            return spaceship;
        }

        private ColorIsAvailablePair GetRandomAvailableColorPair()
        {
            if (colors == null || colors.Length == 0)
                throw new InvalidOperationException("SpaceshipsSpawner has no color pairs configured.");

            List<ColorIsAvailablePair> availableColors = new(colors.Length);
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i] != null && !colors[i].isTaken)
                    availableColors.Add(colors[i]);
            }

            if (availableColors.Count == 0)
            {
                Debug.LogError("[SpaceshipsSpawner] No available spaceship colors; reusing the first pair.");
                return colors[0];
            }

            return availableColors[UnityEngine.Random.Range(0, availableColors.Count)];
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
            for (int i = 0; i < spaceships.Count; i++)
            {
                SpaceshipBehaviour spaceship = spaceships[i];
                if (spaceship == null)
                    continue;

                if (spaceship.HasData)
                    ReleaseColor(spaceship.ColorCode);
                spaceship.Hide();
            }
        }

        private void ReleaseColor(string colorCode)
        {
            ColorIsAvailablePair pair = GetColorPair(colorCode);
            if (pair != null)
                pair.isTaken = false;
        }

        public void RefreshGrabbablesAtStation()
        {
            foreach (SpaceshipBehaviour spaceship in spaceships)
            {
                if (spaceship == null)
                    continue;

                spaceship.RefreshGrabbableAtStation();
                spaceship.RefreshGrabZoneTarget();
            }
        }

        public void SetViewTransition(bool isTransitioning)
        {
            foreach (SpaceshipBehaviour spaceship in spaceships)
            {
                if (spaceship != null && spaceship.IsActive)
                    spaceship.SetViewTransition(isTransitioning);
            }
        }

        #region Debug

        public SpaceshipBehaviour GetRandomActive()
        {
            SpaceshipBehaviour selected = null;
            int activeCount = 0;
            for (int i = 0; i < spaceships.Count; i++)
            {
                SpaceshipBehaviour spaceship = spaceships[i];
                if (spaceship == null || !spaceship.IsActive)
                    continue;

                activeCount++;
                if (UnityEngine.Random.Range(0, activeCount) == 0)
                    selected = spaceship;
            }

            return selected;
        }

        public List<SpaceshipBehaviour> GetAll() => spaceships;

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

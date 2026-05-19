using System.Collections.Generic;
using DigitalLove.Game.Planets;
using DigitalLove.Game.Spaceships;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.DebugActions
{
    [RequireComponent(typeof(EditorDebugContext))]
    public class SpaceshipRouteDebugActions : MonoBehaviour
    {
        private EditorDebugContext context;
        private readonly List<string> excludedIds = new();

        private void Awake() => context = GetComponent<EditorDebugContext>();

        [Button]
        public void Debug_CreateRandomRoute()
        {
            excludedIds.Clear();
            FillRoute(context.ResolveShipForRoute());
        }

        [Button]
        public void Debug_CreateMultiStopRoute()
        {
            excludedIds.Clear();
            SpaceshipBehaviour ship = context.ResolveShipForRoute();
            if (ship == null)
                return;

            List<PlanetBehaviour> stops = new();
            for (int i = 0; i < 3; i++)
            {
                PlanetBehaviour planet = context.GetRandomPlanetExcludingIds(excludedIds);
                if (planet == null)
                    break;
                excludedIds.Add(planet.Id);
                stops.Add(planet);
            }

            if (stops.Count == 0)
            {
                Debug.LogWarning("EditorDebug: No planets available for multi-stop route.");
                return;
            }

            context.PrepareShipForRoute(ship);
            ship.SetRoute(stops);
            ship.NotifyLoopChanged();
            context.Planets.UnlockPlanetStores();
            Debug.Log($"Ship {ship.Id}: {stops.Count}-stop route.");
        }

        [Button]
        public void Debug_FillRoutes()
        {
            excludedIds.Clear();
            foreach (SpaceshipBehaviour spaceship in context.Spaceships.GetAll())
                FillRoute(spaceship);
        }

        [Button]
        public void Debug_ClickEditionButtons()
        {
            foreach (SpaceshipBehaviour spaceship in context.Spaceships.GetAll())
            {
                if (spaceship.HasRoute)
                    spaceship.Debug_InvokeOnLoopEditionButtonClicked();
            }
        }

        private void FillRoute(SpaceshipBehaviour spaceship)
        {
            if (spaceship == null)
                return;

            PlanetBehaviour planet = context.GetRandomPlanetExcludingIds(excludedIds);
            if (planet == null)
                return;

            excludedIds.Add(planet.Id);
            context.PrepareShipForRoute(spaceship);
            spaceship.SetRoute(new[] { planet });
            spaceship.NotifyLoopChanged();
            context.Planets.UnlockPlanetStores();
        }
    }
}

using System;
using System.Collections.Generic;
using DigitalLove.Game.Planets;
using DigitalLove.Game.UI;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public enum SelectionConfirmResult
    {
        Rejected,
        StartedLoop,
        ExtendedLoop
    }

    public class SpaceshipLoop
    {
        private readonly MonoBehaviour coroutineHost;
        private readonly Transform shipTransform;
        private readonly DestinationSelector destinationSelector;
        private readonly StationBehaviour station;
        private readonly SplineContainerWrapper splineContainerWrapper;
        private readonly TravellerBehaviour traveller;
        private readonly float legDelay;

        private readonly List<PlanetBehaviour> destinations = new();
        private TravellerLoopRunner loopRunner;
        private Action<LoopCompleteEventArgs> onLoopComplete;

        public IReadOnlyList<PlanetBehaviour> Destinations => destinations;
        public bool HasDestinations => destinations.Count > 0;
        public HubBehaviour Hub => destinationSelector.Hub;
        public Vector3 RoutePanelAnchor => splineContainerWrapper.GetPanelAnchorPosition();

        public SpaceshipLoop(
            MonoBehaviour coroutineHost,
            Transform shipTransform,
            DestinationSelector destinationSelector,
            StationBehaviour station,
            SplineContainerWrapper splineContainerWrapper,
            TravellerBehaviour traveller,
            float legDelay)
        {
            this.coroutineHost = coroutineHost;
            this.shipTransform = shipTransform;
            this.destinationSelector = destinationSelector;
            this.station = station;
            this.splineContainerWrapper = splineContainerWrapper;
            this.traveller = traveller;
            this.legDelay = legDelay;
        }

        public void SetVisuals(string spaceshipId, Color color)
        {
            splineContainerWrapper.SetColor(color);
        }

        public void SetRoutePanelData(RoutePanel routePanel, string spaceshipId, Color color)
        {
            routePanel.SetData(spaceshipId, color);
        }

        public void SetOnLoopComplete(Action<LoopCompleteEventArgs> onLoopComplete)
        {
            this.onLoopComplete = onLoopComplete;
            loopRunner?.SetOnLoopIterationComplete(onLoopComplete);
        }

        public Transform GetSelectionOriginTransform()
        {
            return station != null && station.IsActive ? station.transform : Hub.transform;
        }

        public IEnumerable<string> GetExcludedPlanetIds()
        {
            foreach (PlanetBehaviour planet in destinations)
                yield return planet.Id;
        }

        public void BeginSelection()
        {
            destinationSelector.SetSelectionOrigin(GetSelectionOriginTransform());
            destinationSelector.SetExcludedPlanetIds(GetExcludedPlanetIds());
            destinationSelector.StartLookingForDestination(true);
        }

        public void EndSelection() => destinationSelector.StartLookingForDestination(false);

        public SelectionConfirmResult ConfirmSelection(PlanetBehaviour planet, Action onLoopChanged)
        {
            bool isFirstDestination = !HasDestinations;
            if (!TryAppendDestination(planet))
                return SelectionConfirmResult.Rejected;

            onLoopChanged?.Invoke();

            if (!isFirstDestination)
                RebuildRoute();

            return isFirstDestination
                ? SelectionConfirmResult.StartedLoop
                : SelectionConfirmResult.ExtendedLoop;
        }

        public bool TryAppendDestination(PlanetBehaviour planet)
        {
            if (planet == null || destinations.Contains(planet))
                return false;

            destinations.Add(planet);
            station.RepositionAt(planet);
            return true;
        }

        public void SetDestinations(IReadOnlyList<PlanetBehaviour> planets)
        {
            destinations.Clear();
            if (planets == null || planets.Count == 0)
                return;

            foreach (PlanetBehaviour planet in planets)
                destinations.Add(planet);

            station.RepositionAt(planets[planets.Count - 1]);
        }

        public void ClearDestinations()
        {
            destinations.Clear();
            station.Hide();
        }

        public void MoveShipToActiveStation()
        {
            if (station.IsActive)
                shipTransform.SetWorldPose(station.SpawnPose);
        }

        public void MoveShipToHub()
        {
            if (Hub != null)
                shipTransform.SetWorldPose(Hub.SpawnPose);
        }

        public void RebuildRoute()
        {
            splineContainerWrapper.BuildLoop(Hub, destinations);
            splineContainerWrapper.SetLineRendererActive(destinations.Count > 0);
        }

        public void SetLineRendererActive(bool active) => splineContainerWrapper.SetLineRendererActive(active);

        public void ResetVisuals()
        {
            StopTraveller();
            splineContainerWrapper.SetLineRendererActive(false);
        }

        public void StartTraveller(string spaceshipId, Func<LoopEventArgs> getLoopEventArgs)
        {
            loopRunner ??= new TravellerLoopRunner(coroutineHost, traveller, legDelay);
            loopRunner.SetOnLoopIterationComplete(onLoopComplete);
            loopRunner.StartLoop(
                () => splineContainerWrapper.Legs,
                spaceshipId,
                PickLettersAtStop,
                getLoopEventArgs);
        }

        public void StopTraveller() => loopRunner?.Stop();

        public List<string> GetDestinationIds()
        {
            List<string> ids = new(destinations.Count);
            foreach (PlanetBehaviour planet in destinations)
                ids.Add(planet.Id);
            return ids;
        }

        private int PickLettersAtStop(int stopIndex)
        {
            IReadOnlyList<RouteLegPath> legs = splineContainerWrapper.Legs;
            if (legs == null || stopIndex >= legs.Count)
                return 0;

            PlanetBehaviour planet = legs[stopIndex].PickupPlanet;
            return planet != null ? planet.PlanetStore.PickAllLetters() : 0;
        }
    }
}

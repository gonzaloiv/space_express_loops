using System;
using DigitalLove.Game.Nodes;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The endpoint views: one pool per role, painted from RouteEndpoints and placed where the
    // session puts each source.
    public sealed class EndpointPools
    {
        private readonly RouteEndpoints endpoints = new();
        private readonly ViewPool<FieldPlanetBehaviour> origins;
        private readonly ViewPool<FieldPlanetBehaviour> destinations;
        private readonly ViewPool<InterchangePlanetBehaviour> interchanges;
        private readonly Color[] pipColors = new Color[RouteEndpoints.MaxPips];

        private bool pipsColoured = true;
        private bool labelsVisible = true;

        public EndpointPools(FieldPlanetSet.EndpointPrototypes prototypes, Transform parent)
        {
            origins = new ViewPool<FieldPlanetBehaviour>(
                prototypes.origin, parent, "Origin", created => created.Initialize());
            destinations = new ViewPool<FieldPlanetBehaviour>(
                prototypes.destination, parent, "Destination", created => created.Initialize());
            interchanges = new ViewPool<InterchangePlanetBehaviour>(
                prototypes.interchange, parent, "Interchange", created => created.Initialize());
        }

        public RouteEndpoints Endpoints => endpoints;

        public FieldPlanetBehaviour PrimaryDestination { get; private set; }

        public void SetPlan(RoutePlan value) => endpoints.Build(value);

        public void SetPipsColoured(bool value) => pipsColoured = value;

        public void SetLabelsVisible(bool visible)
        {
            labelsVisible = visible;
            origins.ForEachFrom(0, view => view.SetLabelsVisible(visible));
            destinations.ForEachFrom(0, view => view.SetLabelsVisible(visible));
            interchanges.ForEachFrom(0, view => view.SetLabelsVisible(visible));
        }

        public void Apply(SpaceShapingSession session)
        {
            endpoints.Resolve(session);
            PrimaryDestination = null;
            int origin = 0;
            int destination = 0;
            int interchange = 0;
            for (int entry = 0; entry < endpoints.Count; entry++)
                ApplyEntry(session, entry, ref origin, ref destination, ref interchange);

            HideUnused(origins, origin);
            HideUnused(destinations, destination);
            HideUnused(interchanges, interchange);
        }

        public void SetStateDim(float amount) => ForEach(view => view.SetStateDim(amount));

        public void SetMapFade(Vector3 focus, float near, float far) =>
            ForEach(view => FieldPlanetFade.Apply(view, focus, near, far));

        public void ClearMapFade() => ForEach(view => FieldPlanetFade.Clear(view));

        public void Hide() => ForEach(view => view.Hide());

        public void Pulse(int source, Color danger)
        {
            Pulse(origins, source, danger);
            Pulse(destinations, source, danger);
            Pulse(interchanges, source, danger);
        }

        private void ApplyEntry(
            SpaceShapingSession session, int entry,
            ref int origin, ref int destination, ref int interchange)
        {
            RouteEndpointRole role = endpoints.RoleAt(entry);
            if (role == RouteEndpointRole.Interchange)
            {
                InterchangePlanetBehaviour shared = interchanges.EnsureSlot(interchange++);
                Spawn(shared, session, entry);
                PaintInterchange(shared, entry);
                TrackPrimary(entry, shared);
                return;
            }

            FieldPlanetBehaviour planet = role == RouteEndpointRole.Origin
                ? origins.EnsureSlot(origin++)
                : destinations.EnsureSlot(destination++);
            if (Spawn(planet, session, entry))
                planet.SetRouteColor(endpoints.ColorAt(entry, 0));
            TrackPrimary(entry, planet);
        }

        private bool Spawn(FieldPlanetBehaviour view, SpaceShapingSession session, int entry)
        {
            int source = endpoints.SourceAt(entry);
            if (source < 0)
            {
                view.Hide();
                return false;
            }

            view.SetLabelsVisible(labelsVisible);
            view.Spawn(
                session.GetPosition(source), session.GetScale(source),
                source, session.Name(source));
            return true;
        }

        private void PaintInterchange(InterchangePlanetBehaviour view, int entry)
        {
            int pips = endpoints.PipCount(entry);
            if (!pipsColoured)
            {
                view.SetNotches(pips);
                return;
            }

            for (int i = 0; i < pips; i++)
                pipColors[i] = endpoints.ColorAt(entry, i);
            view.SetPips(pips, pipColors);
        }

        private void TrackPrimary(int entry, FieldPlanetBehaviour view)
        {
            if (entry == endpoints.PrimaryDestination)
                PrimaryDestination = view;
        }

        private static void Pulse<T>(
            ViewPool<T> pool, int source, Color danger) where T : FieldPlanetBehaviour
        {
            for (int i = 0; i < pool.Views.Count; i++)
            {
                FieldPlanetBehaviour view = pool.Views[i];
                if (view.IsVisible && view.SourceIndex == source)
                    view.PlayRefusal(danger);
            }
        }

        private static void HideUnused<T>(ViewPool<T> pool, int count) where T : FieldPlanetBehaviour =>
            pool.ForEachFrom(count, view => view.Hide());

        private void ForEach(Action<FieldPlanetBehaviour> action)
        {
            origins.ForEachFrom(0, action);
            destinations.ForEachFrom(0, action);
            interchanges.ForEachFrom(0, action);
        }
    }
}

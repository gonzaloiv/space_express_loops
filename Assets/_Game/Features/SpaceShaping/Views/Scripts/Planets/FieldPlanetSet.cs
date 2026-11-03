using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The authored planet set the field frames: the sun, the route endpoints, and pooled sources.
    public class FieldPlanetSet
    {
        private readonly Prototypes prototypes;
        private readonly RouteColors routeColors;
        private readonly List<FieldPlanetBehaviour> sources = new();

        private Transform parent;
        private FieldPlanetBehaviour sunPlanet;
        private FieldPlanetBehaviour originPlanet;
        private FieldPlanetBehaviour destinationPlanet;

        public FieldPlanetSet(Prototypes prototypes, RouteColors routeColors)
        {
            this.prototypes = prototypes;
            this.routeColors = routeColors;
        }

        public void Initialize(Transform parent)
        {
            this.parent = parent;
            sunPlanet = Create(prototypes.Sun);
            originPlanet = Create(prototypes.Origin);
            destinationPlanet = Create(prototypes.Destination);
        }

        public void Apply(SpaceShapingSession session)
        {
            int origin = session.IndexOf(session.Origin);
            int destination = session.IndexOf(session.Destination);
            bool sunSpawned = SpawnSources(session, origin, destination);
            if (!sunSpawned)
                sunPlanet.Hide();

            Spawn(originPlanet, session, origin);
            Spawn(destinationPlanet, session, destination);
            ApplyRouteColors(origin, destination);
        }

        private bool SpawnSources(
            SpaceShapingSession session, int origin, int destination)
        {
            int slot = 0;
            bool sunSpawned = false;
            for (int i = 0; i < session.SourceCount; i++)
            {
                if (i == origin || i == destination)
                    continue;

                if (session.IsSun(i))
                {
                    sunPlanet.Spawn(session.GetPosition(i), session.GetScale(i), i, session.Name(i));
                    sunSpawned = true;
                    continue;
                }

                Source(slot++).Spawn(
                    session.GetPosition(i), session.GetScale(i), i, session.Name(i));
            }

            HideUnusedSources(slot);
            return sunSpawned;
        }

        private void HideUnusedSources(int count)
        {
            for (int i = count; i < sources.Count; i++)
                sources[i].Hide();
        }

        private void ApplyRouteColors(int origin, int destination)
        {
            if (origin >= 0)
                originPlanet.SetRouteColor(routeColors.Origin.value);
            if (destination >= 0)
                destinationPlanet.SetRouteColor(routeColors.Destination.value);
        }

        public readonly struct Prototypes
        {
            public readonly FieldPlanetBehaviour Source;
            public readonly FieldPlanetBehaviour Sun;
            public readonly FieldPlanetBehaviour Origin;
            public readonly FieldPlanetBehaviour Destination;

            public Prototypes(
                FieldPlanetBehaviour source, FieldPlanetBehaviour sun,
                FieldPlanetBehaviour origin, FieldPlanetBehaviour destination)
            {
                Source = source;
                Sun = sun;
                Origin = origin;
                Destination = destination;
            }
        }

        public readonly struct RouteColors
        {
            public readonly ColorValue Origin;
            public readonly ColorValue Destination;

            public RouteColors(ColorValue origin, ColorValue destination)
            {
                Origin = origin;
                Destination = destination;
            }
        }

        private FieldPlanetBehaviour Create(FieldPlanetBehaviour prototype)
        {
            FieldPlanetBehaviour planet = Object.Instantiate(prototype, parent);
            planet.Initialize();
            return planet;
        }

        private static void Spawn(
            FieldPlanetBehaviour planet, SpaceShapingSession session, int index)
        {
            if (index >= 0)
                planet.Spawn(session.GetPosition(index), session.GetScale(index), index, session.Name(index));
            else
                planet.Hide();
        }

        private FieldPlanetBehaviour Source(int index)
        {
            if (sources.Count == 0)
                sources.AcquireInactive(prototypes.Source, parent, created => created.Initialize());

            return sources.EnsureSlot(index, parent, created => created.Initialize());
        }
    }
}

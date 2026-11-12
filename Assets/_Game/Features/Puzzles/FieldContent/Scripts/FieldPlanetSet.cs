using System;
using DigitalLove.Game.Nodes;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The authored planet set the field frames: the sun, the route endpoints, and pooled sources.
    public class FieldPlanetSet
    {
        private readonly Prototypes prototypes;

        private Transform parent;
        private ViewPool<FieldPlanetBehaviour> sources;
        private FieldPlanetBehaviour sunPlanet;
        private EndpointPools endpoints;
        private SpaceShapingSession session;
        private RoutePlan plan = RoutePlan.Empty;
        private bool labelsVisible = true;

        public FieldPlanetSet(Prototypes prototypes) => this.prototypes = prototypes;

        public void SetRoutePlan(RoutePlan value)
        {
            plan = value;
            if (endpoints != null)
                endpoints.SetPlan(value);
        }

        public void SetPipsColoured(bool value) => endpoints.SetPipsColoured(value);

        public void Initialize(Transform parent)
        {
            this.parent = parent;
            sources = new ViewPool<FieldPlanetBehaviour>(
                prototypes.Source, parent, "Planet", created => created.Initialize());
            endpoints = new EndpointPools(prototypes.Endpoints, parent);
            endpoints.SetPlan(plan);
            sunPlanet = Create(prototypes.Sun);
        }

        public void SetLabelsVisible(bool visible)
        {
            labelsVisible = visible;
            sources.ForEachFrom(0, view => view.SetLabelsVisible(visible));
            sunPlanet.SetLabelsVisible(visible);
            endpoints.SetLabelsVisible(visible);
        }

        public Vector3 SunWorldPosition =>
            sunPlanet != null ? sunPlanet.transform.position : parent.position;

        public FieldPlanetBehaviour Destination => endpoints.PrimaryDestination;

        public void ShowSunLabel(string text) => ShowOnlySunLabel(text, false);

        public void ShowCentreLabel(string text) => ShowOnlySunLabel(text, true);

        public void SetSunLabelColor(Color color) => sunPlanet.SetLabelColor(color);
        public void HideSunLabel() => sunPlanet.SetLabelsVisible(false);

        public void SetEndpointMarksShown(bool shown) => endpoints.SetMarksShown(shown);


        public void SetStateDim(float amount)
        {
            sources.ForEachFrom(0, view => view.SetStateDim(amount));
            sunPlanet.SetStateDim(amount);
            endpoints.SetStateDim(amount);
        }

        private void ShowOnlySunLabel(string text, bool labelOnly)
        {
            sources.ForEachFrom(0, view => view.SetLabelsVisible(false));
            endpoints.SetLabelsVisible(false);
            if (labelOnly)
            {
                sunPlanet.transform.localPosition = Vector3.zero;
                sunPlanet.ShowLabelOnly(text);
                return;
            }

            sunPlanet.SetLabelsVisible(true);
            sunPlanet.SetLabel(text);
        }

        public void SetMapFade(Vector3 focus, float near, float far)
        {
            sources.ForEachFrom(0, view => FieldPlanetFade.Apply(view, focus, near, far));
            FieldPlanetFade.Apply(sunPlanet, focus, near, far);
            endpoints.SetMapFade(focus, near, far);
        }

        public void ClearMapFade()
        {
            sources.ForEachFrom(0, view => FieldPlanetFade.Clear(view));
            FieldPlanetFade.Clear(sunPlanet);
            endpoints.ClearMapFade();
        }

        public void Apply(SpaceShapingSession session)
        {
            this.session = session;
            endpoints.Apply(session);
            bool sunSpawned = SpawnSources(session);
            if (!sunSpawned)
                sunPlanet.Hide();
        }

        public void PulseEndpoint(Vector3 point, Color danger)
        {
            int source = session != null ? session.EndpointSource(point) : -1;
            if (source >= 0)
                endpoints.Pulse(source, danger);
        }

        public void Hide()
        {
            sources.ForEachFrom(0, view => view.Hide());
            sunPlanet.Hide();
            endpoints.Hide();
        }

        private bool SpawnSources(SpaceShapingSession session)
        {
            int slot = 0;
            bool sunSpawned = false;
            for (int i = 0; i < session.SourceCount; i++)
            {
                if (endpoints.Endpoints.IsEndpointSource(i))
                    continue;

                if (session.IsSun(i))
                {
                    sunPlanet.Spawn(
                        session.GetPosition(i), session.GetScale(i), i, session.Name(i));
                    sunSpawned = true;
                    continue;
                }

                FieldPlanetBehaviour planet = Source(slot++);
                planet.Spawn(
                    session.GetPosition(i), session.GetScale(i), i, session.Name(i));
                planet.ClearRouteColor();
            }

            HideUnusedSources(slot);
            return sunSpawned;
        }

        private void HideUnusedSources(int count) =>
            sources.ForEachFrom(count, view => view.Hide());

        [Serializable]
        public struct EndpointPrototypes
        {
            public FieldPlanetBehaviour origin;
            public FieldPlanetBehaviour destination;
            public InterchangePlanetBehaviour interchange;
        }

        public readonly struct Prototypes
        {
            public readonly FieldPlanetBehaviour Source;
            public readonly FieldPlanetBehaviour Sun;
            public readonly EndpointPrototypes Endpoints;

            public Prototypes(
                FieldPlanetBehaviour source, FieldPlanetBehaviour sun,
                EndpointPrototypes endpoints)
            {
                Source = source;
                Sun = sun;
                Endpoints = endpoints;
            }
        }

        private FieldPlanetBehaviour Create(FieldPlanetBehaviour prototype)
        {
            FieldPlanetBehaviour planet = UnityEngine.Object.Instantiate(prototype, parent);
            planet.Initialize();
            planet.SetLabelsVisible(labelsVisible);
            return planet;
        }

        private FieldPlanetBehaviour Source(int index)
        {
            FieldPlanetBehaviour source = sources.EnsureSlot(index);
            source.SetLabelsVisible(labelsVisible);
            return source;
        }
    }
}

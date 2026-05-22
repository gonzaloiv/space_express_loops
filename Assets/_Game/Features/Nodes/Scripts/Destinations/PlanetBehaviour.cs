using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    [RequireComponent(typeof(PlanetRefs))]
    public class PlanetBehaviour : MonoBehaviour, IRouteWorldNode
    {
        [SerializeField] private PlanetRefs refs;

        private PlanetRoutePresentation presentation;
        private PlanetHandlers handlers;
        private string id;

        public string Id => id;
        public bool IsDestination => presentation != null && presentation.IsDestination;
        public bool IsOnRoute => presentation != null && presentation.IsOnRoute;
        public bool IsActive => gameObject.activeInHierarchy;

        public PlanetStore PlanetStore => refs.planetStore;
        public NodeBody NodeBody => refs.nodeBody;
        public Vector3 Position => refs.nodeBody.Position;

        private void Awake()
        {
            if (refs == null)
                refs = GetComponent<PlanetRefs>();
        }

        public void Initialize()
        {
            if (presentation != null)
                return;

            presentation = new PlanetRoutePresentation(refs);
            presentation.ResetToDefault();
        }

        public void Configure(PlanetHandlers planetHandlers) => handlers = planetHandlers;

        public void ApplyRouteVisual(RouteVisualState state, Color? routeColor = null)
        {
            Initialize();
            presentation.Apply(state, routeColor);
        }

        public void ApplyRouteColor(Color color)
        {
            Initialize();
            presentation.Apply(RouteVisualState.OnRoute, color);
        }

        public void ResetRouteColor()
        {
            Initialize();
            presentation.ResetToDefault();
        }

        public void Spawn(PlanetData planetData)
        {
            Initialize();
            gameObject.SetActive(true);
            SetupFromData(planetData);
            SetupUI();
            refs.planetStore.PrepareStoring(
                refs.lettersPanel,
                planetData.lettersPerMinute,
                planetData.maxLetters,
                handlers.OnPlanetFull);
        }

        public void Hide()
        {
            id = string.Empty;
            ResetRouteColor();
            gameObject.SetActive(false);
        }

        public void BeginStoring() => refs.planetStore.BeginStoring();

        private void SetupFromData(PlanetData planetData)
        {
            id = planetData.id;
            transform.localPosition = planetData.localPosition.ToVector3();
            refs.nodeBody.Init(planetData.radius);
            presentation.ResetToDefault();
        }

        private void SetupUI()
        {
            refs.lettersPanel.Init(transform.position + transform.up * refs.nodeBody.Radius);
        }
    }

    public static class PlanetBehaviourPoolExtensions
    {
        public static void ForEachInPool(this IList<PlanetBehaviour> pool, Action<PlanetBehaviour> action)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null)
                    action(pool[i]);
            }
        }

        public static PlanetBehaviour EnsureSlot(this List<PlanetBehaviour> pool, int index, Transform parent)
        {
            while (index >= pool.Count)
                pool.GrowPoolEntry(parent);

            return pool[index];
        }

        private static void GrowPoolEntry(this List<PlanetBehaviour> pool, Transform parent)
        {
            PlanetBehaviour planet = UnityEngine.Object.Instantiate(pool[0], parent);
            planet.Initialize();
            planet.Hide();
            pool.Add(planet);
        }
    }
}

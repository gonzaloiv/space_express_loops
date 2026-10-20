using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    [RequireComponent(typeof(PlanetRefs))]
    public class PlanetBehaviour : MonoBehaviour, IRouteWorldNode, IPoolableNode
    {
        [SerializeField] private PlanetRefs refs;
        [SerializeField] private UniverseNodeHandle universeHandle;

        private PlanetRoutePresentation presentation;
        private PlanetHandlers handlers;
        private string id;

        public string Id => id;
        public bool IsDestination => presentation != null && presentation.IsDestination;
        public bool IsOnRoute => presentation != null && presentation.IsOnRoute;
        public bool IsActive => gameObject.activeInHierarchy;

        public PlanetStore PlanetStore => refs.PlanetStore;
        public NodeBody NodeBody => refs.NodeBody;
        public UniverseNodeHandle UniverseHandle => universeHandle;
        public Vector3 Position => refs.NodeBody.Position;

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
            refs.PlanetStore.PrepareStoring(
                refs.LettersPanel,
                planetData.lettersPerMinute,
                planetData.maxLetters,
                () => handlers.OnPlanetFull?.Invoke());
        }

        public void Hide()
        {
            id = string.Empty;
            ResetRouteColor();
            gameObject.SetActive(false);
        }

        public void BeginStoring() => refs.PlanetStore.BeginStoring();

        private void SetupFromData(PlanetData planetData)
        {
            id = planetData.id;
            transform.localPosition = planetData.localPosition.ToVector3();
            refs.NodeBody.Init(planetData.radius);
            presentation.ResetToDefault();
        }

        private void SetupUI()
        {
            refs.LettersPanel.Init(transform.position + transform.up * refs.NodeBody.Radius);
        }
    }
}

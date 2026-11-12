using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class FieldContentView : MonoBehaviour
    {
        [SerializeField] private Transform contentRoot;
        [SerializeField] private FieldPlanetBehaviour sourcePrototype;
        [SerializeField] private FieldPlanetBehaviour sunPrototype;
        [SerializeField] private FieldPlanetSet.EndpointPrototypes endpointPrototypes;
        [SerializeField] private Transform sourceParent;
        [SerializeField] private SpaceShapingSettings settings;
        [SerializeField] private float previewSize = 0.16f;
        [SerializeField] private bool labelsVisible;

        protected Transform ContentRoot => contentRoot;
        protected float PreviewSize => previewSize;

        public Vector3 SunWorldPosition => planets.SunWorldPosition;

        private FieldPlanetSet planets;

        protected virtual void Awake()
        {
            ClearGeneratedChildren(sourceParent);
            FieldPlanetSet.Prototypes prototypes = new(
                sourcePrototype, sunPrototype, endpointPrototypes);
            planets = new FieldPlanetSet(prototypes);
            planets.Initialize(sourceParent);
            planets.SetRouteColoured(false);
            planets.SetLabelsVisible(labelsVisible);
        }

        private static void ClearGeneratedChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject child = parent.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }

        public virtual void Render(PuzzleData puzzle)
        {
            if (puzzle == null)
            {
                planets.Hide();
                return;
            }

            SpaceShapingSession session = new(
                SpaceShapingSetup.Build(puzzle, PuzzleSimSettings.From(settings)));
            planets.SetRoutePlan(RoutePlan.Build(puzzle, settings.routePalette));
            planets.Apply(session);
            Fit(session);
        }

        public virtual void Hide() => planets.Hide();

        public void SetMapFade(Vector3 focus, float near, float far) =>
            planets.SetMapFade(focus, near, far);

        public void ClearMapFade() => planets.ClearMapFade();

        protected void SetEndpointMarksShown(bool shown) => planets.SetEndpointMarksShown(shown);

        protected void SetPlanetStateDim(float planetsDim, float sunDim) =>
            planets.SetStateDim(planetsDim, sunDim);

        protected virtual void Fit(SpaceShapingSession session)
        {
            BoundsBuilder builder = new();
            for (int i = 0; i < session.SourceCount; i++)
                builder.AddSphere(session.GetPosition(i), session.GetScale(i) * 0.5f);

            AddContentBounds(builder);
            builder.AddPoint(session.Origin);
            builder.AddPoint(session.Destination);
            if (!builder.TryGet(out Vector3 centre, out float extent))
            {
                ContentRoot.localScale = Vector3.one;
                ContentRoot.localPosition = Vector3.zero;
                return;
            }

            float scale = PreviewSize / Mathf.Max(extent * 2f, 0.0001f);
            ContentRoot.localScale = Vector3.one * scale;
            ContentRoot.localPosition = -centre * scale;
        }

        protected virtual void AddContentBounds(BoundsBuilder builder)
        {
        }
    }
}

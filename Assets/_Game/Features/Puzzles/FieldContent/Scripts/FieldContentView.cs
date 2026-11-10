using DigitalLove.Game.Nodes;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class FieldContentView : MonoBehaviour
    {
        [SerializeField] private Transform contentRoot;
        [SerializeField] private FieldPlanetBehaviour sourcePrototype;
        [SerializeField] private FieldPlanetBehaviour sunPrototype;
        [SerializeField] private FieldPlanetBehaviour originPrototype;
        [SerializeField] private FieldPlanetBehaviour destinationPrototype;
        [SerializeField] private Transform sourceParent;
        [SerializeField] private SpaceShapingSettings settings;
        [SerializeField] private float previewSize = 0.16f;
        [SerializeField] private bool labelsVisible;

        protected Transform ContentRoot => contentRoot;
        protected float PreviewSize => previewSize;
        protected float PreviewScale { get; private set; } = 1f;

        public string SunName { get; private set; } = string.Empty;

        public Vector3 SunWorldPosition => planets.SunWorldPosition;

        private FieldPlanetSet planets;

        protected virtual void Awake()
        {
            ClearGeneratedChildren(sourceParent);
            FieldPlanetSet.Prototypes prototypes = new(
                sourcePrototype, sunPrototype, originPrototype, destinationPrototype);
            planets = new FieldPlanetSet(prototypes, null);
            planets.Initialize(sourceParent);
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
            SunName = string.Empty;
            if (puzzle == null)
            {
                planets.Hide();
                return;
            }

            SpaceShapingSession session = new(
                SpaceShapingSetup.Build(puzzle, PuzzleSimSettings.From(settings)));
            SunName = FindSunName(session);
            planets.Apply(session);
            Fit(session);
        }

        private static string FindSunName(SpaceShapingSession session)
        {
            for (int i = 0; i < session.SourceCount; i++)
            {
                if (session.IsSun(i))
                    return session.Name(i);
            }

            return string.Empty;
        }

        public virtual void Hide() => planets.Hide();

        public void SetMapFade(Vector3 focus, float near, float far) =>
            planets.SetMapFade(focus, near, far);

        public void ClearMapFade() => planets.ClearMapFade();

        protected void SetSunLabel(string text) => planets.ShowSunLabel(text);

        protected void SetCentreLabel(string text) => planets.ShowCentreLabel(text);

        protected void SetSunLabelColor(Color color) => planets.SetSunLabelColor(color);

        protected void SetPlanetStateDim(float amount) => planets.SetStateDim(amount);

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
                PreviewScale = 1f;
                ContentRoot.localScale = Vector3.one;
                ContentRoot.localPosition = Vector3.zero;
                return;
            }

            float scale = PreviewSize / Mathf.Max(extent * 2f, 0.0001f);
            PreviewScale = scale;
            ContentRoot.localScale = Vector3.one * scale;
            ContentRoot.localPosition = -centre * scale;
        }

        protected virtual void AddContentBounds(BoundsBuilder builder)
        {
        }
    }
}

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

        protected Transform ContentRoot => contentRoot;
        protected float PreviewSize => previewSize;

        private FieldPlanetSet planets;

        protected virtual void Awake()
        {
            FieldPlanetSet.Prototypes prototypes = new(
                sourcePrototype, sunPrototype, originPrototype, destinationPrototype);
            planets = new FieldPlanetSet(prototypes, null);
            planets.Initialize(sourceParent);
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
            planets.Apply(session);
            Fit(session);
        }

        public virtual void Hide() => planets.Hide();

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

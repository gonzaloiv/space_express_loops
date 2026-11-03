using DigitalLove.Game.Nodes;
using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public class PuzzleDioramaView : MonoBehaviour
    {
        [SerializeField] private Transform contentRoot;
        [SerializeField] private FieldPlanetBehaviour sourcePrototype;
        [SerializeField] private FieldPlanetBehaviour sunPrototype;
        [SerializeField] private FieldPlanetBehaviour originPrototype;
        [SerializeField] private FieldPlanetBehaviour destinationPrototype;
        [SerializeField] private Transform sourceParent;
        [SerializeField] private Transform cubePrototype;
        [SerializeField] private Transform cubeParent;
        [SerializeField] private SpaceShapingSettings settings;
        [SerializeField] private float previewSize = 0.16f;

        private FieldPlanetSet planets;
        private ViewPool<Transform> cubes;

        private void Awake()
        {
            FieldPlanetSet.Prototypes prototypes = new(
                sourcePrototype, sunPrototype, originPrototype, destinationPrototype);
            planets = new FieldPlanetSet(prototypes, null);
            planets.Initialize(sourceParent);
            cubes = new ViewPool<Transform>(cubePrototype, cubeParent, "Cube", null);
        }

        public void Render(PuzzleData puzzle)
        {
            if (puzzle == null)
            {
                HideAll();
                return;
            }

            SpaceShapingSession session = Session(puzzle);
            planets.Apply(session);
            int cubeCount = RenderCubes(puzzle.frames);
            HideUnusedCubes(cubeCount);
            Fit(session, puzzle.frames);
        }

        private SpaceShapingSession Session(PuzzleData puzzle) =>
            new(SpaceShapingSetup.Build(puzzle, PuzzleSimSettings.From(settings)));

        private int RenderCubes(PuzzleFrame[] frames)
        {
            int count = frames != null ? frames.Length : 0;
            for (int i = 0; i < count; i++)
            {
                Transform cube = cubes.EnsureSlot(i);
                cube.gameObject.SetActive(true);
                cube.localPosition = frames[i].centre;
                cube.localScale = frames[i].BoxSize;
            }

            return count;
        }

        private void Fit(SpaceShapingSession session, PuzzleFrame[] frames)
        {
            if (!TryMeasure(session, frames, out Vector3 centre, out float extent))
            {
                contentRoot.localScale = Vector3.one;
                contentRoot.localPosition = Vector3.zero;
                return;
            }

            float scale = previewSize / Mathf.Max(extent * 2f, 0.0001f);
            contentRoot.localScale = Vector3.one * scale;
            contentRoot.localPosition = -centre * scale;
        }

        private static bool TryMeasure(
            SpaceShapingSession session, PuzzleFrame[] frames,
            out Vector3 centre, out float extent)
        {
            BoundsBuilder builder = new();
            for (int i = 0; i < session.SourceCount; i++)
                builder.AddSphere(session.GetPosition(i), session.GetScale(i) * 0.5f);

            int frameCount = frames != null ? frames.Length : 0;
            for (int i = 0; i < frameCount; i++)
                builder.AddSphere(frames[i].centre, frames[i].size * 0.5f);

            builder.AddPoint(session.Origin);
            builder.AddPoint(session.Destination);
            return builder.TryGet(out centre, out extent);
        }

        private void HideAll()
        {
            planets.Hide();
            HideUnusedCubes(0);
        }

        private void HideUnusedCubes(int count) =>
            cubes.ForEachFrom(count, view => view.gameObject.SetActive(false));
    }
}

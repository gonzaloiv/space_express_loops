using System.Collections.Generic;
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
            Bounds bounds = default;
            bool any = false;
            for (int i = 0; i < session.SourceCount; i++)
                AddSource(ref bounds, session.GetPosition(i), session.GetScale(i), ref any);

            int frameCount = frames != null ? frames.Length : 0;
            for (int i = 0; i < frameCount; i++)
                AddFrame(ref bounds, frames[i], ref any);

            AddPoint(ref bounds, session.Origin, ref any);
            AddPoint(ref bounds, session.Destination, ref any);
            centre = bounds.center;
            extent = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) * 0.5f;
            return any;
        }

        private static void AddSource(
            ref Bounds bounds, Vector3 position, float scale, ref bool any)
        {
            float radius = scale * 0.5f;
            AddPoint(ref bounds, position, ref any);
            bounds.Encapsulate(position + Vector3.one * radius);
            bounds.Encapsulate(position - Vector3.one * radius);
        }

        private static bool AddPoint(ref Bounds bounds, Vector3 point, ref bool any)
        {
            if (!any)
            {
                bounds = new Bounds(point, Vector3.zero);
                any = true;
            }
            else
            {
                bounds.Encapsulate(point);
            }

            return true;
        }

        private static void AddFrame(ref Bounds bounds, PuzzleFrame frame, ref bool any)
        {
            float radius = frame.size * 0.5f;
            AddPoint(ref bounds, frame.centre, ref any);
            bounds.Encapsulate(frame.centre + Vector3.one * radius);
            bounds.Encapsulate(frame.centre - Vector3.one * radius);
        }

        private void HideAll()
        {
            planets.Hide();
            HideUnusedCubes(0);
        }

        private void HideUnusedCubes(int count)
        {
            IReadOnlyList<Transform> views = cubes.Views;
            for (int i = count; i < views.Count; i++)
                views[i].gameObject.SetActive(false);
        }
    }
}

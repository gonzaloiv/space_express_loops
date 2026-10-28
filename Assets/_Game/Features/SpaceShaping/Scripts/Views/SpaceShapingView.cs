using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingView : MonoBehaviour
    {
        [SerializeField] private FieldPlanetBehaviour prototype;
        [SerializeField] private Transform sourceParent;
        [SerializeField] private SpacetimeMeshView mesh;
        [SerializeField] private TrajectoryLineView trajectory;
        [SerializeField] private SpaceZoneView cubePrototype;
        [SerializeField] private Transform cubeParent;

        private readonly List<FieldPlanetBehaviour> pool = new();
        private readonly List<SpaceZoneView> fixedCubes = new();
        private SpaceZoneView cube;
        private SpaceZone lastZone;
        private bool hasSnapshot;

        public IReadOnlyList<SpaceZoneView> FixedCubes => fixedCubes;

        public SpaceZone Zone => Cube().Zone;

        public bool IsScaling => Cube().IsScaling;

        public bool HasChanged()
        {
            if (!hasSnapshot)
                return true;

            SpaceZone current = Cube().Zone;
            return current.Centre != lastZone.Centre
                || current.HalfExtents != lastZone.HalfExtents
                || current.Scale != lastZone.Scale
                || current.Falloff != lastZone.Falloff;
        }

        public void ApplyFromSession(SpaceShapingSession session)
        {
            int count = session.SourceCount;
            for (int i = 0; i < count; i++)
                Source(i).Spawn(session.GetPosition(i), session.GetScale(i) * 0.5f, i);

            for (int i = count; i < pool.Count; i++)
                pool[i].Hide();
        }

        public void SetConditions(ZoneConditions conditions) => Cube().SetConditions(conditions);

        public void SetZoneFrame(Vector3 centre, Vector3 size, float falloff)
        {
            Cube().SetFrame(centre, size);
            Cube().SetFalloff(falloff);
        }

        public void SetZoneScale(float scale) => Cube().SetScale(scale);

        public void SetPrediction(bool solved, TrajectoryOutcome outcome) =>
            Cube().SetPrediction(solved, outcome);

        public void Snapshot()
        {
            lastZone = Cube().Zone;
            hasSnapshot = true;
        }

        public void Render(
            SpacetimeField field, IReadOnlyList<SpaceZone> zones, Vector3 origin, Vector3 destination,
            float crossHalf) =>
            mesh.Render(field, zones, origin, destination, crossHalf);

        public void ShowTrajectory(TrajectoryResult result, bool solved) => trajectory.Show(result, solved);

        public void Refuse() => Cube().Refuse();

        public void PulseTrajectory() => trajectory.Pulse();

        public void ResetZone()
        {
            Cube().ResetScale();
            hasSnapshot = false;
        }

        public void ShowFixedZones(PuzzleZone[] zones, float falloff)
        {
            if (cube != null)
                cube.gameObject.SetActive(false);

            for (int i = 0; i < zones.Length; i++)
            {
                SpaceZoneView cube = FixedCube(i);
                cube.gameObject.SetActive(true);
                cube.SetConditions(new ZoneConditions(false, false));
                cube.SetFrame(zones[i].centre, zones[i].halfExtents * 2f);
                cube.SetFalloff(zones[i].falloff > 0f ? zones[i].falloff : falloff);
                cube.SetScale(1f);
                cube.SetPrediction(false, TrajectoryOutcome.Miss);
            }

            for (int i = zones.Length; i < fixedCubes.Count; i++)
                fixedCubes[i].gameObject.SetActive(false);
        }

        public void HideFixedZones()
        {
            for (int i = 0; i < fixedCubes.Count; i++)
                fixedCubes[i].gameObject.SetActive(false);
        }

        public void ResetFixedZones()
        {
            for (int i = 0; i < fixedCubes.Count; i++)
                fixedCubes[i].ResetScale();
        }

        public void SetFixedPrediction(bool solved, TrajectoryOutcome outcome)
        {
            for (int i = 0; i < fixedCubes.Count; i++)
            {
                if (fixedCubes[i].gameObject.activeSelf)
                    fixedCubes[i].SetPrediction(solved, outcome);
            }
        }

        public float CrossHalf(SpaceShapingSession session, PuzzleData puzzle, float fallbackCube)
        {
            Vector3 run = session.Destination - session.Origin;
            float length = run.magnitude;
            Vector3 direction = length > 0.0001f ? run / length : Vector3.forward;
            float half = 0f;
            for (int i = 0; i < session.SourceCount; i++)
            {
                Vector3 offset = session.GetPosition(i) - session.Origin;
                Vector3 perpendicular = offset - direction * Vector3.Dot(offset, direction);
                half = Mathf.Max(half, perpendicular.magnitude + session.GetScale(i) * 0.5f);
            }

            if (puzzle.fixedZones != null && puzzle.fixedZones.Length > 0)
            {
                for (int i = 0; i < puzzle.fixedZones.Length; i++)
                {
                    Vector3 extent = puzzle.fixedZones[i].halfExtents;
                    half = Mathf.Max(half, Mathf.Max(extent.x, Mathf.Max(extent.y, extent.z)));
                }

                return half;
            }

            float cube = puzzle.cubeSize > 0f ? puzzle.cubeSize : fallbackCube;
            return Mathf.Max(half, cube * 0.5f);
        }

        private SpaceZoneView FixedCube(int index)
        {
            while (fixedCubes.Count <= index)
                fixedCubes.Add(Instantiate(cubePrototype, cubeParent));

            return fixedCubes[index];
        }

        private SpaceZoneView Cube()
        {
            if (cube == null && Application.isPlaying)
                cube = Instantiate(cubePrototype, cubeParent);

            if (cube != null)
                cube.gameObject.SetActive(true);

            return cube;
        }

        private FieldPlanetBehaviour Source(int index)
        {
            if (pool.Count == 0)
                pool.AcquireInactive(prototype, sourceParent, created => created.Initialize());

            return pool.EnsureSlot(index, sourceParent, created => created.Initialize());
        }
    }
}

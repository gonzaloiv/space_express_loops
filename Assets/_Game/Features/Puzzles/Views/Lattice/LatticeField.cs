using System;
using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping.Grid;
using DigitalLove.Game.Spatial;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The session's field as the lattice and the dust draw it: native copies of the zones, the wells'
    // sources and baseline, and the planets' fade spheres, refreshed each render; the warp and the fade
    // run on Burst over them. The owner disposes it.
    public sealed class LatticeField : ILatticeField, IDisposable
    {
        private const int Batch = 64;

        private NativeArray<SpaceZone> zones;
        private NativeArray<SpaceSource> sources;
        private NativeArray<SpaceSource> baseline;
        private NativeArray<Vector4> spheres;
        private ContentFade fade;

        public bool IsReady { get; private set; }

        public LatticeWarp Warp { get; private set; }

        public void Prepare(SpaceShapingSession session, LatticeWells wells, float fadeNear, float fadeFar)
        {
            bool fresh = Fit(ref sources, session.SourceCount);
            Fit(ref baseline, session.SourceCount);
            Fit(ref spheres, session.SourceCount);
            Fit(ref zones, session.Zones.Count);
            CopyZones(session.Zones);
            CopySpheres(session);
            wells.Fill(session, sources, baseline, fresh);
            Warp = new LatticeWarp(zones.AsReadOnly(), wells.Pull(sources, baseline));
            fade = new ContentFade(
                session.Origin, session.Destination, spheres.AsReadOnly(), fadeNear, fadeFar);
            IsReady = true;
        }

        public JobHandle ScheduleWarp(
            NativeArray<Vector3> rest, NativeArray<Vector3> warped, JobHandle dependsOn) =>
            new LatticeWarpJob { Warp = Warp, Rest = rest, Warped = warped }
                .Schedule(rest.Length, Batch, dependsOn);

        public JobHandle ScheduleFade(
            NativeArray<Vector3> warped, NativeArray<float> alpha, JobHandle dependsOn) =>
            new LatticeFadeJob { Fade = fade, Warped = warped, Alpha = alpha }
                .Schedule(warped.Length, Batch, dependsOn);

        public void Dispose()
        {
            IsReady = false;
            Release(ref zones);
            Release(ref sources);
            Release(ref baseline);
            Release(ref spheres);
        }

        private void CopyZones(IReadOnlyList<SpaceZone> source)
        {
            for (int i = 0; i < zones.Length; i++)
                zones[i] = source[i];
        }

        private void CopySpheres(SpaceShapingSession session)
        {
            for (int i = 0; i < spheres.Length; i++)
            {
                Vector3 centre = session.GetPosition(i);
                spheres[i] = new Vector4(centre.x, centre.y, centre.z, session.GetScale(i) * 0.5f);
            }
        }

        // Keeps a buffer at the length the session needs; true when it had to be (re)made.
        private static bool Fit<T>(ref NativeArray<T> buffer, int length) where T : struct
        {
            if (buffer.IsCreated && buffer.Length == length)
                return false;

            Release(ref buffer);
            buffer = new NativeArray<T>(length, Allocator.Persistent);
            return true;
        }

        private static void Release<T>(ref NativeArray<T> buffer) where T : struct
        {
            if (buffer.IsCreated)
                buffer.Dispose();
            buffer = default;
        }
    }
}

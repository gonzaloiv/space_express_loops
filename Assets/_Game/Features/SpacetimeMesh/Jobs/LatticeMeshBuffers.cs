using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Grid
{
    // The lattice's native working set and its job chain: rest nodes → the field's warp → smoothing and
    // strain per line → the field's fade → tube rings → bounds. One render runs the chain to completion.
    public sealed class LatticeMeshBuffers : IDisposable
    {
        private const int RingBatch = 64;
        private const int LineBatch = 4;

        private readonly int sides;
        private readonly int samplesPerLine;
        private readonly int ringCount;
        private NativeArray<Vector3> fractions;
        private NativeArray<Vector3> ringOffsets;
        private NativeArray<Vector3> rest;
        private NativeArray<Vector3> warped;
        private NativeArray<Vector3> scratch;
        private NativeArray<float> strains;
        private NativeArray<float> alpha;
        private NativeArray<Vector3> vertices;
        private NativeArray<Color> colors;
        private NativeArray<Vector3> minMax;

        public LatticeMeshBuffers(int vertexCount, int gridCount, int sides)
        {
            this.sides = sides;
            ringCount = vertexCount / sides;
            samplesPerLine = vertexCount / (3 * gridCount * gridCount * sides);
            fractions = Allocate<Vector3>(ringCount);
            ringOffsets = Allocate<Vector3>(vertexCount);
            rest = Allocate<Vector3>(ringCount);
            warped = Allocate<Vector3>(ringCount);
            scratch = Allocate<Vector3>(ringCount);
            strains = Allocate<float>(ringCount);
            alpha = Allocate<float>(ringCount);
            vertices = Allocate<Vector3>(vertexCount);
            colors = Allocate<Color>(vertexCount);
            minMax = Allocate<Vector3>(2);
            new LatticeLayout(gridCount, sides, samplesPerLine).Fill(fractions, ringOffsets);
        }

        public int VertexCount => vertices.Length;

        public void Run(ILatticeField field, LatticePass pass)
        {
            JobHandle handle = ScheduleRest(pass);
            handle = field.ScheduleWarp(rest, warped, handle);
            handle = ScheduleLines(pass, handle);
            handle = field.ScheduleFade(warped, alpha, handle);
            handle = ScheduleRings(pass, handle);
            handle = new LatticeBoundsJob { Vertices = vertices, MinMax = minMax }.Schedule(handle);
            handle.Complete();
        }

        public void WriteTo(Mesh mesh)
        {
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            Bounds bounds = new();
            bounds.SetMinMax(minMax[0], minMax[1]);
            mesh.bounds = bounds;
        }

        public void Dispose()
        {
            fractions.Dispose();
            ringOffsets.Dispose();
            rest.Dispose();
            warped.Dispose();
            scratch.Dispose();
            strains.Dispose();
            alpha.Dispose();
            vertices.Dispose();
            colors.Dispose();
            minMax.Dispose();
        }

        private JobHandle ScheduleRest(LatticePass pass) =>
            new LatticeRestJob
            {
                Fractions = fractions,
                Rest = rest,
                LocalToParent = pass.LocalToParent,
                Half = pass.Half,
            }.Schedule(ringCount, RingBatch);

        private JobHandle ScheduleLines(LatticePass pass, JobHandle dependsOn) =>
            new LatticeLineJob
            {
                Rest = rest,
                Warped = warped,
                Scratch = scratch,
                Strains = strains,
                SamplesPerLine = samplesPerLine,
                SmoothPasses = pass.SmoothPasses,
                SqueezeAt = pass.Look.SqueezeAt,
                StretchAt = pass.Look.StretchAt,
            }.Schedule(ringCount / samplesPerLine, LineBatch, dependsOn);

        private JobHandle ScheduleRings(LatticePass pass, JobHandle dependsOn) =>
            new LatticeRingJob
            {
                Warped = warped,
                Strains = strains,
                Alpha = alpha,
                RingOffsets = ringOffsets,
                Vertices = vertices,
                Colors = colors,
                ParentToLocal = pass.ParentToLocal,
                Look = pass.Look,
                Radius = pass.Radius,
                Sides = sides,
            }.Schedule(ringCount, RingBatch, dependsOn);

        private static NativeArray<T> Allocate<T>(int length) where T : struct =>
            new(length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
    }
}

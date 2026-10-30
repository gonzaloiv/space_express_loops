using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // Slice 2 of the multi-ship spike: over K generated fields, sweep the candidate zones and record the
    // best rescued-pair count, with the runtime budget and with the budget held fixed. The gap between
    // the two is the share of rescues that are field-driven rather than bought by time dilation.
    public static class PuzzleShipSweep
    {
        private const int ScaleSamples = 4;

        public static PuzzleShipSweepReport Run(
            PuzzleSeed seed, SpaceShapingSettings settings, int fields, float clearance)
        {
            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            PuzzleBrief brief = PuzzleBrief.From(seed, settings);
            PuzzleShipSweepReport report = new() { Fields = fields };

            for (int i = 0; i < fields; i++)
            {
                PuzzleData field = Field(seed, settings, sim, brief, seed.seed + i);
                if (field == null)
                    continue;

                report.Baked++;
                Accumulate(ref report, Best(field, sim, clearance));
                Object.DestroyImmediate(field);
            }

            return report;
        }

        private static FieldResult Best(PuzzleData field, PuzzleSimSettings sim, float clearance)
        {
            PuzzleShipScan scan = new(field, sim, clearance);
            Vector3[] centres = PuzzleZoneCandidates.Centres(field, field.cubeSizeMax);
            float[] sizes = PuzzleZoneCandidates.Sizes(field);
            float[] scales = PuzzleZoneCandidates.Scales(sim, ScaleSamples);
            FieldResult best = default;

            for (int c = 0; c < centres.Length; c++)
            {
                for (int s = 0; s < sizes.Length; s++)
                {
                    for (int k = 0; k < scales.Length; k++)
                        Consider(scan, field, sim, centres[c], sizes[s], scales[k], ref best);
                }
            }

            return best;
        }

        private static void Consider(
            PuzzleShipScan scan, PuzzleData field, PuzzleSimSettings sim, Vector3 centre, float size,
            float scale, ref FieldResult best)
        {
            PuzzleZone zone = PuzzleZoneCandidates.Zone(centre, size, scale, sim.ZoneFalloff);
            if (!PuzzleZoneCandidates.ContainsSource(zone, field))
                return;

            List<PuzzleShipRescue> rescued = scan.Rescued(zone);
            if (rescued.Count > best.Count)
                best.Count = rescued.Count;

            for (int i = 0; i < rescued.Count; i++)
            {
                if (rescued[i].Margin > best.Margin)
                    best.Margin = rescued[i].Margin;
            }

            if (rescued.Count < 2)
                return;

            int fixedCount = scan.RescuedFixed(zone).Count;
            if (fixedCount > best.FixedCount)
                best.FixedCount = fixedCount;
        }

        private static void Accumulate(ref PuzzleShipSweepReport report, FieldResult result)
        {
            if (result.Count > report.BestCount)
                report.BestCount = result.Count;

            if (result.FixedCount > report.BestFixedCount)
                report.BestFixedCount = result.FixedCount;

            if (result.Margin > report.BestMargin)
                report.BestMargin = result.Margin;

            if (result.Count >= 2)
                report.FieldsWithTwo++;

            if (result.FixedCount >= 2)
                report.FieldsWithTwoFixed++;
        }

        private static PuzzleData Field(
            PuzzleSeed seed, SpaceShapingSettings settings, PuzzleSimSettings sim, PuzzleBrief brief,
            int seedValue)
        {
            PuzzleSeed probe = Object.Instantiate(seed);
            probe.seed = seedValue;
            PuzzleShapeAsset shape = PuzzleArtifactFactory.Shape(probe, settings);
            PuzzlePlanetListAsset planets = PuzzleArtifactFactory.Planets(shape);
            PuzzleData field = ScriptableObject.CreateInstance<PuzzleData>();
            bool baked = PuzzleFieldBake.Apply(brief, field, planets, sim);
            Object.DestroyImmediate(planets);
            Object.DestroyImmediate(shape);
            Object.DestroyImmediate(probe);
            if (baked)
                return field;

            Object.DestroyImmediate(field);
            return null;
        }

        private struct FieldResult
        {
            public int Count;
            public int FixedCount;
            public float Margin;
        }
    }
}

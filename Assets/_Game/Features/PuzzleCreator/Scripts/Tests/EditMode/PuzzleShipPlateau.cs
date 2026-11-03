using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // Slice 2b of the multi-ship spike: for each field, the widest range of zone scales that keeps two
    // or more field-driven rescues. A wide plateau means a forgiving, legible puzzle; a thin one is a
    // knife-edge. Measured with the budget held fixed, so time dilation cannot pad the width.
    public static class PuzzleShipPlateau
    {
        private const int MinShips = 2;

        public static PuzzleShipPlateauReport Run(
            PuzzleSeed seed, SpaceShapingSettings settings, int fields, int samples)
        {
            PuzzleSimSettings sim = PuzzleSimSettings.From(settings);
            PuzzleShipPlateauReport report = new()
            {
                Fields = fields,
                Range = sim.MaxScaleFactor - sim.MinScaleFactor,
                MinWidth = float.MaxValue
            };

            for (int i = 0; i < fields; i++)
            {
                PuzzleData field = PuzzleSeedField.Build(seed, settings, seed.seed + i);
                if (field == null)
                    continue;

                report.Baked++;
                Accumulate(ref report, new Probe(field, sim, samples).Best());
                Object.DestroyImmediate(field);
            }

            if (report.Baked == 0)
                report.MinWidth = 0f;

            return report;
        }

        private static void Accumulate(ref PuzzleShipPlateauReport report, float width)
        {
            report.SumWidth += width;
            if (width > 0f)
                report.FieldsWithPlateau++;

            if (width < report.MinWidth)
                report.MinWidth = width;

            if (width > report.BestWidth)
                report.BestWidth = width;
        }

        private sealed class Probe
        {
            private readonly PuzzleShipScan scan;
            private readonly PuzzleData field;
            private readonly PuzzleSimSettings sim;
            private readonly float[] scales;
            private readonly float step;

            public Probe(PuzzleData field, PuzzleSimSettings sim, int samples)
            {
                this.field = field;
                this.sim = sim;
                scan = new PuzzleShipScan(field, sim, 0f);
                scales = Linear(sim, samples);
                step = (sim.MaxScaleFactor - sim.MinScaleFactor) / (samples - 1);
            }

            public float Best()
            {
                Vector3[] centres = PuzzleZoneCandidates.Centres(field, field.cubeSizeMax);
                float[] sizes = PuzzleZoneCandidates.Sizes(field);
                float best = 0f;
                for (int c = 0; c < centres.Length; c++)
                {
                    for (int s = 0; s < sizes.Length; s++)
                    {
                        float width = Width(centres[c], sizes[s]);
                        if (width > best)
                            best = width;
                    }
                }

                return best;
            }

            private float Width(Vector3 centre, float size)
            {
                int run = 0;
                int longest = 0;
                for (int k = 0; k < scales.Length; k++)
                {
                    PuzzleZone zone = PuzzleZoneCandidates.Zone(centre, size, scales[k], sim.ZoneFalloff);
                    if (!PuzzleZoneCandidates.CoversSource(zone, field))
                    {
                        run = 0;
                        continue;
                    }

                    run = scan.RescuedFixed(zone).Count >= MinShips ? run + 1 : 0;
                    if (run > longest)
                        longest = run;
                }

                return longest < 2 ? 0f : (longest - 1) * step;
            }

            private static float[] Linear(PuzzleSimSettings sim, int samples)
            {
                float[] scales = new float[samples];
                for (int i = 0; i < samples; i++)
                    scales[i] = Mathf.Lerp(sim.MinScaleFactor, sim.MaxScaleFactor, i / (float)(samples - 1));

                return scales;
            }
        }
    }
}

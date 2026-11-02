using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    internal sealed class PuzzleMultiShipProbe
    {
        private const int ScaleSamples = 13;

        private readonly PuzzleShipScan scan;
        private readonly PuzzleData field;
        private readonly PuzzleSimSettings settings;
        private readonly float[] scales;
        private readonly int required;
        private readonly PuzzleShipRelation relation;

        public PuzzleMultiShipProbe(PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief)
        {
            this.field = field;
            this.settings = settings;
            required = brief.Ships;
            relation = brief.Relation;
            scan = new PuzzleShipScan(field, settings, 0f);
            scales = Linear(settings, ScaleSamples);
        }

        public PuzzleShipScan Scan => scan;

        public bool Find(out PuzzleZone zone, out List<PuzzleShipRescue> rescued)
        {
            Candidate best = default;
            Vector3[] centres = PuzzleZoneCandidates.Centres(field, field.cubeSizeMax);
            float[] sizes = PuzzleZoneCandidates.Sizes(field);

            for (int c = 0; c < centres.Length; c++)
            {
                for (int s = 0; s < sizes.Length; s++)
                    Consider(centres[c], sizes[s], ref best);
            }

            zone = best.Zone;
            rescued = best.Rescued;
            return best.Count >= required;
        }

        private void Consider(Vector3 centre, float size, ref Candidate best)
        {
            List<PuzzleShipRescue>[] row = Row(centre, size);
            for (int k = 0; k < row.Length; k++)
            {
                if (!Plateau(row, k))
                    continue;

                int distinct = Distinct(row[k]);
                if (distinct <= best.Count)
                    continue;

                best.Count = distinct;
                best.Zone = Zone(centre, size, scales[k]);
                best.Rescued = row[k];
            }
        }

        private List<PuzzleShipRescue>[] Row(Vector3 centre, float size)
        {
            List<PuzzleShipRescue>[] row = new List<PuzzleShipRescue>[scales.Length];
            for (int k = 0; k < scales.Length; k++)
            {
                PuzzleZone zone = Zone(centre, size, scales[k]);
                if (PuzzleZoneCandidates.CoversSource(zone, field))
                    row[k] = scan.RescuedFixed(zone);
            }

            return row;
        }

        private bool Plateau(List<PuzzleShipRescue>[] row, int k)
        {
            if (row[k] == null || Distinct(row[k]) < required)
                return false;

            if (k > 0 && row[k - 1] != null && Distinct(row[k - 1]) >= required)
                return true;

            return k + 1 < row.Length && row[k + 1] != null && Distinct(row[k + 1]) >= required;
        }

        private int Distinct(List<PuzzleShipRescue> rescued)
        {
            int distinct = 0;
            for (int i = 0; i < rescued.Count; i++)
            {
                if (Fresh(rescued, i))
                    distinct++;
            }

            return distinct;
        }

        private bool Fresh(List<PuzzleShipRescue> rescued, int index)
        {
            for (int i = 0; i < index; i++)
            {
                if (!PuzzleMultiShipBake.Allowed(
                    relation, rescued[i].Origin, rescued[i].Destination,
                    rescued[index].Origin, rescued[index].Destination))
                    return false;
            }

            return true;
        }

        private PuzzleZone Zone(Vector3 centre, float size, float scale) =>
            PuzzleZoneCandidates.Zone(centre, size, scale, settings.ZoneFalloff);

        private struct Candidate
        {
            public int Count;
            public PuzzleZone Zone;
            public List<PuzzleShipRescue> Rescued;
        }

        private static float[] Linear(PuzzleSimSettings settings, int samples)
        {
            float[] scales = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                scales[i] = Mathf.Lerp(
                    settings.MinScaleFactor, settings.MaxScaleFactor, i / (float)(samples - 1));
            }

            return scales;
        }
    }
}

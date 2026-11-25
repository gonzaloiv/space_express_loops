using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    public sealed class PuzzleMultiShipProbe
    {
        private const int ScaleSamples = 13;

        private readonly PuzzleShipScan scan;
        private readonly FleetField fleet;
        private readonly PuzzleSimSettings settings;
        private readonly float[] scales;
        private readonly int required;
        private readonly PuzzleShipRelation relation;
        private readonly PuzzleSunShortcut sunShortcut;

        public PuzzleMultiShipProbe(PuzzleData field, PuzzleSimSettings settings, PuzzleBrief brief)
            : this(FleetField.From(field, settings), brief)
        {
        }

        // The snapshot path: the whole probe runs on plain data, so it is safe off the editor thread.
        public PuzzleMultiShipProbe(FleetField fleet, PuzzleBrief brief)
        {
            this.fleet = fleet;
            settings = fleet.Settings;
            required = brief.Ships;
            relation = brief.Relation;
            scan = new PuzzleShipScan(fleet, 0f);
            scales = Linear(settings, ScaleSamples);
            if (brief.SunRule == PuzzleSunRule.NoSunAnswer)
                sunShortcut = new PuzzleSunShortcut(fleet, scan, scales);
        }

        // Cancels between candidate zones, so a stopped search returns within one zone's work. onZone
        // fires per zone for the searching window's progress.
        public FleetZoneResult Find(CancellationToken token = default, System.Action onZone = null)
        {
            Candidate best = default;
            Vector3[] centres = PuzzleZoneCandidates.Centres(fleet, fleet.CubeSizeMax);
            float[] sizes = PuzzleZoneCandidates.Sizes(fleet);

            // Largest size first: the cube does not resize, so on a tie the biggest cube that rescues
            // every ship ships, like the single-ship jobs' full-size cube.
            for (int c = 0; c < centres.Length && !token.IsCancellationRequested; c++)
            {
                for (int s = sizes.Length - 1; s >= 0 && !token.IsCancellationRequested; s--)
                    Consider(centres[c], sizes[s], token, onZone, ref best);
            }

            return new FleetZoneResult(best.Count >= required, best.Zone, best.Rescued, scan);
        }

        private void Consider(
            Vector3 centre, float size, CancellationToken token, System.Action onZone, ref Candidate best)
        {
            List<PuzzleZone> zones = PuzzleZoneCandidates.Build(
                centre, new[] { size }, scales, settings.ZoneFalloff);
            List<PuzzleShipRescue>[] row = Row(zones, size, token, onZone);
            for (int k = 0; k < row.Length; k++)
            {
                if (!Plateau(row, k))
                    continue;

                int distinct = Distinct(row[k]);
                if (distinct <= best.Count)
                    continue;

                best.Count = distinct;
                best.Zone = zones[k];
                best.Rescued = row[k];
            }
        }

        private List<PuzzleShipRescue>[] Row(
            List<PuzzleZone> zones, float size, CancellationToken token, System.Action onZone)
        {
            List<PuzzleShipRescue>[] row = new List<PuzzleShipRescue>[zones.Count];
            for (int k = 0; k < zones.Count; k++)
            {
                if (token.IsCancellationRequested)
                    return row;

                onZone?.Invoke();
                if (!PuzzleZoneCandidates.CoversSource(zones[k], fleet))
                    continue;

                if (sunShortcut != null && sunShortcut.Covers(zones[k]))
                    continue;

                row[k] = scan.RescuedFixed(zones[k]);
                sunShortcut?.RemoveFrom(row[k], size);
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

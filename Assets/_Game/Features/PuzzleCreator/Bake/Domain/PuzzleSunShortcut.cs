using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // NoSunAnswer for the multi-ship probe: the runs a cube on the sun already rescues, per cube size.
    // The probe leaves them out of every candidate's rescue set and never picks a cube over the sun,
    // so the ships it selects can only be saved away from the sun, instead of rejecting the whole seed
    // after the fact.
    public sealed class PuzzleSunShortcut
    {
        private const float SameRun = 1e-8f;

        private readonly PuzzleShipScan scan;
        private readonly PuzzleSimSettings settings;
        private readonly float[] scales;
        private readonly Vector3 sun;
        private readonly bool hasSun;
        private readonly Dictionary<float, List<PuzzleShipRescue>> bySize = new();

        public PuzzleSunShortcut(FleetField fleet, PuzzleShipScan scan, float[] scales)
        {
            this.scan = scan;
            settings = fleet.Settings;
            this.scales = scales;
            hasSun = TryFindSun(fleet.Sources, out sun);
        }

        public bool Covers(PuzzleZone zone) => hasSun && zone.Contains(sun);

        public void RemoveFrom(List<PuzzleShipRescue> rescued, float size)
        {
            if (!hasSun || rescued == null)
                return;

            List<PuzzleShipRescue> shortcut = At(size);
            rescued.RemoveAll(run => Contains(shortcut, run));
        }

        private List<PuzzleShipRescue> At(float size)
        {
            if (bySize.TryGetValue(size, out List<PuzzleShipRescue> runs))
                return runs;

            runs = new List<PuzzleShipRescue>();
            List<PuzzleZone> zones = PuzzleZoneCandidates.Build(
                sun, new[] { size }, scales, settings.ZoneFalloff);
            for (int i = 0; i < zones.Count; i++)
                runs.AddRange(scan.RescuedFixed(zones[i]));

            bySize[size] = runs;
            return runs;
        }

        private static bool Contains(List<PuzzleShipRescue> runs, PuzzleShipRescue run)
        {
            for (int i = 0; i < runs.Count; i++)
            {
                if ((runs[i].Origin - run.Origin).sqrMagnitude < SameRun
                    && (runs[i].Destination - run.Destination).sqrMagnitude < SameRun)
                    return true;
            }

            return false;
        }

        private static bool TryFindSun(PuzzleSource[] sources, out Vector3 sun)
        {
            sun = Vector3.zero;
            if (sources == null)
                return false;

            for (int i = 0; i < sources.Length; i++)
            {
                if (!sources[i].centreOnly)
                    continue;

                sun = sources[i].home;
                return true;
            }

            return false;
        }
    }
}

using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The player's zone edits: a stack of committed scale commands plus an in-flight preview, and
    // the effective zone set and entropy they produce.
    public sealed class ZoneCommandLog
    {
        private readonly List<SpaceScaleCommand> commands = new();
        private readonly List<SpaceZone> zones = new();

        private SpaceScaleCommand preview;

        public IReadOnlyList<SpaceZone> Zones => zones;

        public int Count => commands.Count;

        public float Committed { get; private set; }

        public float Spent { get; private set; }

        public float Pending => Spent - Committed;

        public void Clear()
        {
            commands.Clear();
            preview = null;
        }

        public void Preview(SpaceZone zone)
        {
            float cost = Mathf.Abs(zone.Scale - ReferenceScale(zone));
            preview = new SpaceScaleCommand(zone, cost);
        }

        public void Commit()
        {
            if (preview == null)
                return;

            commands.Add(preview);
            preview = null;
        }

        public bool Undo()
        {
            if (commands.Count == 0)
                return false;

            commands.RemoveAt(commands.Count - 1);
            return true;
        }

        public void Rebuild(float gain)
        {
            zones.Clear();
            for (int c = 0; c < commands.Count; c++)
                UpsertZone(commands[c].Zone, gain);

            if (preview != null)
                UpsertZone(preview.Zone, gain);

            RecomputeEntropy();
        }

        private void UpsertZone(SpaceZone zone, float gain)
        {
            SpaceZone effective = zone.Effective(gain);
            for (int i = 0; i < zones.Count; i++)
            {
                if (!zones[i].SameRegion(zone))
                    continue;

                zones[i] = effective;
                return;
            }

            zones.Add(effective);
        }

        private void RecomputeEntropy()
        {
            float committed = 0f;
            for (int c = 0; c < commands.Count; c++)
                committed += commands[c].EntropyCost;

            Committed = committed;
            Spent = committed + (preview != null ? preview.EntropyCost : 0f);
        }

        private float ReferenceScale(SpaceZone zone)
        {
            for (int c = commands.Count - 1; c >= 0; c--)
            {
                if (commands[c].Zone.SameRegion(zone))
                    return commands[c].Zone.Scale;
            }

            return 1f;
        }
    }
}

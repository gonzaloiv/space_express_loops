using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // One pooled trajectory line per run, spawned from an authored prototype.
    public class TrajectoryLinePool
    {
        private readonly TrajectoryLineView prototype;
        private readonly Transform parent;
        private readonly List<TrajectoryLineView> lines = new();

        public TrajectoryLinePool(TrajectoryLineView prototype, Transform parent)
        {
            this.prototype = prototype;
            this.parent = parent;
        }

        public void Show(
            IReadOnlyList<TrajectoryResult> results, Vector3 centre, float half, bool pending)
        {
            for (int i = 0; i < results.Count; i++)
                Slot(i).Show(results[i], Arrived(results[i]), centre, half, pending);

            for (int i = results.Count; i < lines.Count; i++)
                lines[i].Hide();
        }

        public void Pulse()
        {
            for (int i = 0; i < lines.Count; i++)
                lines[i].Pulse();
        }

        private static bool Arrived(TrajectoryResult result) =>
            result.Outcome == TrajectoryOutcome.Arrive;

        private TrajectoryLineView Slot(int index)
        {
            while (index >= lines.Count)
            {
                TrajectoryLineView created = Object.Instantiate(prototype, parent);
                created.name = "Line " + (lines.Count + 1);
                lines.Add(created);
            }

            return lines[index];
        }
    }
}

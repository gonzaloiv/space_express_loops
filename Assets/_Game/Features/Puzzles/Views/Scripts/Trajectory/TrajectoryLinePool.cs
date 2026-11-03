using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // One pooled trajectory line per run, spawned from an authored prototype.
    public class TrajectoryLinePool
    {
        private readonly ViewPool<TrajectoryLineView> pool;

        public TrajectoryLinePool(TrajectoryLineView prototype, Transform parent) =>
            pool = new ViewPool<TrajectoryLineView>(prototype, parent, "Line", null);

        public void Show(
            IReadOnlyList<TrajectoryResult> results, Vector3 centre, float half, bool pending)
        {
            for (int i = 0; i < results.Count; i++)
                pool.EnsureSlot(i).Show(results[i], Arrived(results[i]), centre, half, pending);

            HideFrom(results.Count);
        }

        public void ShowBaselines(IReadOnlyList<Vector3> starts, IReadOnlyList<Vector3> ends)
        {
            int count = Mathf.Min(starts.Count, ends.Count);
            for (int i = 0; i < count; i++)
                pool.EnsureSlot(i).ShowBaseline(starts[i], ends[i]);

            HideFrom(count);
        }

        public void Hide() => HideFrom(0);

        public void Pulse()
        {
            IReadOnlyList<TrajectoryLineView> views = pool.Views;
            for (int i = 0; i < views.Count; i++)
                views[i].Pulse();
        }

        private static bool Arrived(TrajectoryResult result) =>
            result.Outcome == TrajectoryOutcome.Arrive;

        private void HideFrom(int index)
        {
            IReadOnlyList<TrajectoryLineView> views = pool.Views;
            for (int i = index; i < views.Count; i++)
                views[i].Hide();
        }
    }
}

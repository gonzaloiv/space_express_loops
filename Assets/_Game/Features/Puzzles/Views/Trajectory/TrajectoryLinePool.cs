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
            IReadOnlyList<TrajectoryResult> results, SpaceShapingSession session, RoutePlan plan,
            Vector3 centre, float half, bool pending)
        {
            for (int i = 0; i < results.Count; i++)
            {
                // The dot's warmth is onboarding only: a graded job's preview stays neutral.
                float? gap = pending && session.NoGrade ? session.MissGap(i, results[i]) : null;
                pool.EnsureSlot(i).Show(results[i], plan.ColorAt(i), gap, centre, half, pending);
            }

            HideFrom(results.Count);
        }

        public void ShowBaselines(
            IReadOnlyList<Vector3> starts, IReadOnlyList<Vector3> ends, RoutePlan plan)
        {
            int count = Mathf.Min(starts.Count, ends.Count);
            for (int i = 0; i < count; i++)
                pool.EnsureSlot(i).ShowBaseline(starts[i], ends[i], plan.ColorAt(i));

            HideFrom(count);
        }

        public void Hide() => HideFrom(0);

        public void Pulse()
        {
            IReadOnlyList<TrajectoryLineView> views = pool.Views;
            for (int i = 0; i < views.Count; i++)
                views[i].Pulse();
        }

        private void HideFrom(int index) =>
            pool.ForEachFrom(index, view => view.Hide());
    }
}

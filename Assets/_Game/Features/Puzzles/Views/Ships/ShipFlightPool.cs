using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // One pooled ship view per run, spawned from an authored prototype. Reports once when the whole
    // flight has landed.
    public class ShipFlightPool
    {
        private readonly ViewPool<ShipTravelView> pool;
        private readonly float staggerSecs;

        private int flying;

        public ShipFlightPool(ShipTravelView prototype, Transform parent, float staggerSecs)
        {
            this.staggerSecs = Mathf.Max(staggerSecs, 0f);
            pool = new ViewPool<ShipTravelView>(prototype, parent, "Ship", OnCreated);
        }

        public event Action Completed;

        public void SoftReset(IReadOnlyList<Vector3> origins)
        {
            Stop();
            for (int i = 0; i < origins.Count; i++)
                pool.EnsureSlot(i).SoftReset(origins[i]);
        }

        public void Travel(IReadOnlyList<TrajectoryResult> results, float stepSecs, bool audible)
        {
            Stop();
            float step = Mathf.Max(stepSecs, 0f);
            for (int i = 0; i < results.Count; i++)
            {
                if (!CanFly(results[i]))
                    continue;

                flying++;
                float simSecs = (results[i].Points.Count - 1) * step;
                pool.EnsureSlot(i).Travel(
                    results[i], simSecs, (flying - 1) * staggerSecs, audible);
            }

            if (flying == 0)
                Completed?.Invoke();
        }

        public void Stop()
        {
            flying = 0;
            pool.ForEachFrom(0, view => view.Stop());
        }

        private static bool CanFly(TrajectoryResult result) =>
            result.Points != null && result.Points.Count >= 2;

        private void OnCreated(ShipTravelView ship) => ship.Completed += OnLanded;

        private void OnLanded()
        {
            flying--;
            if (flying <= 0)
                Completed?.Invoke();
        }
    }
}

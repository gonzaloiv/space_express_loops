using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // One pooled ship view per run, spawned from an authored prototype. Reports once when the whole
    // flight has landed.
    public class ShipFlightPool
    {
        private readonly ShipTravelView prototype;
        private readonly Transform parent;
        private readonly List<ShipTravelView> ships = new();

        private int flying;

        public ShipFlightPool(ShipTravelView prototype, Transform parent)
        {
            this.prototype = prototype;
            this.parent = parent;
        }

        public event Action Completed;

        public void SoftReset(IReadOnlyList<Vector3> origins)
        {
            Stop();
            for (int i = 0; i < origins.Count; i++)
                Slot(i).SoftReset(origins[i]);
        }

        public void Travel(IReadOnlyList<TrajectoryResult> results)
        {
            Stop();
            for (int i = 0; i < results.Count; i++)
            {
                if (!CanFly(results[i]))
                    continue;

                flying++;
                Slot(i).Travel(results[i]);
            }

            if (flying == 0)
                Completed?.Invoke();
        }

        public void Stop()
        {
            flying = 0;
            for (int i = 0; i < ships.Count; i++)
                ships[i].Stop();
        }

        private static bool CanFly(TrajectoryResult result) =>
            result.Points != null && result.Points.Count >= 2;

        private void OnLanded()
        {
            flying--;
            if (flying <= 0)
                Completed?.Invoke();
        }

        private ShipTravelView Slot(int index)
        {
            while (index >= ships.Count)
            {
                ShipTravelView created = Create(ships.Count == 0 ? prototype : ships[0]);
                created.name = "Ship " + (ships.Count + 1);
                ships.Add(created);
            }

            return ships[index];
        }

        private ShipTravelView Create(ShipTravelView source)
        {
            ShipTravelView created = UnityEngine.Object.Instantiate(source, parent);
            created.Completed += OnLanded;
            return created;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // One pooled keep-out ghost per endpoint, shown only while the cube is held for a move.
    public sealed class EndpointKeepOutPool
    {
        private readonly ViewPool<EndpointKeepOutView> pool;

        public EndpointKeepOutPool(EndpointKeepOutView prototype, Transform parent) =>
            pool = new ViewPool<EndpointKeepOutView>(prototype, parent, "KeepOut", null);

        public void Show(IReadOnlyList<Vector3> endpoints, Vector3 size)
        {
            int count = endpoints != null ? endpoints.Count : 0;
            for (int i = 0; i < count; i++)
                pool.EnsureSlot(i).Show(endpoints[i], size);

            HideFrom(count);
        }

        public void Hide() => HideFrom(0);

        public void Flash(int endpoint)
        {
            if (endpoint >= 0 && endpoint < pool.Views.Count)
                pool.Views[endpoint].Flash();
        }

        private void HideFrom(int index) => pool.ForEachFrom(index, view => view.Hide());
    }
}

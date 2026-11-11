using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // One pooled red cube for the refused move; it follows the attempted centre while blocked.
    public sealed class EndpointKeepOutPool
    {
        private readonly ViewPool<EndpointKeepOutView> pool;

        public EndpointKeepOutPool(EndpointKeepOutView prototype, Transform parent) =>
            pool = new ViewPool<EndpointKeepOutView>(prototype, parent, "Refusal", null);

        public void Show(Vector3 centre, Vector3 size, Color danger)
        {
            pool.EnsureSlot(0).Show(centre, size, danger);
            pool.ForEachFrom(1, view => view.Hide());
        }

        public void Hide() => pool.ForEachFrom(0, view => view.Hide());
    }
}

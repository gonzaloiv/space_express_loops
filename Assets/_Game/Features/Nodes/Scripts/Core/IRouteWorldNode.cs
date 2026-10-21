using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public interface IRouteWorldNode
    {
        string Id { get; }
        Vector3 Position { get; }
        NodeBody NodeBody { get; }
        bool IsActive { get; }
        void ApplyRouteColor(Color color);
        void ResetRouteColor();
    }
}

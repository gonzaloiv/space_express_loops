using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    internal sealed class CubeHoverTracker
    {
        private bool hovering;
        private Vector3 world;

        public bool Hovering => hovering;

        public Vector3 World => world;

        public void Track(PointerEvent evt)
        {
            switch (evt.Type)
            {
                case PointerEventType.Hover:
                case PointerEventType.Move:
                    world = evt.Pose.position;
                    hovering = true;
                    break;
                case PointerEventType.Unhover:
                case PointerEventType.Cancel:
                    hovering = false;
                    break;
            }
        }
    }
}
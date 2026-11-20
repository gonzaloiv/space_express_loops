using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    internal sealed class CubeHoverTracker
    {
        private bool hovering;
        private Vector3 world;

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

        // A one-hand pinch moves the cube only from inside its frame, so that spot gets its own state.
        public CubeHover Kind(CubeView zone)
        {
            if (!hovering)
                return CubeHover.None;
            if (zone.CanMove && zone.FrameContains(world))
                return CubeHover.Move;

            return zone.ScaleContains(world) ? CubeHover.Scale : CubeHover.Near;
        }
    }
}

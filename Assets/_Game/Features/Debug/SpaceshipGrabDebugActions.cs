using DigitalLove.Game.Nodes;
using DigitalLove.Game.Spaceships;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.DebugActions
{
    [RequireComponent(typeof(EditorDebugContext))]
    public class SpaceshipGrabDebugActions : MonoBehaviour
    {
        private EditorDebugContext context;

        private void Awake() => context = GetComponent<EditorDebugContext>();

        private SpaceshipBehaviour ResolveShip()
        {
            SpaceshipBehaviour ship = context.ResolveTargetShip();
            if (ship == null)
                return null;

            context.EnsureSpaceshipsReady();
            if (!ship.IsInitialized)
                ship.Initialize();

            context.Spaceships.WireLoopHandlers(ship);
            return ship;
        }

        #region Debug

        [Button]
        public void Debug_GrabSelect()
        {
            SpaceshipBehaviour ship = ResolveShip();
            if (ship == null)
                return;

            ship.Debug_SimulateGrabSelect();
            Debug.Log($"EditorDebug: Grab select on ship {ship.Id}.");
        }

        [Button]
        public void Debug_GrabRelease()
        {
            SpaceshipBehaviour ship = ResolveShip();
            if (ship == null)
                return;

            ship.Debug_SimulateGrabRelease();
            Debug.Log($"EditorDebug: Grab release on ship {ship.Id}.");
        }

        [Button]
        public void Debug_GrabStartRouteFromHub()
        {
            SpaceshipBehaviour ship = ResolveShip();
            if (ship == null)
                return;

            ship.Debug_SimulateGrabSelect();
            Debug.Log($"EditorDebug: Grab from hub on ship {ship.Id} (enters destination selection).");
        }

        #endregion
    }
}

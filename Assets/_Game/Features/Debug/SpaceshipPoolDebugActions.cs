using DigitalLove.Game.Planets;
using DigitalLove.Game.Spaceships;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.DebugActions
{
    [RequireComponent(typeof(EditorDebugContext))]
    public class SpaceshipPoolDebugActions : MonoBehaviour
    {
        private EditorDebugContext context;

        private void Awake() => context = GetComponent<EditorDebugContext>();

        [Button]
        public void Debug_InitializeSpaceshipPool() => context.Spaceships.InitializePool();

        [Button]
        public void Debug_HideAllSpaceships() => context.Spaceships.HideAll();

        [Button]
        public void Debug_SpawnIdleShipAtHub()
        {
            context.EnsureSpaceshipsReady();
            HubBehaviour hub = context.Hubs.SpawnNew();
            SpaceshipBehaviour ship = context.Spaceships.SpawnNew(hub);
            EditorDebugContext.WireLoopLoggers(ship);
            Debug.Log($"Spawned idle ship {ship.Id} at hub {hub.Id}.");
        }
    }
}

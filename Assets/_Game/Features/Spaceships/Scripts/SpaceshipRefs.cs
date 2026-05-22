using DigitalLove.Game.UI;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipRefs : MonoBehaviour
    {
        [Header("Idle")]
        public GameObject grabMePanel;

        [Header("Shared")]
        public GrabbableWrapper grabbableWrapper;
        public RoutePanel routePanel;
        public DestinationSelector destinationSelector;
        public GrabZone grabZone;

        [Header("Route")]
        public GhostBehaviour ghost;
        public SplineContainerWrapper splineContainerWrapper;
        public TravellerBehaviour traveller;
        public float legDelay = 1f;
    }
}

using DigitalLove.Game.UI;
using DigitalLove.Global;
using UnityEngine;
using UnityEngine.Serialization;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipRefs : MonoBehaviour
    {
        [SerializeField] private GameObject grabMePanel;
        [SerializeField] private GrabbableWrapper grabbableWrapper;
        [SerializeField] private RoutePanel routePanel;
        [SerializeField] private DestinationSelector destinationSelector;
        [SerializeField] private GrabZone grabZone;
        [SerializeField] private GhostBehaviour ghost;
        [SerializeField] private SplineContainerWrapper splineContainerWrapper;
        [SerializeField] private TravellerBehaviour traveller;
        [SerializeField, FormerlySerializedAs("legDelay")] private float legDelaySecs = 1f;
        [SerializeField] private FloatValue gameSpeed;

        public GameObject GrabMePanel => grabMePanel;
        public GrabbableWrapper GrabbableWrapper => grabbableWrapper;
        public RoutePanel RoutePanel => routePanel;
        public DestinationSelector DestinationSelector => destinationSelector;
        public GrabZone GrabZone => grabZone;
        public GhostBehaviour Ghost => ghost;
        public SplineContainerWrapper SplineContainerWrapper => splineContainerWrapper;
        public TravellerBehaviour Traveller => traveller;
        public FloatValue GameSpeed => gameSpeed;
        public float LegDelay => legDelaySecs / Mathf.Max(0.0001f, gameSpeed.value);
    }
}

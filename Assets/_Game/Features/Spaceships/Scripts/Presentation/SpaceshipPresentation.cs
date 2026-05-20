using DigitalLove.Game.UI;
using Oculus.Interaction;
using UnityEngine;
using DigitalLove.Global;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipPresentation : MonoBehaviour
    {
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private GrabbableBody grabbableBody;
        [SerializeField] private GhostBehaviour ghost;
        [SerializeField] private RoutePanel routePanel;
        [SerializeField] private DestinationSelector destinationSelector;

        public Grabbable Grabbable => grabbable;
        public RoutePanel RoutePanel => routePanel;
        public DestinationSelector DestinationSelector => destinationSelector;

        public void Reset(SpaceshipLoop loop)
        {
            ghost.SetActive(false);
            grabbableBody.Hide();
            routePanel.Hide();
            loop.SetLineRendererActive(false);
            destinationSelector.StartLookingForDestination(false);
        }

        public void ShowHubIdle()
        {
            grabbableBody.Show();
            destinationSelector.StartLookingForDestination(false);
            grabbable.SetActive(true);
        }

        public void HideHubIdle() => grabbableBody.Hide();

        public void ShowLoopChrome(SpaceshipLoop loop)
        {
            ghost.SetActive(false);
            destinationSelector.StartLookingForDestination(false);

            routePanel.SetPosition(loop.Hub.transform.position);
            routePanel.Show();

            grabbableBody.Show();
            grabbable.SetActive(true);
            loop.MoveShipToActiveStation();
        }

        public void ShowSelectingChrome() => ghost.SetActive(true);

        public void HideSelectingChrome() => ghost.SetActive(false);

        public void ShowStationGrab(SpaceshipLoop loop)
        {
            grabbableBody.Show();
            loop.MoveShipToActiveStation();
        }
    }
}

using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public class PlanetRoutePresentation
    {
        private readonly PlanetRefs refs;
        private bool isDestination;
        private bool isOnRoute;

        public bool IsDestination => isDestination;
        public bool IsOnRoute => isOnRoute;

        public PlanetRoutePresentation(PlanetRefs refs)
        {
            this.refs = refs;
        }

        public void Apply(RouteVisualState state, Color? routeColor = null)
        {
            switch (state)
            {
                case RouteVisualState.Default:
                    ResetToDefault();
                    break;
                case RouteVisualState.OnRoute:
                    if (routeColor.HasValue)
                        refs.NodeBody.SetRouteColor(routeColor.Value);
                    isOnRoute = true;
                    isDestination = false;
                    refs.Outline.enabled = false;
                    break;
                case RouteVisualState.SelectingCandidate:
                    isOnRoute = false;
                    SetIsDestination(false);
                    refs.Outline.enabled = true;
                    break;
                case RouteVisualState.ConfirmedDestination:
                    SetIsDestination(true);
                    refs.Outline.enabled = false;
                    break;
            }
        }

        public void ResetToDefault()
        {
            isOnRoute = false;
            SetIsDestination(false);
            refs.NodeBody.ResetRouteColor();
            refs.Outline.enabled = false;
        }

        private void SetIsDestination(bool value)
        {
            bool wasDestination = isDestination;
            isDestination = value;
            if (value && !wasDestination)
                refs.ScalePunch.Animate();
        }
    }
}

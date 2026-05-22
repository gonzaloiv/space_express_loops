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
                        refs.nodeBody.SetRouteColor(routeColor.Value);
                    isOnRoute = true;
                    isDestination = false;
                    refs.outline.enabled = false;
                    break;
                case RouteVisualState.SelectingCandidate:
                    isOnRoute = false;
                    SetIsDestination(false);
                    refs.outline.enabled = true;
                    break;
                case RouteVisualState.ConfirmedDestination:
                    SetIsDestination(true);
                    refs.outline.enabled = false;
                    break;
            }
        }

        public void ResetToDefault()
        {
            isOnRoute = false;
            SetIsDestination(false);
            refs.nodeBody.ResetRouteColor();
            refs.outline.enabled = false;
        }

        private void SetIsDestination(bool value)
        {
            bool wasDestination = isDestination;
            isDestination = value;
            if (value && !wasDestination)
                refs.scalePunch.Animate();
        }
    }
}

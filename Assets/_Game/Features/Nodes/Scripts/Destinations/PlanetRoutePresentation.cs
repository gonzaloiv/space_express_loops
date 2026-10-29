using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public class PlanetRoutePresentation
    {
        private readonly PlanetRefs refs;
        private bool isDestination;
        private bool isOnRoute;
        private Color defaultOutlineColor;
        private float defaultOutlineWidth;

        public bool IsDestination => isDestination;
        public bool IsOnRoute => isOnRoute;

        public PlanetRoutePresentation(PlanetRefs refs)
        {
            this.refs = refs;
            defaultOutlineColor = refs.Outline.OutlineColor;
            defaultOutlineWidth = refs.Outline.OutlineWidth;
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
                    ResetDestinationOutline();
                    break;
                case RouteVisualState.SelectingCandidate:
                    isOnRoute = false;
                    SetIsDestination(false);
                    refs.Outline.enabled = true;
                    ResetDestinationOutline();
                    break;
                case RouteVisualState.ConfirmedDestination:
                    SetIsDestination(true);
                    refs.Outline.enabled = false;
                    ShowDestinationOutline();
                    break;
            }
        }

        public void ResetToDefault()
        {
            isOnRoute = false;
            SetIsDestination(false);
            refs.NodeBody.ResetRouteColor();
            refs.Outline.enabled = false;
            ResetDestinationOutline();
        }

        private void SetIsDestination(bool value)
        {
            bool wasDestination = isDestination;
            isDestination = value;
            if (value && !wasDestination)
                refs.ScalePunch.Animate();
        }

        private void ShowDestinationOutline()
        {
            refs.Outline.OutlineColor = refs.DestinationColor;
            refs.Outline.OutlineWidth = refs.DestinationOutlineWidth;
            refs.Outline.enabled = true;
        }

        private void ResetDestinationOutline()
        {
            refs.Outline.OutlineColor = defaultOutlineColor;
            refs.Outline.OutlineWidth = defaultOutlineWidth;
        }
    }
}

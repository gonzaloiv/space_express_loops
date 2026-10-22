using DigitalLove.Game.SilentPlanets;
using DigitalLove.Game.Spaceships;
using DigitalLove.Game.Universe;

namespace DigitalLove.Game.Levels
{
    public class LevelViewModeFanout
    {
        private readonly UniverseView universeView;
        private readonly SpaceshipsSpawner spaceshipsSpawner;
        private readonly SilentPlanetsSpawner silentPlanetsSpawner;

        public LevelViewModeFanout(
            UniverseView universeView,
            SpaceshipsSpawner spaceshipsSpawner,
            SilentPlanetsSpawner silentPlanetsSpawner)
        {
            this.universeView = universeView;
            this.spaceshipsSpawner = spaceshipsSpawner;
            this.silentPlanetsSpawner = silentPlanetsSpawner;
        }

        public void Subscribe()
        {
            universeView.Mode.ModeChanged -= OnModeChanged;
            universeView.Mode.ModeChanged += OnModeChanged;
            universeView.Mode.TransitionStarted -= OnTransitionStarted;
            universeView.Mode.TransitionStarted += OnTransitionStarted;
            universeView.Mode.TransitionCompleted -= OnTransitionCompleted;
            universeView.Mode.TransitionCompleted += OnTransitionCompleted;
            ApplyCurrentMode();
        }

        public void Unsubscribe()
        {
            universeView.Mode.ModeChanged -= OnModeChanged;
            universeView.Mode.TransitionStarted -= OnTransitionStarted;
            universeView.Mode.TransitionCompleted -= OnTransitionCompleted;
        }

        public void ApplyCurrentMode() => ApplyBubbleView(universeView.Mode.Current);

        private void OnModeChanged(UniverseViewMode _, UniverseViewMode to) => ApplyBubbleView(to);

        private void OnTransitionStarted(UniverseViewMode target)
        {
            spaceshipsSpawner.SetViewTransition(true);
            ApplyBubbleView(target);
        }

        private void OnTransitionCompleted(UniverseViewMode target)
        {
            ApplyBubbleView(target);
            spaceshipsSpawner.SetViewTransition(false);
            spaceshipsSpawner.RefreshGrabbablesAtStation();
        }

        private void ApplyBubbleView(UniverseViewMode mode)
        {
            bool isBubble = mode == UniverseViewMode.Bubble;
            spaceshipsSpawner.SetBubbleView(isBubble);
            silentPlanetsSpawner.SetBubbleView(isBubble);
        }
    }
}

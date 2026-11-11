using DigitalLove.Game.PuzzleSelection;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.Input;
using DigitalLove.Player;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    // Shows the pinch indicator whenever a pinch does something in the game: a hand on the puzzle's
    // cube (move or scale), on the map's selection cube (slide or pinch to enter), or the left hand in
    // the menu pose during a job (the pinch that exits to the map). Any other pinch shows nothing.
    public class PinchIndicatorGate : MonoBehaviour
    {
        [SerializeField] private TwoHandGestureIndicator indicator;
        [SerializeField] private SpaceShapingView shapingView;
        [SerializeField] private PuzzleMapView mapView;
        [SerializeField] private MetaAimFlagsHomeMenuInvoker menuGesture;

        private void LateUpdate()
        {
            bool inJob = shapingView.isActiveAndEnabled;
            indicator.Shown = (inJob && (shapingView.IsCubeHeld || menuGesture.IsInMenuPose))
                || (mapView.isActiveAndEnabled && mapView.IsCubeHeld);
        }

        private void OnDisable() => indicator.Shown = false;
    }
}

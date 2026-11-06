using System;
using DigitalLove.Game.PuzzleSelection;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.UI.Behaviours;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    public class GameGesturePromptSource : GesturePromptSource
    {
        [SerializeField] private SpaceShapingController puzzleController;
        [SerializeField] private PuzzleMapView mapView;

        private Action<GestureKind> completed;

        public override event Action<GestureKind> Completed
        {
            add => completed += value;
            remove => completed -= value;
        }

        private void OnEnable()
        {
            puzzleController.PinchCommitted += OnPinchCommitted;
            puzzleController.CubeMoved += OnCubeMoved;
            mapView.EnterRequested += OnEnterRequested;
            mapView.Panned += OnPanned;
        }

        private void OnDisable()
        {
            puzzleController.PinchCommitted -= OnPinchCommitted;
            puzzleController.CubeMoved -= OnCubeMoved;
            mapView.EnterRequested -= OnEnterRequested;
            mapView.Panned -= OnPanned;
        }

        private void OnPinchCommitted() => completed?.Invoke(GestureKind.Scale);

        private void OnCubeMoved() => completed?.Invoke(GestureKind.Move);

        private void OnEnterRequested() => completed?.Invoke(GestureKind.Enter);

        private void OnPanned() => completed?.Invoke(GestureKind.Pan);
    }
}
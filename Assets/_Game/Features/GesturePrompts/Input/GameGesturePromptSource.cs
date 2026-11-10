using System;
using DigitalLove.Game.PuzzleSelection;
using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.GesturePrompts
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
        }

        private void OnDisable()
        {
            puzzleController.PinchCommitted -= OnPinchCommitted;
            puzzleController.CubeMoved -= OnCubeMoved;
            mapView.EnterRequested -= OnEnterRequested;
        }

        // The exit gesture is read by the puzzle flow, which reports it here so the prompt is learned.
        public void ReportExit() => completed?.Invoke(GestureKind.Exit);

        // The flow owns the lesson timing: Spend is learned when a failing job ends, Stars on a solve.
        public void ReportSpend() => completed?.Invoke(GestureKind.Spend);

        public void ReportStars() => completed?.Invoke(GestureKind.Stars);

        private void OnPinchCommitted() => completed?.Invoke(GestureKind.Scale);

        private void OnCubeMoved() => completed?.Invoke(GestureKind.Move);

        private void OnEnterRequested() => completed?.Invoke(GestureKind.Enter);
    }
}

using System.Collections.Generic;
using DigitalLove.Game.Spatial;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Poses a job mid-stretch without hands, for store art: one frame's zone held at a scale, and the
    // lattice, planets and predicted paths drawn the way a live pinch draws them, and each ship held at
    // a fraction of its predicted run (0 hides them).
    public static class PuzzleStretchPose
    {
        public static void Show(
            PuzzleData puzzle, SpaceShapingSettings settings, SpaceShapingView view, int frameIndex,
            float scale, float shipProgress, float shipScale)
        {
            view.gameObject.SetActive(true);
            view.SetRoutePlan(RoutePlan.Build(puzzle, settings.routePalette));
            view.SetConditions(puzzle.ToZoneConditions());
            view.PlaceFrames(puzzle.frames, settings.zoneFalloff);

            PuzzleRun run = new(SpaceShapingSetup.Build(puzzle, PuzzleSimSettings.From(settings)));
            if (puzzle.frames != null && puzzle.frames.Length > 0)
            {
                int index = Mathf.Clamp(frameIndex, 0, puzzle.frames.Length - 1);
                PuzzleFrame frame = puzzle.frames[index];
                run.PreviewZone(new SpaceZone(frame.centre, frame.BoxSize * 0.5f, scale, settings.zoneFalloff));
                view.PoseZone(index, scale);
            }

            PuzzlePreview.Apply(run, view);
            IReadOnlyList<TrajectoryResult> results = run.SimulatePreview();
            view.ShowTrajectories(run.Session, results, true);
            if (shipProgress > 0f)
                view.HoldShips(run.Session, results, shipProgress, shipScale);
            else
                view.StopShips();
        }
    }
}

using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.Store
{
    // The store cover's set (Cover Shooting scene): a job's field posed mid-stretch, with no hands and
    // no flow, so the camera can take it from any angle. It poses on Play (the view builds its pools in
    // Awake); the context menu re-poses after a tweak.
    public class CoverStage : MonoBehaviour
    {
        [SerializeField] private SpaceShapingView view;
        [SerializeField] private SpaceShapingSettings settings;
        [Tooltip("The job to pose; a deep well (job 5 or 6) bends the lattice most.")]
        [SerializeField] private PuzzleData puzzle;
        [Tooltip("Which of the job's frames is being stretched.")]
        [SerializeField, Min(0)] private int frame;
        [Tooltip("The zone's scale mid-stretch: above 1 grows a well (curve), below 1 shrinks a blocker (clear).")]
        [SerializeField, Range(0.3f, 3f)] private float scale = 1.8f;

        private void Start() => Pose();

        [ContextMenu("Pose stretch")]
        public void Pose() => PuzzleStretchPose.Show(puzzle, settings, view, frame, scale);

        [ContextMenu("Pose rest")]
        public void PoseRest() => PuzzleStretchPose.Show(puzzle, settings, view, frame, 1f);
    }
}

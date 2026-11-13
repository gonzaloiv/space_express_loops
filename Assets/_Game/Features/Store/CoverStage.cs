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
        [Tooltip("How far along its predicted run the ship is held (0 = no ship, 1 = at the destination).")]
        [SerializeField, Range(0f, 1f)] private float shipProgress = 0.65f;
        [Tooltip("The held ship's size against its authored size; the real ship is a few pixels at cover distance.")]
        [SerializeField, Range(1f, 6f)] private float shipScale = 3f;

        private void Start() => Pose();

        [ContextMenu("Pose stretch")]
        public void Pose() => Show(scale);

        [ContextMenu("Pose rest")]
        public void PoseRest() => Show(1f);

        private void Show(float zoneScale) =>
            PuzzleStretchPose.Show(puzzle, settings, view, frame, zoneScale, shipProgress, shipScale);
    }
}

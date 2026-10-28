using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class PuzzleFieldPlacement : MonoBehaviour
    {
        [SerializeField] private Transform head;
        [SerializeField] private SpaceShapingState state;
        [SerializeField] private float distance = 0.6f;
        [SerializeField] private float heightOffset = -0.1f;
        [SerializeField] private float scale = 0.8f;

        private PuzzleData lastPuzzle;

        private void Update()
        {
            if (head.position.sqrMagnitude < 0.01f)
                return;

            PuzzleData current = state.CurrentPuzzle;
            if (current == null || current == lastPuzzle)
                return;

            lastPuzzle = current;
            Place(current);
        }

        private void Place(PuzzleData puzzle)
        {
            Vector3 headForward = head.forward;
            headForward = headForward.sqrMagnitude > 0.0001f ? headForward.normalized : Vector3.forward;

            Quaternion view = Quaternion.LookRotation(headForward, Vector3.up);
            Quaternion data = PuzzleViewFrame.Rotation(puzzle.origin, puzzle.destination);
            Quaternion rotation = view * Quaternion.Inverse(data);

            Vector3 centre = head.position + headForward * distance + Vector3.up * heightOffset;
            Vector3 midpoint = (puzzle.origin + puzzle.destination) * 0.5f;
            transform.SetPositionAndRotation(centre - rotation * (midpoint * scale), rotation);
            transform.localScale = Vector3.one * scale;
        }
    }
}

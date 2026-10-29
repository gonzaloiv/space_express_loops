using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class PuzzleFieldPlacement : MonoBehaviour
    {
        [SerializeField] private Transform head;
        [SerializeField] private SpaceShapingController state;
        [SerializeField] private float distance = 0.6f;
        [SerializeField] private float heightOffset = -0.1f;
        [SerializeField] private float scale = 0.8f;

        private bool placed;

        private void Update()
        {
            if (placed || head.position.sqrMagnitude < 0.01f)
                return;

            PuzzleData current = state.CurrentPuzzle;
            if (current == null)
                return;

            placed = true;
            Place(current);
        }

        private void Place(PuzzleData puzzle)
        {
            Vector3 headForward = head.forward;
            headForward.y = 0f;
            headForward = headForward.sqrMagnitude > 0.0001f ? headForward.normalized : Vector3.forward;

            Quaternion rotation = Quaternion.LookRotation(headForward, Vector3.up);
            Vector3 centre = head.position + headForward * distance + Vector3.up * heightOffset;
            Vector3 midpoint = (puzzle.origin + puzzle.destination) * 0.5f;
            transform.SetPositionAndRotation(centre - rotation * (midpoint * scale), rotation);
            transform.localScale = Vector3.one * scale;
        }
    }
}

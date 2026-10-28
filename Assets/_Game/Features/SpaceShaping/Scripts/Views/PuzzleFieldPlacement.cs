using System.Collections;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class PuzzleFieldPlacement : MonoBehaviour
    {
        [SerializeField] private Transform head;
        [SerializeField] private float distance = 0.6f;
        [SerializeField] private float heightOffset = -0.1f;
        [SerializeField] private float scale = 0.8f;
        [SerializeField] private float yawOffset;

        private IEnumerator Start()
        {
            while (head.position.sqrMagnitude < 0.01f)
                yield return null;

            Place();
        }

        private void Place()
        {
            Vector3 forward = head.forward;
            forward.y = 0f;
            forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;

            Vector3 position = head.position + forward * distance + Vector3.up * heightOffset;
            Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up)
                * Quaternion.Euler(0f, yawOffset, 0f);

            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = Vector3.one * scale;
        }
    }
}

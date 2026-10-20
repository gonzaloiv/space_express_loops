using System.Collections.Generic;
using DigitalLove.Global;
using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class GhostBehaviour : MonoBehaviour
    {
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private GameObject body;
        [SerializeField] private int positionBufferSize = 5;

        private Queue<Vector3> positions = new();

        public void SetActive(bool isActive)
        {
            if (isActive)
            {
                positions.Clear();
                body.transform.LocalReset();
                body.SetActive(true);
            }
            else
            {
                body.SetActive(false);
            }
        }

        private void OnEnable()
        {
            grabbable.WhenPointerEventRaised += OnPointerEvent;
        }

        private void OnDisable()
        {
            grabbable.WhenPointerEventRaised -= OnPointerEvent;
        }

        private void OnPointerEvent(PointerEvent pointer)
        {
            if (pointer.Type == PointerEventType.Move)
                OnMove();
        }

        private void OnMove()
        {
            Vector3 offset = grabbable.Transform.position - transform.position;
            Vector3 averageOffset = GetAverageOffset(offset);
            Vector3 position = transform.position + averageOffset;
            body.transform.position = position;

            Vector3 lookDir = position - transform.position;
            if (lookDir.sqrMagnitude > 0.000001f)
                body.transform.rotation = Quaternion.LookRotation(lookDir);
        }

        private Vector3 GetAverageOffset(Vector3 offset)
        {
            positions.Enqueue(offset);
            if (positions.Count > positionBufferSize)
                positions.Dequeue();
            Vector3 averageOffset = Vector3.zero;
            foreach (Vector3 pos in positions)
                averageOffset += pos;

            averageOffset /= positions.Count;
            return averageOffset;
        }

        private void OnValidate()
        {
            positionBufferSize = Mathf.Max(1, positionBufferSize);
        }
    }
}

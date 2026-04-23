using System.Collections.Generic;
using DigitalLove.Global;
using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class GhostBehaviour : MonoBehaviour
    {
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private float radius = 0.1f;
        [SerializeField] private Transform dragZone;
        [SerializeField] private GameObject body;
        [SerializeField] private int positionBufferSize = 5;

        private Queue<Vector3> positions = new Queue<Vector3>();

        public Transform Body => body.transform;

        public void SetActive(bool isActive)
        {
            if (isActive)
            {
                positions.Clear();
                dragZone.gameObject.SetActive(true);
                body.transform.LocalReset();
                body.SetActive(true);
            }
            else
            {
                dragZone.gameObject.SetActive(false);
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
            Vector3 offset = grabbable.transform.position - transform.position;
            offset = Vector3.ClampMagnitude(offset, radius);
            Vector3 averageOffset = GetAverageOffset(offset);
            Vector3 position = transform.position + averageOffset;

            Vector3 lookDir = transform.position - position;
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            body.transform.rotation = targetRot;
        }

        private Vector3 GetAverageOffset(Vector3 offset)
        {
            positions.Enqueue(offset);
            if (positions.Count > positionBufferSize)
                positions.Dequeue();
            Vector3 averageOffset = Vector3.zero;
            foreach (Vector3 pos in positions)
            {
                averageOffset += pos;
            }
            averageOffset /= positions.Count;
            return averageOffset;
        }
    }
}
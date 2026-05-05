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
        [SerializeField] private Renderer[] rends;
        [SerializeField] private int positionBufferSize = 5;

        private Queue<Vector3> positions = new Queue<Vector3>();

        public Transform Body => body.transform;

        public void SetColor(Color color)
        {
            foreach (Renderer rend in rends)
                rend.material.color = color;
        }

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
            Vector3 offset = grabbable.transform.position - transform.position;
            Vector3 averageOffset = GetAverageOffset(offset);
            Vector3 position = transform.position + averageOffset;
            body.transform.position = position;

            Vector3 lookDir = position - transform.position; // ? From center to position
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


        // ! DEBUG

        private void Update()
        {
            if (Application.isEditor)
                OnMove();
        }
    }
}
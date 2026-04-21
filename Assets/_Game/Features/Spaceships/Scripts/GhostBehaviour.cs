using DigitalLove.Global;
using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class GhostBehaviour : MonoBehaviour
    {
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private float radius = 0.1f;
        [SerializeField] private float speed = 3f;
        [SerializeField] private Transform dragZone;
        [SerializeField] private GameObject ghostBody;

        private Pose poseToMatch = new();

        public void SetActive(bool isActive)
        {
            if (isActive)
            {
                dragZone.gameObject.SetActive(true);
                ghostBody.transform.LocalReset();
                ghostBody.SetActive(true);
            }
            else
            {
                dragZone.gameObject.SetActive(false);
                ghostBody.SetActive(false);
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
            Vector3 current = grabbable.transform.position;
            Vector3 offset = current - transform.position;
            poseToMatch.position = transform.position + offset.ClampSimetric(radius);

            Vector3 lookDir = transform.position - current;
            poseToMatch.rotation = Quaternion.LookRotation(lookDir);
        }

        private void Update()
        {
            if (!ghostBody.activeInHierarchy)
                return;
            ghostBody.transform.position = Vector3.Lerp(ghostBody.transform.position, poseToMatch.position, speed * Time.deltaTime);
            ghostBody.transform.rotation = Quaternion.Lerp(ghostBody.transform.rotation, poseToMatch.rotation, speed * Time.deltaTime);
        }
    }
}
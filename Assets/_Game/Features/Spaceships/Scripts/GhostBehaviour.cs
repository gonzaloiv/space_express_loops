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
        [SerializeField] private GameObject body;
        [SerializeField] private float rotationSnap = 5f;
        [SerializeField] private float translationSnap = 0.05f;

        private Pose poseToMatch = new();

        public Transform Body => body.transform;

        public void SetActive(bool isActive)
        {
            if (isActive)
            {
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
            Vector3 targetPos = transform.position + offset.ClampSimetric(radius);
            poseToMatch.position = targetPos.Snap(translationSnap);
            body.transform.position = poseToMatch.position;

            Vector3 lookDir = transform.position - grabbable.transform.position;
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            targetRot = Quaternion.Euler(targetRot.eulerAngles.Snap(rotationSnap));
            poseToMatch.rotation = targetRot;
            body.transform.rotation = poseToMatch.rotation;
        }

        private void Update()
        {
            if (!body.activeInHierarchy)
                return;
            // body.transform.position = Vector3.Lerp(body.transform.position, poseToMatch.position, speed * Time.deltaTime);
            // body.transform.rotation = Quaternion.Lerp(body.transform.rotation, poseToMatch.rotation, speed * Time.deltaTime);
        }
    }
}
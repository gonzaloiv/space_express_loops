using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DigitalLove.Game.SpaceShaping
{
    // Types the ECO star stamp above the destination: one icon per step, with a per-star event the
    // cue player can tick. The icon count is fixed; the grade decides how many are filled.
    public class StarStampView : MonoBehaviour
    {
        [SerializeField] private RawImage[] stars;
        [SerializeField] private float starStepSecs = 0.25f;
        [SerializeField] private Color filled = new(0.62f, 0.96f, 1f, 1f);
        [SerializeField] private Color empty = new(0.62f, 0.96f, 1f, 0.18f);

        private Coroutine typing;
        private Camera view;

        public event Action<int> StarStamped;

        private Camera View => view != null ? view : view = Camera.main;

        public float Play(int grade, Vector3 worldPosition)
        {
            Stop();
            transform.position = worldPosition;
            gameObject.SetActive(true);
            int filledCount = Mathf.Clamp(grade, 0, stars.Length);
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].enabled = false;
                stars[i].color = i < filledCount ? filled : empty;
            }

            typing = StartCoroutine(Type());
            return Mathf.Max(starStepSecs, 0f) * stars.Length;
        }

        public void Stop()
        {
            StopTyping();
            gameObject.SetActive(false);
        }

        private IEnumerator Type()
        {
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].enabled = true;
                StarStamped?.Invoke(i);
                yield return new WaitForSeconds(starStepSecs);
            }

            typing = null;
        }

        private void LateUpdate()
        {
            Camera camera = View;
            if (camera == null)
                return;

            Vector3 forward = transform.position - camera.transform.position;
            if (forward.sqrMagnitude < 0.000001f)
                return;

            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }

        private void StopTyping()
        {
            if (typing == null)
                return;

            StopCoroutine(typing);
            typing = null;
        }

        private void OnDisable() => StopTyping();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class ShipTravelView : MonoBehaviour
    {
        [SerializeField] private float travelSecs = 1.5f;

        private Coroutine flight;

        public void Reset(Vector3 origin)
        {
            Stop();
            transform.localPosition = origin;
        }

        public void Travel(TrajectoryResult result)
        {
            Stop();
            flight = StartCoroutine(Fly(result));
        }

        public void Stop()
        {
            if (flight != null)
                StopCoroutine(flight);
            flight = null;
        }

        private IEnumerator Fly(TrajectoryResult result)
        {
            List<Vector3> points = result.Points;
            if (points.Count < 2)
                yield break;

            float t = 0f;
            int last = points.Count - 1;
            while (t < last)
            {
                transform.localPosition = points[(int)t];
                t += last * Time.deltaTime / travelSecs;
                yield return null;
            }

            transform.localPosition = points[last];
        }
    }
}

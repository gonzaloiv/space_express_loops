using System;
using System.Collections;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class TravellerLoopRunner
    {
        private readonly MonoBehaviour host;
        private readonly TravellerBehaviour traveller;
        private readonly float legDelay;

        private Coroutine loopCoroutine;
        private Action<LoopCompleteEventArgs> onLoopIterationComplete;

        public TravellerLoopRunner(MonoBehaviour host, TravellerBehaviour traveller, float legDelay = 1f)
        {
            this.host = host;
            this.traveller = traveller;
            this.legDelay = legDelay;
        }

        public void SetOnLoopIterationComplete(Action<LoopCompleteEventArgs> onLoopIterationComplete)
        {
            this.onLoopIterationComplete = onLoopIterationComplete;
        }

        public void StartLoop(
            SplineContainerWrapper spline,
            string spaceshipId,
            Func<int> pickLetters,
            Func<LoopEventArgs> getCurrentLoopEventArgs)
        {
            Stop();
            loopCoroutine = host.StartCoroutine(RunLoop(spline, spaceshipId, pickLetters, getCurrentLoopEventArgs));
        }

        public void Stop()
        {
            if (loopCoroutine != null)
            {
                host.StopCoroutine(loopCoroutine);
                loopCoroutine = null;
            }

            traveller.HideAndCancelPath();
        }

        private IEnumerator RunLoop(
            SplineContainerWrapper spline,
            string spaceshipId,
            Func<int> pickLetters,
            Func<LoopEventArgs> getCurrentLoopEventArgs)
        {
            while (true)
            {
                int pickedLetters = 0;
                traveller.ShowEmpty();

                bool goLegSucceeded = false;
                yield return FollowPathLeg(spline.GoPositions, success => goLegSucceeded = success);
                if (!goLegSucceeded)
                {
                    onLoopIterationComplete?.Invoke(new LoopCompleteEventArgs(spaceshipId, pickedLetters));
                    continue;
                }

                yield return new WaitForSeconds(legDelay);

                pickedLetters = pickLetters();
                traveller.ShowLoaded(pickedLetters);

                bool returnLegSucceeded = false;
                yield return FollowPathLeg(spline.ReturnPositions, success => returnLegSucceeded = success);
                if (!returnLegSucceeded)
                {
                    onLoopIterationComplete?.Invoke(new LoopCompleteEventArgs(spaceshipId, pickedLetters));
                    continue;
                }

                yield return new WaitForSeconds(legDelay);
                onLoopIterationComplete?.Invoke(new LoopCompleteEventArgs(getCurrentLoopEventArgs(), pickedLetters));
            }
        }

        private IEnumerator FollowPathLeg(Vector3[] path, Action<bool> onComplete)
        {
            if (path == null || path.Length < 2)
            {
                onComplete(false);
                yield break;
            }

            bool? completedSuccessfully = null;
            traveller.FollowPath(path, success => completedSuccessfully = success);
            yield return new WaitUntil(() => completedSuccessfully.HasValue || !traveller.IsFollowingPath);
            onComplete(completedSuccessfully.GetValueOrDefault(false));
        }
    }
}

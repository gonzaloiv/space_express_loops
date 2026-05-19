using System;
using System.Collections;
using UnityEngine;
using DigitalLove.Game.Planets;

namespace DigitalLove.Game.Spaceships
{
    public class TravellerLoopRunner
    {
        private readonly MonoBehaviour host;
        private readonly TravellerBehaviour traveller;
        private readonly float legDelay;

        private Coroutine loopCoroutine;
        private Action<LoopCompleteEventArgs> onLoopIterationComplete;
        private Func<TravellerRoutePlan> getRoutePlan;

        public bool IsRunning => loopCoroutine != null;

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
            Func<TravellerRoutePlan> getRoutePlan,
            string spaceshipId,
            Func<int, int> pickLettersAtStop,
            Func<LoopEventArgs> getCurrentLoopEventArgs)
        {
            this.getRoutePlan = getRoutePlan;
            if (IsRunning)
                Stop();

            loopCoroutine = host.StartCoroutine(RunLoop(spaceshipId, pickLettersAtStop, getCurrentLoopEventArgs));
        }

        public void Stop()
        {
            if (loopCoroutine != null)
            {
                host.StopCoroutine(loopCoroutine);
                loopCoroutine = null;
            }

            getRoutePlan = null;
            traveller.HideAndCancelPath();
        }

        private IEnumerator RunLoop(
            string spaceshipId,
            Func<int, int> pickLettersAtStop,
            Func<LoopEventArgs> getCurrentLoopEventArgs)
        {
            while (true)
            {
                TravellerRoutePlan routePlan = getRoutePlan?.Invoke();
                if (routePlan == null || routePlan.Segments == null || routePlan.Segments.Count == 0)
                {
                    yield return null;
                    continue;
                }

                int totalPickedLetters = 0;
                traveller.ShowEmpty();
                bool iterationFailed = false;

                for (int stopIndex = 0; stopIndex < routePlan.Segments.Count; stopIndex++)
                {
                    routePlan = getRoutePlan?.Invoke();
                    if (routePlan == null || stopIndex >= routePlan.Segments.Count)
                        break;

                    Vector3[] segment = routePlan.Segments[stopIndex];
                    bool isPickupStop = stopIndex < routePlan.PickupPlanets.Count;
                    PlanetBehaviour pickupPlanet = isPickupStop ? routePlan.PickupPlanets[stopIndex] : null;
                    bool legSucceeded = false;
                    yield return FollowPathLeg(segment, pickupPlanet, success => legSucceeded = success);
                    if (!legSucceeded)
                    {
                        iterationFailed = true;
                        break;
                    }

                    yield return new WaitForSeconds(legDelay);

                    routePlan = getRoutePlan?.Invoke();
                    if (routePlan != null && stopIndex < routePlan.PickupPlanets.Count)
                    {
                        int pickedAtStop = pickLettersAtStop(stopIndex);
                        totalPickedLetters += pickedAtStop;
                        traveller.ShowLoaded(totalPickedLetters);
                    }
                }

                if (iterationFailed)
                {
                    onLoopIterationComplete?.Invoke(new LoopCompleteEventArgs(spaceshipId, totalPickedLetters));
                    continue;
                }

                yield return new WaitForSeconds(legDelay);
                onLoopIterationComplete?.Invoke(new LoopCompleteEventArgs(getCurrentLoopEventArgs(), totalPickedLetters));
            }
        }

        private IEnumerator FollowPathLeg(Vector3[] path, PlanetBehaviour pickupPlanet, Action<bool> onComplete)
        {
            if (path == null || path.Length < 2)
            {
                onComplete(false);
                yield break;
            }

            bool? completedSuccessfully = null;
            traveller.FollowPath(path, pickupPlanet, success => completedSuccessfully = success);
            yield return new WaitUntil(() => completedSuccessfully.HasValue || !traveller.IsFollowingPath);
            onComplete(completedSuccessfully.GetValueOrDefault(false));
        }
    }
}

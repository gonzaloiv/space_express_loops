using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalLove.Game.Nodes;

namespace DigitalLove.Game.Spaceships
{
    public class TravellerRouteRunner
    {
        private readonly MonoBehaviour host;
        private readonly SplineContainerWrapper route;
        private readonly TravellerBehaviour traveller;
        private readonly float legDelay;

        private Coroutine loopCoroutine;
        private Action<LoopCompleteEventArgs> onLoopIterationComplete;
        private bool isPaused;

        public bool IsRunning => loopCoroutine != null;

        public TravellerRouteRunner(
            MonoBehaviour host,
            SplineContainerWrapper route,
            TravellerBehaviour traveller,
            float legDelay = 1f)
        {
            this.host = host;
            this.route = route;
            this.traveller = traveller;
            this.legDelay = legDelay;
        }

        public void SetOnLoopIterationComplete(Action<LoopCompleteEventArgs> onLoopIterationComplete)
        {
            this.onLoopIterationComplete = onLoopIterationComplete;
        }

        public void StartLoop(string spaceshipId, Func<LoopEventArgs> getCurrentLoopEventArgs)
        {
            if (IsRunning)
                Stop();
            loopCoroutine = host.StartCoroutine(RunLoop(spaceshipId, getCurrentLoopEventArgs));
        }

        public void Stop()
        {
            if (loopCoroutine != null)
            {
                host.StopCoroutine(loopCoroutine);
                loopCoroutine = null;
            }
            isPaused = false;
            traveller.HideAndCancelPath();
        }

        public void Pause()
        {
            isPaused = true;
            traveller.PauseFollowing();
        }

        public void Resume()
        {
            isPaused = false;
            traveller.ResumeFollowing();
        }

        private IEnumerator RunLoop(string spaceshipId, Func<LoopEventArgs> getCurrentLoopEventArgs)
        {
            while (true)
            {
                IReadOnlyList<RouteLeg> legs = route.Legs;
                if (legs.Count == 0)
                {
                    yield return null;
                    continue;
                }

                int totalPickedLetters = 0;
                traveller.ShowEmpty();
                bool iterationFailed = false;

                for (int stopIndex = 0; stopIndex < legs.Count; stopIndex++)
                {
                    while (isPaused)
                        yield return null;

                    RouteLeg leg = legs[stopIndex];
                    route.ShowLeg(stopIndex);

                    bool legSucceeded = false;
                    yield return FollowPathLeg(leg.positions, leg.pickupPlanet, success => legSucceeded = success);
                    if (!legSucceeded)
                    {
                        iterationFailed = true;
                        break;
                    }

                    yield return WaitForLegDelay();

                    if (leg.pickupPlanet != null)
                    {
                        totalPickedLetters += leg.pickupPlanet.PlanetStore.PickAllLetters();
                        traveller.ShowLoaded(totalPickedLetters);
                    }
                }

                if (iterationFailed)
                {
                    onLoopIterationComplete?.Invoke(new LoopCompleteEventArgs(spaceshipId, totalPickedLetters));
                    continue;
                }

                yield return WaitForLegDelay();
                onLoopIterationComplete?.Invoke(new LoopCompleteEventArgs(getCurrentLoopEventArgs(), totalPickedLetters));
            }
        }

        private IEnumerator WaitForLegDelay()
        {
            float elapsed = 0f;
            while (elapsed < legDelay)
            {
                if (isPaused)
                {
                    yield return null;
                    continue;
                }

                elapsed += Time.deltaTime;
                yield return null;
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

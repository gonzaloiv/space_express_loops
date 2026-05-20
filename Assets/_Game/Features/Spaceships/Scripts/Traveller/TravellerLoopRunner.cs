using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalLove.Game.Planets;

namespace DigitalLove.Game.Spaceships
{
    public class TravellerLoopRunner
    {
        private readonly MonoBehaviour host;
        private readonly RouteContainer route;
        private readonly TravellerBehaviour traveller;
        private readonly float legDelay;

        private Coroutine loopCoroutine;
        private Action<LoopCompleteEventArgs> onLoopIterationComplete;

        public bool IsRunning => loopCoroutine != null;

        public TravellerLoopRunner(
            MonoBehaviour host,
            RouteContainer route,
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

            traveller.HideAndCancelPath();
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
                    RouteLeg leg = legs[stopIndex];
                    route.ShowLeg(stopIndex);

                    bool legSucceeded = false;
                    yield return FollowPathLeg(leg.positions, leg.pickupPlanet, success => legSucceeded = success);
                    if (!legSucceeded)
                    {
                        iterationFailed = true;
                        break;
                    }

                    yield return new WaitForSeconds(legDelay);

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

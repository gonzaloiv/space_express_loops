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
        private readonly TravellerBehaviour traveller;
        private readonly float legDelay;

        private Coroutine loopCoroutine;
        private Action<LoopCompleteEventArgs> onLoopIterationComplete;
        private Func<IReadOnlyList<RouteLegPath>> getLegs;

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
            Func<IReadOnlyList<RouteLegPath>> getLegs,
            string spaceshipId,
            Func<int, int> pickLettersAtStop,
            Func<LoopEventArgs> getCurrentLoopEventArgs)
        {
            this.getLegs = getLegs;
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

            getLegs = null;
            traveller.HideAndCancelPath();
        }

        private IEnumerator RunLoop(
            string spaceshipId,
            Func<int, int> pickLettersAtStop,
            Func<LoopEventArgs> getCurrentLoopEventArgs)
        {
            while (true)
            {
                IReadOnlyList<RouteLegPath> legs = getLegs?.Invoke();
                if (legs == null || legs.Count == 0)
                {
                    yield return null;
                    continue;
                }

                int totalPickedLetters = 0;
                traveller.ShowEmpty();
                bool iterationFailed = false;

                for (int stopIndex = 0; stopIndex < legs.Count; stopIndex++)
                {
                    legs = getLegs?.Invoke();
                    if (legs == null || stopIndex >= legs.Count)
                        break;

                    RouteLegPath leg = legs[stopIndex];
                    bool legSucceeded = false;
                    yield return FollowPathLeg(leg.Positions, leg.PickupPlanet, success => legSucceeded = success);
                    if (!legSucceeded)
                    {
                        iterationFailed = true;
                        break;
                    }

                    yield return new WaitForSeconds(legDelay);

                    legs = getLegs?.Invoke();
                    if (legs != null && stopIndex < legs.Count && legs[stopIndex].PickupPlanet != null)
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

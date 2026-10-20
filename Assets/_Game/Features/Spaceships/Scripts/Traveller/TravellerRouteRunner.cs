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
        private readonly float legDelaySecs;

        private Coroutine loopCoroutine;
        private Action<LoopCompleteEventArgs> onLoopIterationComplete;
        private bool isPaused;

        public bool IsRunning => loopCoroutine != null;

        public TravellerRouteRunner(
            MonoBehaviour host,
            SplineContainerWrapper route,
            TravellerBehaviour traveller,
            float legDelaySecs = 1f)
        {
            this.host = host;
            this.route = route;
            this.traveller = traveller;
            this.legDelaySecs = legDelaySecs;
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

        private sealed class LoopIteration
        {
            public int PickedLetters;
            public bool Failed;
        }

        private IEnumerator RunLoop(string spaceshipId, Func<LoopEventArgs> getCurrentLoopEventArgs)
        {
            while (true)
            {
                if (route.Legs.Count == 0)
                {
                    yield return null;
                    continue;
                }

                yield return RunIteration(spaceshipId, getCurrentLoopEventArgs, new LoopIteration());
            }
        }

        private IEnumerator RunIteration(
            string spaceshipId,
            Func<LoopEventArgs> getCurrentLoopEventArgs,
            LoopIteration iteration)
        {
            traveller.ShowEmpty();
            yield return RunLegs(iteration);

            if (iteration.Failed)
            {
                onLoopIterationComplete?.Invoke(new LoopCompleteEventArgs(spaceshipId, iteration.PickedLetters));
                yield break;
            }

            yield return WaitForLegDelay();
            onLoopIterationComplete?.Invoke(new LoopCompleteEventArgs(getCurrentLoopEventArgs(), iteration.PickedLetters));
        }

        private IEnumerator RunLegs(LoopIteration iteration)
        {
            IReadOnlyList<RouteLeg> legs = route.Legs;
            for (int stopIndex = 0; stopIndex < legs.Count; stopIndex++)
            {
                while (isPaused)
                    yield return null;

                yield return RunLeg(legs[stopIndex], stopIndex, iteration);
                if (iteration.Failed)
                    yield break;
            }
        }

        private IEnumerator RunLeg(RouteLeg leg, int stopIndex, LoopIteration iteration)
        {
            route.ShowLeg(stopIndex);

            bool legSucceeded = false;
            yield return FollowPathLeg(leg.positions, leg.pickupPlanet, success => legSucceeded = success);
            if (!legSucceeded)
            {
                iteration.Failed = true;
                yield break;
            }

            yield return WaitForLegDelay();

            if (leg.pickupPlanet == null)
                yield break;

            iteration.PickedLetters += leg.pickupPlanet.PlanetStore.PickAllLetters();
            traveller.ShowLoaded(iteration.PickedLetters);
        }

        private IEnumerator WaitForLegDelay()
        {
            float elapsed = 0f;
            while (elapsed < legDelaySecs)
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

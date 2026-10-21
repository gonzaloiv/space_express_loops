using System;
using System.Collections;
using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using DigitalLove.Game.Persistence;
using DigitalLove.Game.Spaceships;
using DigitalLove.XR.MRUtilityKit;
using Meta.XR.MRUtilityKit;
using UnityEngine;

namespace DigitalLove.Game.Levels
{
    public class LevelContainer : MonoBehaviour
    {
        [SerializeField] private MRUKRoomAnchorsContainer mrukRoomAnchorsContainer;
        [SerializeField] private MrukRoomLocalPlacement roomPlacement;
        [SerializeField] private PlanetsSpawner planetsSpawner;
        [SerializeField] private SpaceshipsSpawner spaceshipsSpawner;
        [SerializeField] private HubsSpawner hubsSpawner;
        [SerializeField] private UniverseView universeView;

        private GameSnapshot currentSnapshot;
        private bool hasCenteredContent;

        public PlanetsSpawner PlanetsSpawner => planetsSpawner;
        public SpaceshipsSpawner SpaceshipsSpawner => spaceshipsSpawner;
        public HubsSpawner HubsSpawner => hubsSpawner;
        public UniverseView UniverseView => universeView;

        private void OnEnable() => Subscribe();

        private void OnDisable() => Unsubscribe();

        public void Init()
        {
            spaceshipsSpawner.SetPlanetAvailability(new LevelLoopPlanetAvailability(planetsSpawner, hubsSpawner));
            Subscribe();
            HideAll();
        }

        private void Subscribe()
        {
            universeView.TransitionStarted -= OnViewTransitionStarted;
            universeView.TransitionStarted += OnViewTransitionStarted;
            universeView.TransitionCompleted -= OnViewTransitionCompleted;
            universeView.TransitionCompleted += OnViewTransitionCompleted;
            universeView.BubbleTransformCommitted -= OnBubbleTransformCommitted;
            universeView.BubbleTransformCommitted += OnBubbleTransformCommitted;
        }

        private void Unsubscribe()
        {
            universeView.TransitionStarted -= OnViewTransitionStarted;
            universeView.TransitionCompleted -= OnViewTransitionCompleted;
            universeView.BubbleTransformCommitted -= OnBubbleTransformCommitted;
        }

        private void OnViewTransitionStarted(bool _) =>
            spaceshipsSpawner.SetViewTransition(true);

        private void OnViewTransitionCompleted(bool _)
        {
            spaceshipsSpawner.SetViewTransition(false);
            spaceshipsSpawner.RefreshGrabbablesAtStation();
        }

        private void OnBubbleTransformCommitted()
        {
            if (currentSnapshot == null)
                return;

            currentSnapshot.SaveBubbleTransform(universeView.BubbleOffsetLocal, universeView.BubbleYaw);
        }

        public void SyncIdCounters(GameSnapshot gameSnapshot)
        {
            List<string> planetIds = new(gameSnapshot.planets.Count);
            foreach (PlanetData planet in gameSnapshot.planets)
                planetIds.Add(planet.id);
            planetsSpawner.SyncIdsFromSnapshot(planetIds);

            List<string> spaceshipIds = new(gameSnapshot.loops.Count);
            foreach (LoopData loop in gameSnapshot.loops)
                spaceshipIds.Add(loop.spaceshipId);
            spaceshipsSpawner.SyncIdsFromSnapshot(spaceshipIds);

            List<string> hubIds = new(gameSnapshot.hubs.Count);
            foreach (HubData hub in gameSnapshot.hubs)
                hubIds.Add(hub.id);
            hubsSpawner.SyncIdsFromSnapshot(hubIds);
        }

        public void HideAll()
        {
            planetsSpawner.ResetPool();
            spaceshipsSpawner.ResetPool();
            hubsSpawner.ResetPool();
            roomPlacement.Clear();
        }

        public void ResetForRestart()
        {
            HideAll();
            mrukRoomAnchorsContainer.ClearRoomAnchors();
        }

        public void SpawnRound(RoundData roundData, GameSnapshot gameSnapshot)
        {
            currentSnapshot = gameSnapshot;
            universeView.WithRoomPose(() => SpawnRoundInternal(roundData, gameSnapshot));
            CenterOnSpawnedContent();
        }

        private void SpawnRoundInternal(RoundData roundData, GameSnapshot gameSnapshot)
        {
            SyncIdCounters(gameSnapshot);

            if (roundData.shouldSpawnSpaceship)
                SpawnSpaceship(gameSnapshot);

            SpawnPlanets(gameSnapshot, roundData);
            FinalizeSpawn(gameSnapshot);
        }

        public void RestoreFromSnapshot(GameSnapshot gameSnapshot, Action onComplete)
        {
            currentSnapshot = gameSnapshot;
            SetRoomBasedPose(() =>
            {
                if (gameSnapshot.hasBubbleTransform)
                    universeView.RestoreBubbleTransform(gameSnapshot.bubbleOffsetLocal, gameSnapshot.bubbleYaw);

                universeView.WithRoomPose(() => RespawnFromData(gameSnapshot));
                CenterOnSpawnedContent();
                onComplete?.Invoke();
            });
        }

        private void SpawnSpaceship(GameSnapshot gameSnapshot)
        {
            HubBehaviour hub = hubsSpawner.SpawnNew();
            gameSnapshot.AddHub(hubsSpawner.CreateHubData(hub));
            SpaceshipBehaviour spaceship = spaceshipsSpawner.SpawnNew(hub);
            gameSnapshot.SaveLoop(new LoopData
            {
                spaceshipId = spaceship.Id,
                colorCode = spaceship.ColorCode,
                hubId = spaceship.HubId
            });
        }

        private void SpawnPlanets(GameSnapshot gameSnapshot, RoundData roundData)
        {
            List<PlanetData> roundPlanets = planetsSpawner.GeneratePlanetDataFromPlanetsSeed(
                roundData.planetsToAdd.GetRandomValue(),
                roundData.planetSeed,
                roundData.distanceBetweenPlanetsMultiplier
            );
            gameSnapshot.AddPlanets(roundPlanets);
            planetsSpawner.SpawnPlanets(gameSnapshot.planets);
        }

        private void RespawnFromData(GameSnapshot gameSnapshot)
        {
            roomPlacement.SyncFromSnapshot(
                gameSnapshot.hubs,
                gameSnapshot.planets,
                hubsSpawner.HubPlacementRadius);
            hubsSpawner.SpawnHubs(gameSnapshot.hubs);
            planetsSpawner.SpawnPlanets(gameSnapshot.planets);

            foreach (LoopData loop in gameSnapshot.loops)
                SpawnSpaceshipFromLoop(loop, gameSnapshot);

            FinalizeSpawn(gameSnapshot);
        }

        private void FinalizeSpawn(GameSnapshot gameSnapshot)
        {
            PlanetRouteColorSync.Apply(gameSnapshot, planetsSpawner, hubsSpawner, spaceshipsSpawner);
            spaceshipsSpawner.RefreshGrabbablesAtStation();
        }

        private void CenterOnSpawnedContent()
        {
            if (hasCenteredContent)
                return;

            hasCenteredContent = LevelContentCenter.TrySet(universeView, hubsSpawner.All, planetsSpawner.All);
        }

        private void SpawnSpaceshipFromLoop(LoopData loop, GameSnapshot gameSnapshot)
        {
            HubBehaviour hub = hubsSpawner.GetById(loop.hubId)
                ?? hubsSpawner.GetOrSpawn(loop.hubId, gameSnapshot.GetHubById(loop.hubId));
            List<PlanetBehaviour> destinations = loop.ResolveDestinations(planetsSpawner.GetById);
            spaceshipsSpawner.SpawnRestored(loop.spaceshipId, hub, destinations, loop.colorCode);
        }

        public void StartFresh(Action onComplete)
        {
            currentSnapshot = null;
            mrukRoomAnchorsContainer.Clear();
            SetRoomBasedPose(onComplete);
        }

        public void SetRoomBasedPose(Action onComplete)
        {
            StartCoroutine(SetRoomBasedPoseRoutine(onComplete));
        }

        private IEnumerator SetRoomBasedPoseRoutine(Action onComplete)
        {
            yield return WaitForRoomReady();

            hasCenteredContent = false;
            MRUKRoom room = MRUK.Instance.GetCurrentRoom();
            Bounds roomBounds = room.GetRoomBounds();
            Pose originPose = new()
            {
                position = room.Center(),
                rotation = room.transform.rotation
            };

            mrukRoomAnchorsContainer.InitAndLoadRoomAnchors(originPose, _ =>
            {
                if (!mrukRoomAnchorsContainer.TryGetAnchorPose(out Pose toSet))
                {
                    Debug.LogWarning("[LevelContainer] Failed to load room anchor; falling back to room center.");
                    toSet = originPose;
                }

                universeView.SetRoomAnchor(toSet, roomBounds);
                onComplete?.Invoke();
            });
        }

        private static IEnumerator WaitForRoomReady()
        {
            while (MRUK.Instance == null
                || !MRUK.Instance.IsInitialized
                || MRUK.Instance.GetCurrentRoom() == null
                || Camera.main == null)
            {
                yield return null;
            }

            yield return null;
        }

        public void ResetLetters()
        {
            for (int i = 0; i < planetsSpawner.All.Count; i++)
            {
                PlanetBehaviour planet = planetsSpawner.All[i];
                if (planet != null && planet.IsActive)
                    planet.PlanetStore.ResetLetters();
            }
        }
    }
}

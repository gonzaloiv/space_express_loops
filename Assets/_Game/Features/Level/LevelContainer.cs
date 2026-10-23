using System;
using System.Collections;
using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using DigitalLove.Game.Persistence;
using DigitalLove.Game.Sectors;
using DigitalLove.Game.SilentPlanets;
using DigitalLove.Game.Universe;
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
        [SerializeField] private SectorFieldSpawner sectorFieldSpawner;
        [SerializeField] private UniverseView universeView;

        private GameSnapshot currentSnapshot;
        private LevelViewModeFanout viewModeFanout;

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

        private void ApplyContentFrame()
        {
            universeView.SetContentCenter(universeView.ContentPosition);
            universeView.SetContentRadius(sectorFieldSpawner.PlayRadius);
        }

        private void Subscribe()
        {
            viewModeFanout ??= new LevelViewModeFanout(
                universeView,
                spaceshipsSpawner,
                sectorFieldSpawner);
            viewModeFanout.Subscribe();
            universeView.BubbleTransformCommitted -= OnBubbleTransformCommitted;
            universeView.BubbleTransformCommitted += OnBubbleTransformCommitted;
        }

        private void Unsubscribe()
        {
            viewModeFanout?.Unsubscribe();
            universeView.BubbleTransformCommitted -= OnBubbleTransformCommitted;
        }

        private void OnBubbleTransformCommitted()
        {
            if (currentSnapshot == null)
                return;

            BubblePlacement placement = universeView.BubblePlacement;
            currentSnapshot.SaveBubbleTransform(placement.OffsetLocal, placement.Yaw);
        }

        public void SyncIdCounters(GameSnapshot gameSnapshot) =>
            SpawnerIdSync.Sync(gameSnapshot, planetsSpawner, spaceshipsSpawner, hubsSpawner);

        public void HideAll()
        {
            sectorFieldSpawner.ResetPool();
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
            ApplyContentFrame();
            universeView.WithRoomPose(() => SpawnRoundInternal(roundData, gameSnapshot));
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
                if (gameSnapshot.HasBubblePlacement)
                    universeView.RestoreBubbleTransform(gameSnapshot.BubbleOffsetLocal, gameSnapshot.BubbleYaw);

                ApplyContentFrame();
                universeView.WithRoomPose(() => RespawnFromData(gameSnapshot));
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
            sectorFieldSpawner.SpawnIfNeeded(gameSnapshot.EnsureSilentPlanetSeed());
            SectorFieldData field = sectorFieldSpawner.Field;
            int offset = gameSnapshot.planets.Count;
            int count = roundData.planetsToAdd.GetRandomValue();
            SectorSelection[] selections = SectorSelector.SelectDestinations(
                field,
                field.worldSeed,
                offset,
                count);

            List<Vector3> positions = new(selections.Length);
            for (int i = 0; i < selections.Length; i++)
                positions.Add(selections[i].Position);

            List<PlanetData> roundPlanets = planetsSpawner.GeneratePlanetDataFromPlanetsSeed(
                positions.Count,
                roundData.planetSeed,
                positions
            );
            gameSnapshot.AddPlanets(roundPlanets);
            planetsSpawner.SpawnPlanets(gameSnapshot.planets);
        }

        private void RespawnFromData(GameSnapshot gameSnapshot)
        {
            roomPlacement.SyncFromSnapshot(gameSnapshot.hubs, hubsSpawner.HubPlacementRadius);
            hubsSpawner.SpawnHubs(gameSnapshot.hubs);
            planetsSpawner.SpawnPlanets(gameSnapshot.planets);

            foreach (LoopData loop in gameSnapshot.loops)
                SpawnSpaceshipFromLoop(loop, gameSnapshot);

            FinalizeSpawn(gameSnapshot);
        }

        private void FinalizeSpawn(GameSnapshot gameSnapshot)
        {
            viewModeFanout.ApplyCurrentMode();
            sectorFieldSpawner.SpawnIfNeeded(gameSnapshot.EnsureSilentPlanetSeed());
            PlanetRouteColorSync.Apply(gameSnapshot, planetsSpawner, hubsSpawner, spaceshipsSpawner);
            spaceshipsSpawner.RefreshGrabbablesAtStation();
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

            MRUKRoom room = MRUK.Instance.GetCurrentRoom();
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

                universeView.SetRoomAnchor(toSet);
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

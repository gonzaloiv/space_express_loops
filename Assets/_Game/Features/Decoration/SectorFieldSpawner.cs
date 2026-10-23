using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using DigitalLove.Game.Sectors;
using UnityEngine;

namespace DigitalLove.Game.SilentPlanets
{
    public class SectorFieldSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SilentPlanetBehaviour prototype;
        [SerializeField] private SectorFieldData field = new();
        [SerializeField] private Transform viewTransform;

        [Header("Visibility")]
        [SerializeField, Min(1f)] private float hideRadiusFactor = 2f;
        [SerializeField, Min(0f)] private float roomVisibleRadius = 3f;

        private static readonly int ScopeCenterId = Shader.PropertyToID("_ColorScopeCenter");
        private static readonly int ScopeRadiiId = Shader.PropertyToID("_ColorScopeRadii");
        private static readonly int ScopeStrengthId = Shader.PropertyToID("_ColorScopeStrength");

        private readonly List<SilentPlanetBehaviour> planets = new();
        private bool hasSpawned;
        private bool isBubbleView;

        public SectorFieldData Field => field;
        public float PlayRadius => field.destinationRadius > 0f ? field.destinationRadius : field.fieldRadius;

        public void ResetPool()
        {
            HideAll();
            planets.ForEachInPool(planet => planet.Initialize());
        }

        public void SetBubbleView(bool bubble) => isBubbleView = bubble;

        public void SpawnIfNeeded(int seed)
        {
            if (hasSpawned)
                return;

            hasSpawned = true;
            field.worldSeed = seed;
            SectorPlacement[] placements = SectorField.Generate(field);
            for (int i = 0; i < placements.Length; i++)
                SpawnSector(placements[i]);
        }

        private void LateUpdate()
        {
            if (!hasSpawned)
                return;

            if (isBubbleView)
                UpdateBubbleVisibility();
            else
                UpdateRoomVisibility();
        }

        private void SpawnSector(SectorPlacement placement)
        {
            SectorLayout layout = placement.Layout;
            int seed = SectorGrid.CellSeed(field.worldSeed, placement.Cell);
            Acquire().Spawn(placement.Center, layout.AnchorRadius, seed);

            for (int i = 0; i < layout.PlanetCount; i++)
            {
                SectorPlanet planet = layout.Planets[i];
                Acquire().Spawn(placement.Center + planet.Position, planet.Radius, seed + i + 1);
            }
        }

        private void UpdateBubbleVisibility()
        {
            if (Shader.GetGlobalFloat(ScopeStrengthId) <= 0f)
            {
                ShowAll();
                return;
            }

            Vector3 center = Shader.GetGlobalVector(ScopeCenterId);
            Vector3 extents = Shader.GetGlobalVector(ScopeRadiiId) * Mathf.Max(hideRadiusFactor, 1f);
            if (extents.x <= 0f || extents.y <= 0f || extents.z <= 0f)
            {
                ShowAll();
                return;
            }

            for (int i = 0; i < planets.Count; i++)
            {
                SilentPlanetBehaviour planet = planets[i];
                if (planet == null || !planet.IsActive)
                    continue;

                planet.SetVisible(IsInsideScope(planet.Position, center, extents));
            }
        }

        private static bool IsInsideScope(Vector3 position, Vector3 center, Vector3 extents)
        {
            Vector3 local = position - center;
            Vector3 normalized = new(local.x / extents.x, local.y / extents.y, local.z / extents.z);
            return normalized.sqrMagnitude <= 1f;
        }

        private void UpdateRoomVisibility()
        {
            Vector3 eye = viewTransform.position;
            float sqrRadius = roomVisibleRadius * roomVisibleRadius;
            for (int i = 0; i < planets.Count; i++)
            {
                SilentPlanetBehaviour planet = planets[i];
                if (planet == null || !planet.IsActive)
                    continue;

                planet.SetVisible((planet.Position - eye).sqrMagnitude <= sqrRadius);
            }
        }

        private void ShowAll() => planets.ForEachInPool(planet =>
        {
            if (planet.IsActive)
                planet.SetVisible(true);
        });

        private void HideAll()
        {
            planets.ForEachInPool(planet => planet.Hide());
            hasSpawned = false;
        }

        private SilentPlanetBehaviour Acquire() =>
            planets.AcquireInactive(prototype, transform, created =>
            {
                created.Initialize();
                created.Hide();
            });
    }
}

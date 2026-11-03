using System;
using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The zone cube set: lazy slots, per-puzzle framing, and bulk conditions, prediction, and refusal.
    public class ZoneCubePool
    {
        private readonly ViewPool<SpaceZoneView> pool;
        private ZoneConditions conditions;
        private float falloff = 0.5f;
        private float minScale = 0.6f;
        private float maxScale = 1.8f;

        public ZoneCubePool(SpaceZoneView prototype, Transform parent) =>
            pool = new ViewPool<SpaceZoneView>(prototype, parent, "Zone", OnCreated);

        public event Action<ZoneRefusal> Refused;

        public SpaceZone Zone
        {
            get
            {
                SpaceZoneView cube = pool.Views.Active();
                return cube != null ? cube.Zone : default;
            }
        }

        public bool IsScaling => pool.Views.AnyScaling();

        public void SetConditions(ZoneConditions value)
        {
            conditions = value;
            pool.Views.ApplyConditions(value);
        }

        public void SetScaleEnvelope(float min, float max)
        {
            minScale = min;
            maxScale = max;
            pool.Views.ApplyScaleEnvelope(min, max);
        }

        public void ShowFrames(PuzzleFrame[] frames, float zoneFalloff)
        {
            falloff = zoneFalloff;
            int count = frames != null ? frames.Length : 0;
            for (int i = 0; i < count; i++)
                Place(pool.EnsureSlot(i), frames[i]);

            IReadOnlyList<SpaceZoneView> views = pool.Views;
            for (int i = count; i < views.Count; i++)
                views[i].gameObject.SetActive(false);
        }

        public void ApplyPrediction(ZoneVerdict verdict) => pool.Views.ApplyPrediction(verdict);

        public void ClearPredictions() => pool.Views.ClearPredictions();

        public void Refuse(ZoneRefusal reason)
        {
            SpaceZoneView cube = pool.Views.Active();
            if (cube != null)
                cube.Refuse(reason);
        }

        private void Place(SpaceZoneView cube, PuzzleFrame frame)
        {
            cube.SetConditions(conditions);
            cube.SetScaleEnvelope(minScale, maxScale);
            cube.Place(frame.centre, frame.BoxSize, falloff);
        }

        private void OnCreated(SpaceZoneView cube) => cube.Refused += OnRefused;

        private void OnRefused(ZoneRefusal reason) => Refused?.Invoke(reason);
    }
}

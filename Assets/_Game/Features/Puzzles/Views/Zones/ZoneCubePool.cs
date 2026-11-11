using System;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The zone cube set: lazy slots, per-puzzle framing, and bulk conditions, prediction, and refusal.
    public class ZoneCubePool
    {
        private readonly ViewPool<CubeView> pool;
        private ZoneConditions conditions;
        private float falloff = 0.5f;
        private float minScale = 0.6f;
        private float maxScale = 1.8f;
        private bool inputLocked;

        public ZoneCubePool(CubeView prototype, Transform parent) =>
            pool = new ViewPool<CubeView>(prototype, parent, "Zone", OnCreated);

        public event Action<ZoneRefusal> Refused;

        public event Action MoveEnded;

        public event Action<int> OutOfBoundsAt;

        public event Action Moved;

        public SpaceZone Zone
        {
            get
            {
                CubeView cube = pool.Views.Active();
                return cube != null ? cube.Zone : default;
            }
        }

        public CubeMoveRefusal MoveRefusal
        {
            get
            {
                for (int i = 0; i < pool.Views.Count; i++)
                {
                    CubeView cube = pool.Views[i];
                    if (!cube.gameObject.activeSelf)
                        continue;

                    CubeMoveRefusal refusal = cube.MoveRefusal;
                    if (refusal.Blocked)
                        return refusal;
                }

                return default;
            }
        }

        public Color Danger
        {
            get
            {
                CubeView cube = pool.Views.Active();
                return cube != null ? cube.Danger : Color.white;
            }
        }

        public bool IsScaling => pool.Views.AnyScaling();

        public bool IsHeld => pool.Views.AnyHeld();

        public Vector3 ActivePosition
        {
            get
            {
                CubeView cube = pool.Views.Active();
                return cube != null ? cube.transform.position : Vector3.zero;
            }
        }

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

            pool.ForEachFrom(count, view => view.gameObject.SetActive(false));
        }

        public void ApplyPrediction(ZoneVerdict verdict) => pool.Views.ApplyPrediction(verdict);

        public void ClearPredictions() => pool.Views.ClearPredictions();

        public void SetStrain(float value) => pool.Views.ApplyStrain(value);

        public void Refuse(ZoneRefusal reason)
        {
            CubeView cube = pool.Views.Active();
            if (cube != null)
                cube.Refuse(reason);
        }

        public void SetInputLocked(bool value)
        {
            inputLocked = value;
            pool.ForEachFrom(0, view => view.SetInputLocked(value));
        }

        private void Place(CubeView cube, PuzzleFrame frame)
        {
            cube.SetConditions(conditions);
            cube.SetScaleEnvelope(minScale, maxScale);
            cube.Place(frame.centre, frame.BoxSize, falloff);
        }

        private void OnCreated(CubeView cube)
        {
            cube.SetInputLocked(inputLocked);
            cube.Refused += OnRefused;
            cube.Moved += OnMoved;
            cube.MoveEnded += OnMoveEnded;
            cube.OutOfBoundsAt += OnOutOfBoundsAt;
        }

        private void OnRefused(ZoneRefusal reason) => Refused?.Invoke(reason);

        private void OnMoved() => Moved?.Invoke();

        private void OnMoveEnded() => MoveEnded?.Invoke();

        private void OnOutOfBoundsAt(int endpoint) => OutOfBoundsAt?.Invoke(endpoint);
    }
}

using System;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;
using DigitalLove.Game.Spatial;

namespace DigitalLove.Game.PuzzleSelection
{
    // The map's selection cube is the puzzle's own zone cube: a pinch inside scales the selected
    // diorama, a frame grab slides it over the map. This wraps the zone with the map's rules.
    internal class MapCube
    {
        private const float EnvelopeHeadroom = 1.05f;
        private const float RefuseScale = 1.01f;

        private readonly CubeView zone;
        private readonly Vector3 size;
        private readonly float enterAt;
        private readonly float lockedCeiling;

        public MapCube(CubeView zone, float size, float enterAt, float lockedLimit)
        {
            this.zone = zone;
            this.size = Vector3.one * size;
            this.enterAt = Mathf.Max(enterAt, 1.01f);
            lockedCeiling = Mathf.Lerp(1f, this.enterAt, Mathf.Clamp01(lockedLimit));
        }

        public event Action Moved
        {
            add => zone.Moved += value;
            remove => zone.Moved -= value;
        }

        public Vector3 Position => zone.transform.position;

        public bool IsScaling => zone.IsScaling;

        public bool IsHeld => zone.isActiveAndEnabled && zone.IsHeld;

        public float Scale => zone.Scale;

        public bool ReachedEnter => zone.Scale >= enterAt;

        public bool TriedToScale => zone.Scale > RefuseScale;

        public void Show(Vector3 centre, bool canEnter)
        {
            zone.SetConditions(ZoneConditions.SlideOnPlane());
            zone.Place(centre, size, 0f);
            zone.SetInputLocked(false);
            SetEnterable(canEnter);
        }

        // Shrinking does nothing on the map; a locked node stops early and reads dark.
        public void SetEnterable(bool canEnter)
        {
            zone.SetScaleEnvelope(1f, canEnter ? enterAt * EnvelopeHeadroom : lockedCeiling);
            zone.SetDormant(!canEnter);
        }

        public void SnapTo(Vector3 centre) => zone.SetFrame(centre, size);

        public void Release() => zone.ResetScale();

        public void Lock()
        {
            zone.SetDormant(false);
            zone.SetInputLocked(true);
        }

        public void Refuse() => zone.Refuse(ZoneRefusal.Locked);

        public void SetInteractive(bool value, bool canEnter)
        {
            zone.SetInputLocked(!value);
            if (value)
                SetEnterable(canEnter);
            else
                zone.SetDormant(false);
        }

        public void Hide()
        {
            zone.SetInputLocked(false);
            zone.ResetScale();
            zone.SetDormant(false);
            zone.gameObject.SetActive(false);
        }
    }
}

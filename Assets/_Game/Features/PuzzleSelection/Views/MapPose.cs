using System;
using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    // The map holds still while the cube travels over it: the map is placed once per visit with the
    // selected node in front of the player, and the fields fade around wherever the cube is. The menu
    // sits a little below the field and rises back to it as a job is entered.
    internal class MapPose
    {
        private readonly Transform menu;
        private readonly float menuDrop;
        private readonly Transform mapRoot;
        private readonly RunSlotView selectedSlot;
        private readonly Transform cube;
        private readonly ViewPool<RunSlotView> fields;
        private readonly float viewRadius;
        private readonly Action<RunSlotView> applyFieldFade;
        private Vector3 fadeFocus;
        private float reach = 1f;

        public bool ShowAll { get; set; }

        public float FadeFar => Mathf.Max(viewRadius * mapRoot.lossyScale.x * reach, 0.0001f);

        public float FadeNear => FadeFar * 0.55f;

        public MapPose(
            Transform menu,
            float menuDrop,
            Transform mapRoot,
            RunSlotView selectedSlot,
            Transform cube,
            ViewPool<RunSlotView> fields,
            float viewRadius)
        {
            this.menu = menu;
            this.menuDrop = menuDrop;
            this.mapRoot = mapRoot;
            this.selectedSlot = selectedSlot;
            this.cube = cube;
            this.fields = fields;
            this.viewRadius = viewRadius;
            applyFieldFade = ApplyFieldFade;
        }

        // Places the selected job on its node, then centres the map on that job's sun.
        public void SetPose(Vector3 node)
        {
            SetRise(0f);
            PlaceSelected(node);
            mapRoot.localPosition = -Vector3.Scale(mapRoot.localScale, selectedSlot.SunMapPosition);
        }

        public void PlaceSelected(Vector3 node)
        {
            selectedSlot.PlaceSunAt(node);
            ApplyFade();
        }

        // 0 holds the menu at its drop below the field, 1 lifts it onto the field. Parent-space, so
        // it follows the seat.
        public void SetRise(float value)
        {
            float t = Mathf.SmoothStep(0f, 1f, value);
            menu.localPosition = Vector3.down * (menuDrop * (1f - t));
        }

        // How much of the map stays lit around the cube: 1 is the full view, 0 hides everything but
        // the selected job. Entering a job closes it, so the map fades out as the diorama grows.
        public void SetReach(float value)
        {
            reach = Mathf.Clamp01(value);
            ApplyFade();
        }

        public void ApplyFade()
        {
            if (ShowAll)
            {
                fields.ForEachFrom(0, field => field.ClearMapFade());
                return;
            }

            fadeFocus = cube.position;
            fields.ForEachFrom(0, applyFieldFade);
        }

        private void ApplyFieldFade(RunSlotView field) =>
            field.SetMapFade(fadeFocus, FadeNear, FadeFar);
    }
}

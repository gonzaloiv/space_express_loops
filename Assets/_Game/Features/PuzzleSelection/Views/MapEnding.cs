using System;
using System.Collections.Generic;
using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    internal class MapEnding
    {
        private readonly RunSlotView selectedSlot;
        private readonly ViewPool<RunSlotView> fields;
        private readonly MapNodeTint tint;
        private readonly float nodeSecs;
        private readonly float finishSecs;
        private readonly List<RunSlotView> slots = new();
        private Action onComplete;
        private float elapsed;
        private int next;
        private bool playing;

        public MapEnding(
            RunSlotView selectedSlot, ViewPool<RunSlotView> fields,
            MapNodeTint tint, float nodeSecs, float finishSecs)
        {
            this.selectedSlot = selectedSlot;
            this.fields = fields;
            this.tint = tint;
            this.nodeSecs = Mathf.Max(nodeSecs, 0f);
            this.finishSecs = Mathf.Max(finishSecs, 0f);
        }

        public void Play(Action onComplete)
        {
            this.onComplete = onComplete;
            slots.Clear();
            slots.Add(selectedSlot);
            for (int i = 0; i < fields.Views.Count; i++)
            {
                if (fields.Views[i].MapIndex >= 0)
                    slots.Add(fields.Views[i]);
            }
            slots.Sort(CompareByIndex);

            Color unlit = tint.For(JobState.Locked);
            for (int i = 0; i < slots.Count; i++)
                slots[i].SetEndingLit(false, unlit);

            elapsed = 0f;
            next = 0;
            playing = true;
        }

        public void Tick(float deltaSecs)
        {
            if (!playing)
                return;

            elapsed += deltaSecs;
            if (next < slots.Count)
            {
                if (elapsed < nodeSecs)
                    return;

                elapsed = 0f;
                slots[next].SetEndingLit(true, tint.For(JobState.Solved));
                next++;
                return;
            }

            if (elapsed < finishSecs)
                return;

            playing = false;
            Action callback = onComplete;
            onComplete = null;
            callback?.Invoke();
        }

        public void Stop()
        {
            playing = false;
            onComplete = null;
        }

        private static int CompareByIndex(RunSlotView first, RunSlotView second) =>
            first.MapIndex.CompareTo(second.MapIndex);
    }
}

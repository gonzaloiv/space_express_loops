using System;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    [Serializable]
    public struct SelectionCubePalette
    {
        public ColorValue Current;
        public ColorValue Solved;
        public ColorValue Locked;
    }

    internal class SelectionCubeLens
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private readonly Renderer renderer;
        private readonly Transform cube;
        private readonly SelectionCubePalette palette;
        private readonly MaterialPropertyBlock block = new();
        private readonly Vector3 homeScale;
        private float currentScale = 1f;

        public SelectionCubeLens(
            Renderer renderer, Transform cube, SelectionCubePalette palette)
        {
            this.renderer = renderer;
            this.cube = cube;
            this.palette = palette;
            homeScale = cube.localScale;
        }

        public void Show(JobState state, float scale)
        {
            currentScale = scale;
            Color color = ColorFor(state).value;
            block.SetColor(BaseColorId, color);
            block.SetColor(ColorId, color);
            renderer.SetPropertyBlock(block);
            cube.localScale = homeScale * scale;
        }

        public void ShowState(JobState state) => Show(state, currentScale);

        public void Reset()
        {
            currentScale = 1f;
            renderer.SetPropertyBlock(null);
            cube.localScale = homeScale;
        }

        private ColorValue ColorFor(JobState state)
        {
            switch (state)
            {
                case JobState.Solved:
                    return palette.Solved;
                case JobState.Current:
                    return palette.Current;
                default:
                    return palette.Locked;
            }
        }
    }
}

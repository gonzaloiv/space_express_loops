using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public class PuzzleDioramaView : FieldContentView
    {
        [SerializeField] private Transform cubePrototype;
        [SerializeField] private Transform cubeParent;
        private bool cubesVisible;

        private ViewPool<Transform> cubes;
        private PuzzleFrame[] frames;

        protected override void Awake()
        {
            base.Awake();
            cubes = new ViewPool<Transform>(cubePrototype, cubeParent, "Cube", null);
        }

        public override void Render(PuzzleData puzzle)
        {
            frames = puzzle != null ? puzzle.frames : null;
            base.Render(puzzle);
            if (puzzle == null)
            {
                HideUnusedCubes(0);
                return;
            }

            int count = cubesVisible ? RenderCubes(frames) : 0;
            HideUnusedCubes(count);
        }

        public void SetEndpointsMarked(bool marked) => SetEndpointMarksShown(marked);

        public void SetStateDim(float amount) => SetPlanetStateDim(amount, amount);

        public void SetStateDim(float planets, float sun) => SetPlanetStateDim(planets, sun);

        public override void Hide()
        {
            base.Hide();
            HideUnusedCubes(0);
        }

        public void SetCubesVisible(bool visible)
        {
            if (cubesVisible == visible)
                return;

            cubesVisible = visible;
            if (cubes == null)
                return;

            int count = visible ? RenderCubes(frames) : 0;
            HideUnusedCubes(count);
        }

        protected override void AddContentBounds(BoundsBuilder builder)
        {
            int frameCount = cubesVisible && frames != null ? frames.Length : 0;
            for (int i = 0; i < frameCount; i++)
                builder.AddSphere(frames[i].centre, frames[i].size * 0.5f);
        }

        private int RenderCubes(PuzzleFrame[] source)
        {
            int count = source != null ? source.Length : 0;
            for (int i = 0; i < count; i++)
            {
                Transform cube = cubes.EnsureSlot(i);
                cube.gameObject.SetActive(true);
                cube.localPosition = source[i].centre;
                cube.localScale = source[i].BoxSize;
            }

            return count;
        }

        private void HideUnusedCubes(int count)
        {
            if (cubes == null)
                return;

            cubes.ForEachFrom(count, view => view.gameObject.SetActive(false));
        }
    }
}

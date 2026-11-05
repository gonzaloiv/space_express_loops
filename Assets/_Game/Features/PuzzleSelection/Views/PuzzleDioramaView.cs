using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public class PuzzleDioramaView : FieldContentView
    {
        [SerializeField] private Transform cubePrototype;
        [SerializeField] private Transform cubeParent;
        [SerializeField] private bool cubesVisible = true;

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

        public void Render(int index, PuzzleData puzzle)
        {
            Render(puzzle);
            if (puzzle != null)
                SetSunLabel((index + 1) + " · " + SunName);
        }

        public override void Hide()
        {
            base.Hide();
            HideUnusedCubes(0);
        }

        public void SetCubesVisible(bool visible)
        {
            cubesVisible = visible;
            if (!visible && cubes != null)
                HideUnusedCubes(0);
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

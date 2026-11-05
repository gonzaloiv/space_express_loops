using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    internal class MapOverview
    {
        private readonly Transform mapRoot;
        private readonly MapPose pose;
        private readonly Vector3 baseScale;
        private readonly float targetScale;
        private bool wasOverviewing;

        public MapOverview(Transform mapRoot, MapPose pose, float targetScale)
        {
            this.mapRoot = mapRoot;
            this.pose = pose;
            baseScale = mapRoot.localScale;
            this.targetScale = targetScale;
        }

        public void Apply(float progress, bool showAll, Vector3 mapFocus, Vector3 slotFocus)
        {
            bool overviewing = progress > 0.001f;
            if (!overviewing && !wasOverviewing)
                return;

            wasOverviewing = overviewing;
            mapRoot.localScale = baseScale * Mathf.Lerp(1f, targetScale, progress);
            pose.ShowAll = showAll || overviewing;
            pose.SetPose(mapFocus, slotFocus);
        }

        public void Reset(bool showAll)
        {
            wasOverviewing = false;
            mapRoot.localScale = baseScale;
            pose.ShowAll = showAll;
        }
    }
}

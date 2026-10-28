using System.Collections.Generic;

namespace DigitalLove.Game.SpaceShaping
{
    internal sealed class FixedZoneGesture
    {
        private readonly List<bool> scaling = new();

        public void Reset() => scaling.Clear();

        public bool Track(
            SpaceShapingSession session, IReadOnlyList<SpaceZoneView> cubes, out bool commit)
        {
            commit = false;
            bool changed = false;
            while (scaling.Count < cubes.Count)
                scaling.Add(false);

            for (int i = 0; i < cubes.Count; i++)
            {
                if (!cubes[i].gameObject.activeSelf)
                {
                    scaling[i] = false;
                    continue;
                }

                if (cubes[i].IsScaling)
                {
                    if (!scaling[i] && session.IsOverBudget)
                    {
                        cubes[i].Refuse();
                        continue;
                    }

                    session.PreviewZone(cubes[i].Zone);
                    scaling[i] = true;
                    changed = true;
                }
                else if (scaling[i])
                {
                    scaling[i] = false;
                    session.CommitPreview();
                    commit = true;
                }
            }

            return changed;
        }
    }
}

using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    // The selection cube's snap-and-select pass: a drag previews the nearest node, a new node is
    // selected, and the cube snaps back to the selected sun.
    internal sealed class MapCubeSelection
    {
        private readonly MapCube cube;
        private readonly MapPose pose;
        private readonly MapFieldRenderer fieldView;
        private readonly MapSelectionReadout readout;

        public MapCubeSelection(
            MapCube cube, MapPose pose, MapFieldRenderer fieldView, MapSelectionReadout readout)
        {
            this.cube = cube;
            this.pose = pose;
            this.fieldView = fieldView;
            this.readout = readout;
        }

        public bool CubeMoved(PuzzleMap map, Transform mapRoot)
        {
            int nearest = MapSnap.Nearest(map, mapRoot, cube.Position);
            if (nearest >= 0 && nearest != map.SelectedIndex && map.Select(nearest))
            {
                RenderFields(map);
                return true;
            }

            readout.Show(map.Selected);
            return false;
        }

        public void Snap(Vector3 sunFocus)
        {
            cube.SnapTo(sunFocus);
            pose.ApplyFade();
        }

        private void RenderFields(PuzzleMap map)
        {
            fieldView.Render(map);
            pose.PlaceSelected(map.Position(map.SelectedIndex));
            cube.SetEnterable(map.CanEnter);
        }
    }
}

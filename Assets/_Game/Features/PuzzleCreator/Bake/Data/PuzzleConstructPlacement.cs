using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // A fan-out's picked layout before the lesson: every ship shares one origin and carries its own
    // destination, and the hub is the bend near that origin. Plain data only.
    public sealed class PuzzleConstructPlacement
    {
        public PuzzleFieldPlanet[] Planets;
        public PuzzleConstructedShip[] Ships;
        public int HubIndex;
        public Vector3 HubHome;
    }
}

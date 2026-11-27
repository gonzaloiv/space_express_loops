using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    // The fan-out build: pick one origin, its destinations and the bend, then aim the destinations
    // onto the bend's answered paths. The lesson re-checks and names the answer afterwards.
    public static class PuzzleConstructFanOut
    {
        public static PuzzleConstructPlacement Build(
            PuzzleFieldPlanet[] planets, PuzzleConstructRequest request, int seedValue)
        {
            if (request.Brief.Ships < 2 || request.Brief.Relation != PuzzleShipRelation.FanOut)
                return null;
            if (!PuzzleConstructFanOutChord.TryPick(
                    planets, request, out PuzzleConstructedShip[] ships, out int bend))
                return null;
            if (!PuzzleConstructFanOutAim.TryAim(
                    planets, request, seedValue, ships[0].OriginIndex, bend, ships))
                return null;

            return new PuzzleConstructPlacement
            {
                Planets = planets,
                Ships = ships,
                HubIndex = bend,
                HubHome = planets[bend].position
            };
        }
    }
}

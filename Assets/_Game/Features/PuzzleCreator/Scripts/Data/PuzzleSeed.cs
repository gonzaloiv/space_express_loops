using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    [CreateAssetMenu(fileName = "PuzzleSeed", menuName = "DigitalLove/Game/Puzzle Seed")]
    public class PuzzleSeed : ScriptableObject
    {
        public int seed = 1;
        [HideInInspector] public float distance = 1f;
        public int planetCount = 9;

        [Tooltip("Minimum clearance between the run's endpoint planets and every other planet, in metres. Wider = a clearer corridor and a denser field is allowed.")]
        public float corridorClearance = 0.05f;
    }
}

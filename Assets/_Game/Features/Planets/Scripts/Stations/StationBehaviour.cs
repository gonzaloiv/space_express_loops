using UnityEngine;
using DigitalLove.Global;

namespace DigitalLove.Game.Planets
{
    public class StationBehaviour : MonoBehaviour
    {
        [SerializeField] private Transform anchor;

        private bool isTaken;

        public bool IsTaken => isTaken;
        public Pose WorldPose => anchor.ToWorldPose();

        public void SetIsTaken(bool isTaken) => this.isTaken = isTaken;
    }
}
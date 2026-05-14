using System;

namespace DigitalLove.Game.Spaceships
{
    [Serializable]
    public class LoopData
    {
        public string spaceshipId;
        public string originId;
        public string destinationId;
        public string colorCode;

        public bool HasDestination => !string.IsNullOrEmpty(destinationId);
    }
}
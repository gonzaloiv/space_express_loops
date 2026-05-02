using System;

namespace DigitalLove.Game.Spaceships
{
    [Serializable]
    public class LoopEventArgs
    {
        public string originId;
        public string spaceshipId;
        public string destinationId;

        public bool IsBaseLoop => string.IsNullOrEmpty(originId);
    }

    [Serializable]
    public class LoopCompleteEventArgs : LoopEventArgs
    {
        public int value;
    }
}
using System;

namespace DigitalLove.Game.Spaceships
{
    [Serializable]
    public class LoopEventArgs
    {
        public string spaceshipId;
        public string originId;
        public string destinationId;

        public bool IsBaseLoop => string.IsNullOrEmpty(originId);
    }

    [Serializable]
    public class LoopCompleteEventArgs : LoopEventArgs
    {
        public int value;

        public LoopCompleteEventArgs(string spaceshipId, int value) : base()
        {
            this.spaceshipId = spaceshipId;
            this.value = value;
        }

        public LoopCompleteEventArgs(LoopEventArgs args, int value) : base()
        {
            originId = args.originId;
            spaceshipId = args.spaceshipId;
            destinationId = args.destinationId;
            this.value = value;
        }
    }
}
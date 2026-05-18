using System;

namespace DigitalLove.Game.Spaceships
{
    [Serializable]
    public class LoopEventArgs
    {
        public string spaceshipId;
        public string destinationId;
        public string colorCode;

        public bool HasFailed => string.IsNullOrEmpty(destinationId);
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
            spaceshipId = args.spaceshipId;
            destinationId = args.destinationId;
            colorCode = args.colorCode;
            this.value = value;
        }
    }
}
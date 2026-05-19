using System;
using System.Collections.Generic;

namespace DigitalLove.Game.Spaceships
{
    [Serializable]
    public class LoopEventArgs
    {
        public string spaceshipId;
        public List<string> destinationIds = new();
        public string colorCode;
        public string hubId;

        public bool HasRoute => destinationIds != null && destinationIds.Count > 0;
    }

    [Serializable]
    public class LoopCompleteEventArgs : LoopEventArgs
    {
        public int value;
        public bool IterationFailed { get; }

        public LoopCompleteEventArgs(string spaceshipId, int value, bool iterationFailed = true) : base()
        {
            this.spaceshipId = spaceshipId;
            this.value = value;
            IterationFailed = iterationFailed;
        }

        public LoopCompleteEventArgs(LoopEventArgs args, int value) : base()
        {
            spaceshipId = args.spaceshipId;
            destinationIds = args.destinationIds != null ? new List<string>(args.destinationIds) : new();
            colorCode = args.colorCode;
            hubId = args.hubId;
            this.value = value;
            IterationFailed = false;
        }
    }
}
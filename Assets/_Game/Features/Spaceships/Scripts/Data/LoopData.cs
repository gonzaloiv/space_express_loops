using System;
using Newtonsoft.Json;

namespace DigitalLove.Game.Spaceships
{
    [Serializable]
    public class LoopData
    {
        public string spaceshipId;
        public string destinationId;
        public string colorCode;
        public string hubId;

        [JsonIgnore] public bool HasDestination => !string.IsNullOrEmpty(destinationId);
    }
}

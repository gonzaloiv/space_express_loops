using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DigitalLove.Game.Spaceships
{
    [Serializable]
    public class LoopData
    {
        public string spaceshipId;
        public List<string> destinationIds = new();
        public string colorCode;
        public string hubId;

        [JsonIgnore] public bool HasDestinations => destinationIds != null && destinationIds.Count > 0;
    }
}

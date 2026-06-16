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

        public static LoopData From(LoopData source) => new()
        {
            spaceshipId = source.spaceshipId,
            destinationIds = CopyDestinationIds(source.destinationIds),
            colorCode = source.colorCode,
            hubId = source.hubId
        };

        public void CopyFrom(LoopData source)
        {
            destinationIds = CopyDestinationIds(source.destinationIds);
            colorCode = source.colorCode;
            hubId = source.hubId;
        }

        public List<T> ResolveDestinations<T>(Func<string, T> resolve) where T : class
        {
            destinationIds ??= new();
            List<T> resolved = new();
            List<string> validIds = new();

            foreach (string id in destinationIds)
            {
                if (string.IsNullOrEmpty(id))
                    continue;

                T item = resolve(id);
                if (item == null)
                    continue;

                validIds.Add(id);
                resolved.Add(item);
            }

            destinationIds = validIds;
            return resolved;
        }

        private static List<string> CopyDestinationIds(List<string> ids) =>
            ids != null ? new List<string>(ids) : new();
    }
}

using System.Collections.Generic;
using DigitalLove.DataAccess;

namespace DigitalLove.Game.Flow
{
    // Jim's job lines already spoken this session. A job's intro and its fail line play once per
    // session, so a retry, from the fail or from the map, does not repeat them.
    internal sealed class HeardJobLines
    {
        private readonly HashSet<string> heard = new();

        public static HeardJobLines In(MemoryDataClient memory)
        {
            HeardJobLines lines = memory.Get<HeardJobLines>();
            if (lines != null)
                return lines;

            lines = new HeardJobLines();
            memory.Put(lines);
            return lines;
        }

        // True the first time a key is asked for this session; records it as heard.
        public bool FirstTime(string key) => heard.Add(key);
    }
}

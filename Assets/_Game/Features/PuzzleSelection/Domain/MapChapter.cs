using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    // One chapter's group on the map: its jobs (a run of map indices) and the system they sit in.
    public readonly struct MapChapter
    {
        public readonly string Id;
        public readonly int Start;
        public readonly int Count;
        public readonly Vector3 Centre;
        public readonly float Radius;

        public MapChapter(string id, int start, int count, Vector3 centre, float radius)
        {
            Id = id;
            Start = start;
            Count = count;
            Centre = centre;
            Radius = radius;
        }

        public bool StartsAt(int index) => Start > 0 && index == Start;
    }
}

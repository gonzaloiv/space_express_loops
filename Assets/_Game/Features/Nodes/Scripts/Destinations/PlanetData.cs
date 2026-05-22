using System;
using DigitalLove.Global;

namespace DigitalLove.Game.Nodes
{
    [Serializable]
    public class PlanetData
    {
        public string id;
        public float radius;
        public SerializableVector3 localPosition;
        public int lettersPerMinute;
        public int maxLetters;
    }
}

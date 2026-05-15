using System;
using DigitalLove.Global;
using Newtonsoft.Json;

namespace DigitalLove.Game.Planets
{
    [Serializable]
    public class PlanetData
    {
        public string id;
        public float radius;
        public SerializableVector3 localPosition;
        public int lettersPerMinute;
        public int maxLetters;
        public SerializableVector2 color;

        [JsonIgnore] public bool HasColor => color != null && color.IsNotZero();
    }
}
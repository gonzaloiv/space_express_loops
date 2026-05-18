using System;
using DigitalLove.Global;

namespace DigitalLove.Game.Planets
{
    [Serializable]
    public class HubData
    {
        public string id;
        public SerializableVector3 localPosition;
        public float placementRadius;
    }
}

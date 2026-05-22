using DigitalLove.Game.UI;
using DigitalLove.VFX;
using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public class PlanetRefs : MonoBehaviour
    {
        [Header("Store")]
        public PlanetStore planetStore;

        [Header("UI")]
        public ResourcePanel lettersPanel;

        [Header("Body")]
        public NodeBody nodeBody;
        public Outline outline;
        public ScalePunch scalePunch;
    }
}

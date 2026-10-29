using DigitalLove.Game.UI;
using DigitalLove.VFX;
using UnityEngine;

namespace DigitalLove.Game.Nodes
{
    public class PlanetRefs : MonoBehaviour
    {
        [SerializeField] private PlanetStore planetStore;
        [SerializeField] private ResourcePanel lettersPanel;
        [SerializeField] private NodeBody nodeBody;
        [SerializeField] private Outline outline;
        [SerializeField] private ScalePunch scalePunch;
        [SerializeField] private Color destinationColor = new(1f, 0.75f, 0.15f, 1f);
        [SerializeField] private float destinationOutlineWidth = 0.12f;

        public PlanetStore PlanetStore => planetStore;
        public ResourcePanel LettersPanel => lettersPanel;
        public NodeBody NodeBody => nodeBody;
        public Outline Outline => outline;
        public ScalePunch ScalePunch => scalePunch;
        public Color DestinationColor => destinationColor;
        public float DestinationOutlineWidth => destinationOutlineWidth;
    }
}

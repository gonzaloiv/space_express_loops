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

        public PlanetStore PlanetStore => planetStore;
        public ResourcePanel LettersPanel => lettersPanel;
        public NodeBody NodeBody => nodeBody;
        public Outline Outline => outline;
        public ScalePunch ScalePunch => scalePunch;
    }
}

using DigitalLove.VFX;
using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class PlanetBehaviour : MonoBehaviour
    {
        [SerializeField] private PlanetStore planetStore;
        [SerializeField] private PlanetBaseBehaviour planetBase;
        [SerializeField] private PlanetBody planetBody;

        [Header("Body")]
        [SerializeField] private Outline outline;
        [SerializeField] private ScalePunch scalePunch;

        private string id;
        private bool isDestination;

        public string Id => id;
        public bool IsDestination => isDestination;
        public bool IsActive => gameObject.activeInHierarchy;

        public PlanetStore PlanetStore => planetStore;
        public PlanetBody PlanetBody => planetBody;
        public Vector3 Position => planetBody.Position;

        public PlanetBaseBehaviour PlanetBase => planetBase;
        public bool CanBeDestination => PlanetBase != null && planetBase.HasAvailableStations;

        public void SetIsDestination(bool isDestination)
        {
            this.isDestination = isDestination;
            if (isDestination && !IsDestination)
                scalePunch.Animate();
            SetOutlineActive(false);
        }

        public void SetOutlineActive(bool isActive)
        {
            outline.enabled = isActive;
        }

        public void Spawn(PlanetData planetData)
        {
            id = planetData.id;
            planetBody.SetRadius(planetData.radius);
            transform.localPosition = planetData.localPosition.ToVector3();
            gameObject.SetActive(true);
            SetOutlineActive(false);

            planetStore.StartStoring(planetData.lettersPerMinute, planetData.maxLetters);
        }
    }
}
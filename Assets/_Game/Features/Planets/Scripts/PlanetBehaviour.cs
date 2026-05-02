using DigitalLove.Global;
using DigitalLove.VFX;
using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class PlanetBehaviour : MonoBehaviour
    {
        [SerializeField] private Outline outline;
        [SerializeField] private ScalePunch scalePunch;
        [SerializeField] private float maxOutlineWidth = 5f;
        [SerializeField] private ColorValue routeColor;
        [SerializeField] private ColorValue defaultColor;
        [SerializeField] private Renderer rend;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private PlanetStore planetStore;
        [SerializeField] private PlanetBaseBehaviour planetBase;
        [SerializeField] private PlanetBody planetBody;

        private PlanetData planetData;
        private float percentage;

        public bool IsDestination => percentage >= 1;
        public string Id => planetData != null ? planetData.id : string.Empty;
        public bool IsActive => gameObject.activeInHierarchy;
        public PlanetStore PlanetStore => planetStore;
        public PlanetBaseBehaviour PlanetBase => planetBase;
        public PlanetBody PlanetBody => planetBody;
        public bool CanBeDestination => planetBase.HasAvailableStations;

        public void SetIsDestination(float percentage)
        {
            if (percentage >= 1 && !IsDestination)
                scalePunch.Animate();
            this.percentage = percentage;
        }

        private void Update()
        {
            outline.enabled = percentage > 0 && !IsDestination;
            outline.OutlineWidth = (1 - percentage) * maxOutlineWidth;
        }

        public void Spawn(PlanetData planetData)
        {
            this.planetData = planetData;
            planetBody.SetRadius(planetData.radius);
            transform.localPosition = planetData.localPosition.ToVector3();
            rend.material.color = defaultColor.value;
            gameObject.SetActive(true);
            planetStore.StartStoring(planetData.lettersPerMinute, planetData.maxLetters);
        }

        public void SetIsInRoute(bool isInRoute)
        {
            rend.material.color = isInRoute ? routeColor.value : defaultColor.value;
        }
    }
}
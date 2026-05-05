using DigitalLove.VFX;
using UnityEngine;
using DigitalLove.Global;

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
            planetBody.Init(planetData.radius);
            transform.localPosition = planetData.localPosition.ToVector3();
            gameObject.SetActive(true);
            SetOutlineActive(false);

            planetStore.StartStoring(planetData.lettersPerMinute, planetData.maxLetters);
        }

        public void SetColor(Vector2 offset)
        {
            planetBody.SetColor(offset);
        }

        [Button]
        public void Debug_SetRandomColor()
        {
            SetColor(new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f)));
        }
    }
}
using DigitalLove.VFX;
using UnityEngine;
using DigitalLove.Game.UI;
using System;

namespace DigitalLove.Game.Planets
{
    public class PlanetBehaviour : MonoBehaviour
    {
        [SerializeField] private PlanetStore planetStore;
        [SerializeField] private PlanetBaseBehaviour planetBase;

        [Header("UI")]
        [SerializeField] private ResourcePanel lettersPanel;

        [Header("Body")]
        [SerializeField] private PlanetBody planetBody;
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

        public void Spawn(PlanetData planetData, Action planetFull)
        {
            gameObject.SetActive(true);

            SetupFromData(planetData);
            SetupUI();

            planetStore.StartStoring(planetData.lettersPerMinute, planetData.maxLetters, planetFull);
        }

        private void SetupFromData(PlanetData planetData)
        {
            id = planetData.id;
            transform.localPosition = planetData.localPosition.ToVector3();
            planetBody.Init(planetData.radius);
            SetOutlineActive(false);
        }

        private void SetupUI()
        {
            lettersPanel.Init(transform.position + transform.up * planetBody.RadiusOffset);
        }
    }
}

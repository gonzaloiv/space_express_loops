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
        [SerializeField] private ButtonPanel setColorButtonPanel;

        [Header("Body")]
        [SerializeField] private PlanetBody planetBody;
        [SerializeField] private Outline outline;
        [SerializeField] private ScalePunch scalePunch;

        private string id;
        private bool isDestination;
        private Action<string> setColorButtonClicked;

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

        public void Spawn(PlanetData planetData, Action<string> setColorButtonClicked)
        {
            gameObject.SetActive(true);

            id = planetData.id;
            this.setColorButtonClicked = setColorButtonClicked;
            transform.localPosition = planetData.localPosition.ToVector3();
            planetBody.Init(planetData.radius);
            SetOutlineActive(false);

            lettersPanel.Init(transform.position + transform.up * planetBody.RadiusOffset);
            setColorButtonPanel.SetPosition(transform.position - transform.up * planetBody.RadiusOffset);

            planetStore.StartStoring(planetData.lettersPerMinute, planetData.maxLetters);
        }

        private void OnEnable()
        {
            setColorButtonPanel.buttonClicked += OnSetColorButtonClicked;
        }

        private void OnSetColorButtonClicked()
        {
            setColorButtonClicked(Id);
        }

        private void OnDisable()
        {
            setColorButtonPanel.buttonClicked -= OnSetColorButtonClicked;
        }
    }
}
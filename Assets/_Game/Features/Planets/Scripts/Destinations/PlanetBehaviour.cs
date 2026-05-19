using DigitalLove.VFX;
using UnityEngine;
using DigitalLove.Game.UI;
using System;

namespace DigitalLove.Game.Planets
{
    public class PlanetBehaviour : MonoBehaviour
    {
        [SerializeField] private PlanetStore planetStore;

        [Header("UI")]
        [SerializeField] private ResourcePanel lettersPanel;

        [Header("Body")]
        [SerializeField] private PlanetBody planetBody;
        [SerializeField] private Outline outline;
        [SerializeField] private ScalePunch scalePunch;

        private string id;
        private bool isDestination;
        private bool isOnRoute;

        public string Id => id;
        public bool IsDestination => isDestination;
        public bool IsOnRoute => isOnRoute;
        public bool IsActive => gameObject.activeInHierarchy;

        public PlanetStore PlanetStore => planetStore;
        public PlanetBody PlanetBody => planetBody;
        public Vector3 Position => planetBody.Position;

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

        public void SetOnRoute(bool onRoute)
        {
            isOnRoute = onRoute;
        }

        public void Spawn(PlanetData planetData, Action planetFull)
        {
            gameObject.SetActive(true);

            SetupFromData(planetData);
            SetupUI();

            planetStore.PrepareStoring(planetData.lettersPerMinute, planetData.maxLetters, planetFull);
        }

        public void BeginStoring() => planetStore.BeginStoring();

        private void SetupFromData(PlanetData planetData)
        {
            id = planetData.id;
            transform.localPosition = planetData.localPosition.ToVector3();
            planetBody.Init(planetData.radius);
            planetBody.ResetRouteColor();
            SetOnRoute(false);
            SetOutlineActive(false);
        }

        private void SetupUI()
        {
            lettersPanel.Init(transform.position + transform.up * planetBody.Radius);
        }
    }
}

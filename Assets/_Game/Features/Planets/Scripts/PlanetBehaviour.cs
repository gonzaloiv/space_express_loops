using DigitalLove.Global;
using DigitalLove.VFX;
using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class PlanetBehaviour : MonoBehaviour
    {
        [SerializeField] private Outline outline;
        [SerializeField] private Transform body;
        [SerializeField] private ScalePunch scalePunch;
        [SerializeField] private float maxOutlineWidth = 5f;
        [SerializeField] private ColorValue routeColor;
        [SerializeField] private ColorValue defaultColor;
        [SerializeField] private Renderer rend;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private LettersPanel lettersPanel;

        private PlanetData planetData;

        private float percentage;
        private int letters;
        private float countdown;

        public bool IsDestination => percentage >= 1;
        public float RadiusOffset => body.lossyScale.x;
        public int Letters => letters;
        public string Id => planetData != null ? planetData.id : string.Empty;
        public bool IsActive => gameObject.activeInHierarchy;

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
            countdown -= Time.deltaTime;
            if (countdown <= 0)
            {
                letters++;
                if (letters > planetData.maxLetters)
                    letters = planetData.maxLetters;
                lettersPanel.Show(letters);
                ResetCoundown();
            }
        }

        public void Spawn(PlanetData planetData)
        {
            this.planetData = planetData;
            body.localScale = Vector3.one * planetData.radius;
            transform.localPosition = planetData.localPosition.ToVector3();

            ResetCoundown();
            rend.material.color = defaultColor.value;

            gameObject.SetActive(true);
            lettersPanel.Init(transform.position + transform.up * planetData.radius, planetData.maxLetters);
        }

        private void ResetCoundown()
        {
            countdown = 60 / (float)planetData.lettersPerMinute;
        }

        public void SetIsInRoute(bool isInRoute)
        {
            rend.material.color = isInRoute ? routeColor.value : defaultColor.value;
        }

        public int PickLetters(int value)
        {
            int picked = 0;
            picked = value > letters ? letters : value;
            letters -= picked;
            lettersPanel.Show(letters);
            return picked;
        }
    }
}
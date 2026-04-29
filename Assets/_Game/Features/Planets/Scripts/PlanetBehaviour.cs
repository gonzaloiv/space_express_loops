using DigitalLove.Global;
using DigitalLove.VFX;
using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class PlanetBehaviour : MonoBehaviour
    {
        private const int MaxLetters = 99;

        [SerializeField] private Outline outline;
        [SerializeField] private Transform body;
        [SerializeField] private ScalePunch scalePunch;
        [SerializeField] private float maxOutlineWidth = 5f;
        [SerializeField] private ColorValue routeColor;
        [SerializeField] private ColorValue defaultColor;
        [SerializeField] private Renderer rend;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private LettersPanel lettersPanel;

        private float percentage;
        private int letters;
        private int lettersPerMinute;
        private float countdown;
        private string id;

        public bool IsDestination => percentage >= 1;
        public float RadiusOffset => body.lossyScale.x;
        public int Letters => letters;
        public string Id => id;
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
                if (letters > MaxLetters)
                    letters = MaxLetters;
                lettersPanel.Show(letters);
                ResetCoundown();
            }
        }

        public void Spawn(PlanetData planetData)
        {
            this.id = planetData.id;
            this.lettersPerMinute = planetData.lettersPerMinute;
            body.localScale = Vector3.one * planetData.radius;
            lettersPanel.Init(transform.position + transform.up * planetData.radius);
            transform.localPosition = planetData.localPosition.ToVector3();

            ResetCoundown();
            rend.material.color = defaultColor.value;
            gameObject.SetActive(true);
        }

        private void ResetCoundown()
        {
            countdown = 60 / (float)lettersPerMinute;
        }

        public void SetIsInRoute(bool isInRoute)
        {
            rend.material.color = isInRoute ? routeColor.value : defaultColor.value;
        }

        public void EmptyLetters()
        {
            letters = 0;
            lettersPanel.Show(letters);
        }

    }
}
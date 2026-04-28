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

        public bool IsDestination => percentage >= 1;
        public float RadiusOffset => body.lossyScale.x;
        public int Letters => letters;

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

        public void Setup(float radius, Vector3 position, int lettersPerMinute)
        {
            body.localScale = Vector3.one * radius;
            transform.position = position;
            this.lettersPerMinute = lettersPerMinute;
            ResetCoundown();
            rend.material.color = defaultColor.value;
            gameObject.SetActive(true);
            lettersPanel.Init(transform.position + transform.up * radius);
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
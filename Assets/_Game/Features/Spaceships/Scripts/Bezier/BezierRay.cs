using System;
using System.Linq;
using DigitalLove.Game.Planets;
using DigitalLove.Global;
using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class BezierRay : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float secsToSelect;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private SplineContainerWrapper splineContainerWrapper;
        [SerializeField] private GameObject arrow;

        private Transform body;
        private float countdown;
        private BasePlanetBehaviour basePlanet;
        private PlanetBehaviour destinationPlanet;
        private bool isLookingForDestination;

        public SplineContainerWrapper Spline => splineContainerWrapper;
        public BasePlanetBehaviour Origin => basePlanet;
        public PlanetBehaviour Destination => destinationPlanet;

        public Action<PlanetBehaviour> planetSelected = (planet) => { };

        public void SetIsLookingForDestination(bool isLookingForDestination) => this.isLookingForDestination = isLookingForDestination;

        public void Init(BasePlanetBehaviour basePlanet, Transform body)
        {
            this.basePlanet = basePlanet;
            this.body = body;
        }

        private void OnEnable()
        {
            isLookingForDestination = true;
        }

        private void OnDisable()
        {
            if (destinationPlanet != null)
                destinationPlanet.SetIsDestination(0);
        }

        private void Update()
        {
            arrow.SetActive(isLookingForDestination);
            if (!isLookingForDestination)
                return;
            CheckHits();
            ShowLineRenderer();
        }

        private void CheckHits()
        {
            transform.SetPose(body.ToWorldPose());
            RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, 100, layerMask).Where(h =>
            {
                PlanetBehaviour planet = h.rigidbody.GetComponent<PlanetBehaviour>();
                if (planet != null && planet.gameObject != basePlanet.gameObject)
                    return true;
                return false;
            }).ToArray();
            if (hits.Length <= 0)
            {
                if (destinationPlanet != null)
                    destinationPlanet.SetIsDestination(0);
                destinationPlanet = null;
                planetSelected.Invoke(null);
            }
            else
            {
                PlanetBehaviour hitPlanet = hits[0].rigidbody.GetComponent<PlanetBehaviour>();
                if (hitPlanet != null && hitPlanet != destinationPlanet && hitPlanet.gameObject != basePlanet.gameObject)
                {
                    if (destinationPlanet != null)
                        destinationPlanet.SetIsDestination(0);
                    destinationPlanet = hitPlanet;
                    countdown = secsToSelect;
                }
                else if (destinationPlanet != null && !destinationPlanet.IsDestination)
                {
                    countdown -= Time.deltaTime;
                    destinationPlanet.SetIsDestination(countdown / secsToSelect);
                    if (countdown <= 0)
                    {
                        planetSelected.Invoke(destinationPlanet);
                        destinationPlanet.SetIsDestination(1);
                    }
                }
            }
        }

        private void ShowLineRenderer()
        {
            bool hasDestination = destinationPlanet != null && destinationPlanet.IsDestination;
            if (!hasDestination)
            {
                lineRenderer.enabled = false;
            }
            else
            {
                splineContainerWrapper.CreateLoop(basePlanet, destinationPlanet);
                lineRenderer.SetSplinePositions(splineContainerWrapper.Positions);
                lineRenderer.enabled = true;
            }
        }

        // ! DEBUG

        public void SetDestinationPlanet(PlanetBehaviour toSet)
        {
            gameObject.SetActive(true);
            destinationPlanet = toSet;
            if (destinationPlanet != null)
                destinationPlanet.SetIsDestination(0);
            destinationPlanet.SetIsDestination(1);
            ShowLineRenderer();
        }
    }
}
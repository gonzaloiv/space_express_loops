using System;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class BasePlanetBehaviour : MonoBehaviour
    {
        [SerializeField] private Renderer rend;
        [SerializeField] private ColorValue baseColor;
        [SerializeField] private StationData[] stations;
        [SerializeField] private LettersPanel lettersPanel;

        private string id;

        public string Id => id;
        public float RadiusOffset => rend.transform.lossyScale.x;

        public void Spawn(string id)
        {
            this.id = id;

            transform.localPosition = Vector3.zero;
            rend.material.color = baseColor.value;
            lettersPanel.Init(transform.position + transform.up * RadiusOffset, 0);

            this.SetActive(true);
        }

        public Pose GetValidStationPose()
        {
            StationData pair = stations[UnityEngine.Random.Range(0, stations.Length)];
            pair.isTaken = true;
            return pair.anchor.ToWorldPose();
        }

        public void ShowLetters(int letters, int maxLetters)
        {
            lettersPanel.SetMaxLetters(maxLetters);
            lettersPanel.Show(letters);
        }
    }
}
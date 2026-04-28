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

        public float RadiusOffset => rend.transform.lossyScale.x;

        public void Spawn(string id, int currentLetters = 0)
        {
            transform.localPosition = Vector3.zero;
            rend.material.color = baseColor.value;
            this.SetActive(true);
            lettersPanel.Init(transform.position + transform.up * RadiusOffset);
            ShowLetters(currentLetters);
        }

        public Pose GetValidStationPose()
        {
            StationData pair = stations[UnityEngine.Random.Range(0, stations.Length)];
            pair.isTaken = true;
            return pair.anchor.ToWorldPose();
        }

        public void ShowLetters(int letters)
        {
            lettersPanel.Show(letters);
        }
    }

    [Serializable]
    public class StationData
    {
        public Transform anchor;
        public bool isTaken;
    }
}
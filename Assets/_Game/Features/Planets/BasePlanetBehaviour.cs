using System;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class BasePlanetBehaviour : MonoBehaviour
    {
        [SerializeField] private Renderer rend;
        [SerializeField] private ColorValue baseColor;
        [SerializeField] private AnchorData[] anchors;
        [SerializeField] private LettersPanel lettersPanel;

        public float RadiusOffset => rend.transform.lossyScale.x;

        public void Init()
        {
            transform.localPosition = Vector3.zero;
            rend.material.color = baseColor.value;
            this.SetActive(true);
            lettersPanel.Init(transform.position + transform.up * RadiusOffset);
        }

        public Pose GetValidSpaceshipPose()
        {
            AnchorData pair = anchors[UnityEngine.Random.Range(0, anchors.Length)];
            pair.isTaken = true;
            return pair.anchor.ToWorldPose();
        }

        public void ShowLetters(int letters)
        {
            lettersPanel.Show(letters);
        }
    }

    [Serializable]
    public class AnchorData
    {
        public Transform anchor;
        public bool isTaken;
    }
}
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

        public void Init()
        {
            transform.localPosition = Vector3.zero;
            rend.material.color = baseColor.value;
            this.SetActive(true);
        }

        public Pose GetValidSpaceshipPose()
        {
            AnchorData pair = anchors[UnityEngine.Random.Range(0, anchors.Length)];
            pair.isTaken = true;
            return pair.anchor.ToWorldPose();
        }
    }

    [Serializable]
    public class AnchorData
    {
        public Transform anchor;
        public bool isTaken;
    }
}
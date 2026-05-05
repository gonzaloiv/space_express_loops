using UnityEngine;
using System.Collections.Generic;

namespace DigitalLove.Game.Planets
{
    public class PlanetBodyRenderer : MonoBehaviour
    {
        [SerializeField] private Renderer rend;
        [SerializeField] private Material defaultMaterial;
        [SerializeField] private Material materialWithColor;

        public void SetDefaultMaterial()
        {
            rend.SetMaterials(new List<Material> { defaultMaterial });
        }

        public void SetColorMaterial(Vector2 offset)
        {
            rend.SetMaterials(new List<Material> { materialWithColor });
            rend.material.SetTextureOffset("_DetailAlbedoMap", offset);
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }
}

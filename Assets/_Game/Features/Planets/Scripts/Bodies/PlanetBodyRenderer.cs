using UnityEngine;
using System.Collections.Generic;

namespace DigitalLove.Game.Planets
{
    public class PlanetBodyRenderer : MonoBehaviour
    {
        public const int ColorPaletteSize = 8;

        [SerializeField] private string detailAlbedoMapKey = "_DetailAlbedoMap";
        [SerializeField] private Renderer rend;
        [SerializeField] private Material defaultMaterial;
        [SerializeField] private Material materialWithColor;

        public Vector2 TextureOffset => rend.material.GetTextureOffset(detailAlbedoMapKey);

        public void SetDefaultMaterial()
        {
            rend.SetMaterials(new List<Material> { defaultMaterial });
        }

        public void SetTextureOffsetMaterial(Vector2 offset)
        {
            rend.SetMaterials(new List<Material> { materialWithColor });
            rend.material.SetTextureOffset(detailAlbedoMapKey, offset);
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }

    public static class PlanetBodyRendererExtensions
    {
        public static void SetRandomTextureOffset(this IEnumerable<PlanetBodyRenderer> rends)
        {
            int cellX = Random.Range(0, PlanetBodyRenderer.ColorPaletteSize);
            int cellY = Random.Range(0, PlanetBodyRenderer.ColorPaletteSize);
            float step = 1f / PlanetBodyRenderer.ColorPaletteSize;
            Vector2 offset = new Vector2((cellX + 0.5f) * step, (cellY + 0.5f) * step);
            foreach (PlanetBodyRenderer rend in rends)
            {
                rend.SetTextureOffsetMaterial(offset);
            }
        }
    }
}
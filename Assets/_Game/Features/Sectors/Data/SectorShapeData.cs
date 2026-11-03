using System;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Sectors
{
    [Serializable]
    public class SectorShapeData
    {
        public SectorShapeKind kind = SectorShapeKind.Ring;
        public MinMaxFloat anchorScale = new(1.2f, 2.5f);
        [Tooltip("Explicit anchor clearance radius. 0 derives it from planet radius × anchorScale.")]
        public float anchorRadius = 0f;
        public MinMaxFloat radius = new(1.2f, 2f);
        public MinMaxInt planetCount = new() { min = 2, max = 6 };
        public MinMaxFloat planetRadius = new(0.15f, 0.35f);
        public MinMaxInt ringCount = new() { min = 1, max = 3 };
        public MinMaxInt armCount = new() { min = 2, max = 3 };
        public float spread = 0.9f;
        public float inclination = 0.08f;
        public float minSeparation = 1.5f;
        [Tooltip("Pitch/roll tilt applied to the layout, in degrees. 0 keeps the field flat (today's look).")]
        public float tilt = 0f;
        [Tooltip("Centre bulge: innermost ring/segment vertical spread as a multiple of its radius. 0 keeps the field flat.")]
        public float bulge = 0f;
    }
}

using System.Collections.Generic;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Sectors
{
    public static class SectorField
    {
        public static SectorPlacement[] Generate(SectorFieldData data)
        {
            List<SectorPlacement> sectors = new();
            int planets = 0;
            int half = HalfCells(data);
            int layers = Mathf.Max(1, data.layers);

            for (int x = -half; x <= half && sectors.Count < data.softSectorBudget; x++)
            {
                for (int z = -half; z <= half && sectors.Count < data.softSectorBudget; z++)
                {
                    for (int y = 0; y < layers && sectors.Count < data.softSectorBudget; y++)
                    {
                        SectorCell cell = new(x, y - layers / 2, z);
                        if (IsOccupied(data, cell))
                            TryPlace(data, cell, sectors, ref planets);
                    }
                }
            }

            return sectors.ToArray();
        }

        public static bool FitsInCells(SectorFieldData data) =>
            SectorGrid.FitsInCell(data.shape.radius.max, data.gap, data.cellSize);

        private static bool TryPlace(
            SectorFieldData data,
            SectorCell cell,
            List<SectorPlacement> sectors,
            ref int planets)
        {
            Vector3 center = cell.Center(data.cellSize);
            if (!IsInsideField(data, center) || OverlapsAny(data, center, sectors))
                return false;

            SectorLayout layout = SectorShape.Generate(
                data.shape,
                new SeededRandom(SectorGrid.CellSeed(data.worldSeed, cell)));

            if (planets + layout.PlanetCount > data.softPlanetBudget)
                return false;

            planets += layout.PlanetCount;
            sectors.Add(new SectorPlacement(cell, center, layout));
            return true;
        }

        private static bool OverlapsAny(
            SectorFieldData data,
            Vector3 center,
            List<SectorPlacement> sectors)
        {
            float radius = data.shape.radius.max;
            for (int i = 0; i < sectors.Count; i++)
            {
                SectorPlacement placed = sectors[i];
                if (SectorGrid.SectorsOverlap(center, radius, placed.Center, placed.Layout.SectorRadius, data.gap))
                    return true;
            }

            return false;
        }

        private static bool IsOccupied(SectorFieldData data, SectorCell cell)
        {
            uint seed = unchecked((uint)SectorGrid.CellSeed(data.worldSeed, cell));
            return seed % 1000u / 1000f < data.density;
        }

        private static bool IsInsideField(SectorFieldData data, Vector3 center) =>
            Mathf.Abs(center.x) <= data.fieldRadius
            && Mathf.Abs(center.y) <= data.fieldRadius
            && Mathf.Abs(center.z) <= data.fieldRadius;

        private static int HalfCells(SectorFieldData data) =>
            Mathf.Max(0, Mathf.CeilToInt(data.fieldRadius / Mathf.Max(data.cellSize, 0.0001f)));
    }
}

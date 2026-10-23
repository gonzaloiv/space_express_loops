using System.Collections.Generic;
using UnityEngine;

namespace DigitalLove.Game.Sectors
{
    public static class SectorSelector
    {
        public static SectorSelection[] SelectDestinations(
            SectorFieldData field,
            int levelSeed,
            int offset,
            int count)
        {
            SectorPlacement[] placements = SectorField.Generate(field);
            int[] order = BuildOrder(field, placements, levelSeed);
            int start = Mathf.Clamp(offset, 0, order.Length);
            int wanted = Mathf.Clamp(count, 0, order.Length - start);
            if (wanted == 0)
                return System.Array.Empty<SectorSelection>();

            SectorSelection[] selections = new SectorSelection[wanted];
            for (int i = 0; i < wanted; i++)
                selections[i] = SelectPlanet(placements[order[start + i]], field.worldSeed);

            return selections;
        }

        public static bool TrySelectHub(
            SectorFieldData field,
            int levelSeed,
            out SectorSelection hub)
        {
            SectorPlacement[] placements = SectorField.Generate(field);
            if (placements.Length == 0)
            {
                hub = default;
                return false;
            }

            int central = 0;
            float nearest = float.MaxValue;
            for (int i = 0; i < placements.Length; i++)
            {
                float distance = placements[i].Center.sqrMagnitude;
                if (distance < nearest)
                {
                    nearest = distance;
                    central = i;
                }
            }

            hub = SelectAnchor(placements[central], field.worldSeed, levelSeed);
            return true;
        }

        private static SectorSelection SelectPlanet(SectorPlacement placement, int worldSeed)
        {
            SectorLayout layout = placement.Layout;
            int index = 0;
            float nearest = float.MaxValue;
            for (int i = 0; i < layout.PlanetCount; i++)
            {
                float distance = layout.Planets[i].Position.sqrMagnitude;
                if (distance < nearest)
                {
                    nearest = distance;
                    index = i;
                }
            }

            SectorPlanet planet = layout.Planets[index];
            int seed = SectorGrid.CellSeed(worldSeed, placement.Cell);
            return new SectorSelection(
                placement,
                index,
                placement.Center + planet.Position,
                planet.Radius,
                seed + index + 1);
        }

        private static SectorSelection SelectAnchor(
            SectorPlacement placement,
            int worldSeed,
            int levelSeed)
        {
            int seed = SectorGrid.CellSeed(worldSeed, placement.Cell);
            return new SectorSelection(
                placement,
                -1,
                placement.Center,
                placement.Layout.AnchorRadius,
                seed + levelSeed);
        }

        private static int[] BuildOrder(SectorFieldData field, SectorPlacement[] placements, int levelSeed)
        {
            List<int> eligible = new();
            for (int i = 0; i < placements.Length; i++)
            {
                if (field.destinationRadius > 0f
                    && placements[i].Center.magnitude > field.destinationRadius)
                    continue;

                eligible.Add(i);
            }

            int[] keys = new int[eligible.Count];
            int[] order = new int[eligible.Count];
            for (int i = 0; i < eligible.Count; i++)
            {
                int placement = eligible[i];
                keys[i] = Mix(SectorGrid.CellSeed(field.worldSeed, placements[placement].Cell), levelSeed);
                order[i] = placement;
            }

            System.Array.Sort(keys, order);
            return order;
        }

        private static int Mix(int value, int salt)
        {
            unchecked
            {
                int hash = value * 486187739 ^ salt;
                hash ^= hash >> 15;
                hash *= unchecked((int)0x85EBCA6B);
                hash ^= hash >> 13;
                return hash;
            }
        }
    }
}

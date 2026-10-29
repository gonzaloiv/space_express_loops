using System.Globalization;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public static class PuzzleArrival
    {
        public static float Offset(float shipTime, float tmin, float tmax)
        {
            if (shipTime < tmin)
                return shipTime - tmin;

            return shipTime > tmax ? shipTime - tmax : 0f;
        }

        public static string Describe(float offset)
        {
            if (offset < 0f)
                return "+" + Seconds(-offset) + " s early";

            return offset > 0f ? Seconds(offset) + " s late" : "in window";
        }

        private static string Seconds(float value) =>
            value.ToString("0.0", CultureInfo.InvariantCulture);
    }
}

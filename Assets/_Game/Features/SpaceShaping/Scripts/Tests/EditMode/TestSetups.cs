using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    internal static class TestSetups
    {
        public static SpaceShapingSetup Session(int maxSteps) => new()
        {
            Origin = new Vector3(-4f, 0f, 0f),
            Destination = new Vector3(4f, 0f, 0f),
            MaxSteps = maxSteps,
            LaunchSpeed = 1f,
            Sim = new PuzzleSimSettings
            {
                CaptureRadius = 0.6f,
                StepSecs = 1f / 120f,
                SourceStrengthScale = 2f,
                SeparationScale = 1.2f,
                LightSpeed = 3.1622777f
            },
            Sources = System.Array.Empty<SourceSpec>()
        };

        public static SpaceShapingSetup Content()
        {
            SpaceShapingSetup setup = Session(4000);
            setup.Sources = new[]
            {
                new SourceSpec
                {
                    Home = new Vector3(-2f, 0f, 0f),
                    InitialScale = 2f,
                    MinScale = 1f,
                    MaxScale = 4f
                }
            };
            return setup;
        }
    }
}

using DigitalLove.Game.SpaceShaping.Cube;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using DigitalLove.Game.Spatial;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class ZoneConditionsTests
    {
        [Test]
        public void ToZoneConditions_CarriesThePuzzleFlags()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.translatable = true;
            puzzle.resizable = false;

            ZoneConditions conditions = puzzle.ToZoneConditions();

            Assert.IsTrue(conditions.Translatable);
            Assert.IsFalse(conditions.Resizable);
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void ToZoneConditions_CarriesEveryShipEndpoint()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.origin = Vector3.zero;
            puzzle.destination = Vector3.right;
            puzzle.ships = new[]
            {
                new PuzzleShip { origin = Vector3.zero, destination = Vector3.right },
                new PuzzleShip { origin = Vector3.up, destination = Vector3.down }
            };

            ZoneConditions conditions = puzzle.ToZoneConditions();

            Assert.AreEqual(4, conditions.Endpoints.Count);
            Assert.AreEqual(Vector3.up, conditions.Endpoints[2]);
            Assert.AreEqual(Vector3.down, conditions.Endpoints[3]);
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void ToZoneConditions_EmptyShipsFallsBackToThePrimaryPair()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.origin = Vector3.zero;
            puzzle.destination = Vector3.right;

            ZoneConditions conditions = puzzle.ToZoneConditions();

            Assert.AreEqual(2, conditions.Endpoints.Count);
            Assert.AreEqual(Vector3.zero, conditions.Endpoints[0]);
            Assert.AreEqual(Vector3.right, conditions.Endpoints[1]);
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void ZoneView_ExposesTheLockedGestures()
        {
            GameObject zoneObject = ZoneObject(out CubeView zone);
            zone.SetConditions(new ZoneConditions(false, true));

            Assert.IsFalse(zone.CanMove);
            Assert.IsTrue(zone.CanResize);
            Object.DestroyImmediate(zoneObject);
        }

        [Test]
        public void ZoneView_RefusesTheRunAnchors()
        {
            GameObject zoneObject = ZoneObject(out CubeView zone);
            zone.SetConditions(new ZoneConditions(
                true, true, new[] { Vector3.zero, new Vector3(1f, 0f, 0f) }));

            Assert.IsFalse(zone.CanPlace(Vector3.zero, Vector3.one * 0.2f).Allowed);
            Assert.IsTrue(zone.CanPlace(new Vector3(0f, 0.5f, 0f), Vector3.one * 0.2f).Allowed);
            Object.DestroyImmediate(zoneObject);
        }

        [Test]
        public void ZoneView_NamesTheEndpointItRefuses()
        {
            GameObject zoneObject = ZoneObject(out CubeView zone);
            zone.SetConditions(new ZoneConditions(
                true, true, new[] { Vector3.zero, new Vector3(1f, 0f, 0f) }));

            ZonePlacement placement = zone.CanPlace(new Vector3(1f, 0f, 0f), Vector3.one * 0.2f);

            Assert.IsTrue(placement.Blocked);
            Assert.AreEqual(1, placement.Endpoint);
            Object.DestroyImmediate(zoneObject);
        }

        [Test]
        public void CubeFeedback_DormantUsesTheLockedColor()
        {
            GameObject zoneObject = ZoneObject(out CubeView zone);
            GameObject frameObject = new("Frame");
            GameObject fillObject = new("Fill");
            MeshRenderer frame = frameObject.AddComponent<MeshRenderer>();
            MeshRenderer fill = fillObject.AddComponent<MeshRenderer>();
            CubePalette palette = new()
            {
                Neutral = Color.white,
                Accent = Color.blue,
                Danger = Color.red,
                FillIdleAlpha = 0.1f,
                FillActiveAlpha = 0.45f
            };
            CubeFeedback feedback = new(frame, fill, palette);

            feedback.SetDormant(true);
            feedback.Apply(zone, 2, false, false, Vector3.zero);
            Color dormant = ReadBaseColor(fill);
            Assert.IsTrue(feedback.IsDormant);
            Assert.AreEqual(1f, dormant.r, 1e-4f);
            Assert.AreEqual(0f, dormant.g, 1e-4f);
            Assert.AreEqual(0f, dormant.b, 1e-4f);
            Assert.AreEqual(0.1f, dormant.a, 1e-4f);

            feedback.SetDormant(false);
            feedback.Apply(zone, 2, false, false, Vector3.zero);
            Assert.AreNotEqual(dormant, ReadBaseColor(fill));

            Object.DestroyImmediate(zoneObject);
            Object.DestroyImmediate(frameObject);
            Object.DestroyImmediate(fillObject);
        }

        [Test]
        public void ZoneView_AllowsAFrameInsideTheOldFalloffBand()
        {
            GameObject zoneObject = ZoneObject(out CubeView zone);
            zone.SetFalloff(0.5f);
            zone.SetConditions(new ZoneConditions(
                true, true, new[] { new Vector3(0.12f, 0f, 0f) }));

            ZonePlacement placement = zone.CanPlace(Vector3.zero, Vector3.one * 0.2f);

            Assert.IsTrue(placement.Allowed);
            Object.DestroyImmediate(zoneObject);
        }

        private static Color ReadBaseColor(Renderer renderer)
        {
            MaterialPropertyBlock block = new();
            renderer.GetPropertyBlock(block);
            return block.GetColor(Shader.PropertyToID("_BaseColor"));
        }

        private static GameObject ZoneObject(out CubeView zone)
        {
            GameObject content = new("Content");
            GameObject zoneObject = new("Zone");
            zoneObject.SetActive(false);
            content.transform.SetParent(zoneObject.transform, false);
            zone = zoneObject.AddComponent<CubeView>();

            SerializedObject serialized = new(zone);
            serialized.FindProperty("contentVisual").objectReferenceValue = content.transform;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            zoneObject.SetActive(true);
            return zoneObject;
        }
    }
}

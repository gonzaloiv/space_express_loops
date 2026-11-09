using DigitalLove.Game.SpaceShaping.Cube;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

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
                true, true, Vector3.zero, new Vector3(1f, 0f, 0f)));

            Assert.IsFalse(zone.CanPlace(Vector3.zero, Vector3.one * 0.2f));
            Assert.IsTrue(zone.CanPlace(new Vector3(0f, 0.5f, 0f), Vector3.one * 0.2f));
            Object.DestroyImmediate(zoneObject);
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

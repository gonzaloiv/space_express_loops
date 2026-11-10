using DigitalLove.Global;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.UI.Console.Tests.EditMode
{
    public class ConsoleBankLayoutTests
    {
        private GameObject origin;
        private GameObject bankRoot;
        private ConsoleBank bank;
        private LineRenderer cardArm;
        private LineRenderer jimArm;
        private GameObject card;
        private GameObject jim;
        private ColorValue armColor;

        [SetUp]
        public void SetUp()
        {
            origin = new GameObject("Origin");
            origin.transform.SetPositionAndRotation(
                new Vector3(0.8f, 1.5f, -0.3f), Quaternion.Euler(0f, 37f, 0f));

            bankRoot = new GameObject("Console Bank");
            bankRoot.transform.SetParent(origin.transform, false);
            bankRoot.transform.localPosition = new Vector3(0.05f, -0.02f, 0.1f);
            bankRoot.transform.localRotation = Quaternion.Euler(0f, 11f, 0f);
            bank = bankRoot.AddComponent<ConsoleBank>();

            card = Window("Card", bankRoot.transform);
            jim = Window("Jim", bankRoot.transform);
            cardArm = Arm("Card Arm");
            jimArm = Arm("Jim Arm");
            armColor = ScriptableObject.CreateInstance<ColorValue>();

            SerializedObject so = new(bank);
            so.FindProperty("jim").objectReferenceValue = jim.GetComponent<RectTransform>();
            so.FindProperty("cardArm").objectReferenceValue = cardArm;
            so.FindProperty("jimArm").objectReferenceValue = jimArm;
            SerializedProperty cards = so.FindProperty("cards");
            cards.arraySize = 1;
            cards.GetArrayElementAtIndex(0).objectReferenceValue = card.GetComponent<RectTransform>();
            so.FindProperty("armColor").objectReferenceValue = armColor;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(origin);
            Object.DestroyImmediate(armColor);
        }

        [Test]
        public void Layout_UnderAMovedAndRotatedOrigin_PutsTheCornerOnTheField()
        {
            Vector3 parentCorner = new(0.2f, -0.25f, 0.7f);
            bank.Layout(parentCorner, Quaternion.Euler(0f, 23f, 0f));

            Matrix4x4 local = Matrix4x4.TRS(
                bankRoot.transform.localPosition,
                bankRoot.transform.localRotation,
                bankRoot.transform.localScale);
            Vector3 expectedLocal = local.inverse.MultiplyPoint3x4(parentCorner);
            Assert.Less(Vector3.Distance(expectedLocal, cardArm.GetPosition(0)), 1e-5f);

            Vector3 expectedWorld = origin.transform.TransformPoint(parentCorner);
            Vector3 actualWorld = cardArm.transform.TransformPoint(cardArm.GetPosition(0));
            Assert.Less(Vector3.Distance(expectedWorld, actualWorld), 1e-5f);
        }

        [Test]
        public void Layout_KeepsTheWindowRotationInTheParentsSpace()
        {
            Quaternion parentRotation = Quaternion.Euler(0f, 23f, 0f);
            bank.Layout(new Vector3(0.2f, -0.25f, 0.7f), parentRotation);

            Quaternion expected =
                Quaternion.Inverse(bankRoot.transform.localRotation) * parentRotation;
            Assert.Less(
                Quaternion.Angle(expected, card.GetComponent<RectTransform>().localRotation), 1e-3f);
        }

        private static GameObject Window(string name, Transform parent)
        {
            GameObject go = new(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private LineRenderer Arm(string name)
        {
            GameObject go = new(name);
            go.transform.SetParent(bankRoot.transform, false);
            LineRenderer line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            return line;
        }
    }
}

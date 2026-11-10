using DigitalLove.Game.SpaceShaping;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace DigitalLove.Game.GesturePrompts.Editor
{
    // The meter's base square in the prompt art: two arms that recede from the front-centre, the
    // star-band ticks, and a star above the square. Rendered as white geometry; LineArt tints it.
    public sealed class MeterRig
    {
        private const int Steps = 49;
        private const float Half = 0.34f;
        private const float CentreY = -0.55f;
        private const float TickLength = 0.055f;
        private const float StarOuter = 0.105f;
        private const float StarInner = 0.045f;
        private const float StarGap = 0.14f;

        private readonly float unit;
        private readonly LineRenderer[] arms = new LineRenderer[2];
        private readonly LineRenderer[] ticks = new LineRenderer[4];
        private readonly Renderer star;
        private readonly Mesh starMesh;
        private readonly Vector3[] points = new Vector3[Steps];

        public MeterRig(Scene scene, Material material, float unit)
        {
            this.unit = unit;
            for (int i = 0; i < arms.Length; i++)
                arms[i] = BuildLine(scene, material, "Meter Arm " + i, 0.022f * unit);
            for (int i = 0; i < ticks.Length; i++)
                ticks[i] = BuildLine(scene, material, "Meter Tick " + i, 0.018f * unit);
            star = BuildStar(scene, material, out starMesh);
            HideAll();
        }

        public void ShowMeter(MeterFrame? frame)
        {
            bool visible = frame.HasValue;
            for (int i = 0; i < arms.Length; i++)
                arms[i].enabled = visible;
            for (int i = 0; i < ticks.Length; i++)
                ticks[i].enabled = visible;

            if (!visible)
                return;

            LayArms(frame.Value.Spend);
            LayTicks();
        }

        public void ShowStar(bool visible) => star.enabled = visible;

        public void HideAll()
        {
            ShowMeter(null);
            ShowStar(false);
        }

        public void Dispose() => Object.DestroyImmediate(starMesh);

        private void LayArms(float spend)
        {
            for (int side = 0; side < arms.Length; side++)
            {
                float sign = side == 0 ? -1f : 1f;
                for (int i = 0; i < Steps; i++)
                {
                    float s = Mathf.Lerp(spend, 1f, i / (Steps - 1f));
                    points[i] = IconPoint(FieldEdgePath.Point(s, sign, Half * unit));
                }

                arms[side].positionCount = Steps;
                arms[side].SetPositions(points);
            }
        }

        private void LayTicks()
        {
            LayTick(0, 1f / 3f, -1f);
            LayTick(1, 1f / 3f, 1f);
            LayTick(2, 2f / 3f, -1f);
            LayTick(3, 2f / 3f, 1f);
        }

        private void LayTick(int index, float s, float side)
        {
            Vector3 foot = IconPoint(FieldEdgePath.Point(s, side, Half * unit));
            ticks[index].positionCount = 2;
            ticks[index].SetPosition(0, foot);
            ticks[index].SetPosition(1, foot + Vector3.up * TickLength * unit);
        }

        // The field path lies flat (x, −half, z); the icon stands it up in the image plane.
        private Vector3 IconPoint(Vector3 point) => new(point.x, CentreY * unit + point.z, 0f);

        private LineRenderer BuildLine(Scene scene, Material material, string name, float width)
        {
            var go = new GameObject(name);
            SceneManager.MoveGameObjectToScene(go, scene);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.useWorldSpace = false;
            line.widthMultiplier = width;
            line.numCapVertices = 2;
            line.numCornerVertices = 2;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            return line;
        }

        private Renderer BuildStar(Scene scene, Material material, out Mesh mesh)
        {
            var go = new GameObject("Meter Star");
            SceneManager.MoveGameObjectToScene(go, scene);
            Vector3 centre = new(0f, (CentreY + Half + StarGap) * unit, 0f);
            var vertices = new Vector3[11];
            vertices[0] = centre;
            for (int i = 0; i < 10; i++)
            {
                float angle = Mathf.PI * 0.5f + i * Mathf.PI * 2f / 10f;
                float radius = (i % 2 == 0 ? StarOuter : StarInner) * unit;
                vertices[i + 1] = centre + new Vector3(
                    Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            }

            var triangles = new int[20 * 3];
            for (int i = 0; i < 10; i++)
            {
                int next = i + 1 == 10 ? 1 : i + 2;
                int at = i * 6;
                triangles[at] = 0;
                triangles[at + 1] = i + 1;
                triangles[at + 2] = next;
                triangles[at + 3] = 0;
                triangles[at + 4] = next;
                triangles[at + 5] = i + 1;
            }

            var built = new Mesh { name = "Meter Star" };
            built.vertices = vertices;
            built.triangles = triangles;
            built.RecalculateNormals();
            go.AddComponent<MeshFilter>().sharedMesh = built;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            mesh = built;
            return renderer;
        }
    }
}

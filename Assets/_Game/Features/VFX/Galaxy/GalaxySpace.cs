using System.Collections.Generic;
using DigitalLove.Global;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace DigitalLove.Game.VFX
{
    public class GalaxySpace : MonoBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private ParticleSystem ps;

        private void Start()
        {
            MRUK.Instance.SceneLoadedEvent.AddListener(OnSceneLoaded);
        }

        private void OnSceneLoaded()
        {
            SetupMaterial();
            SetupPS();
        }

        private void SetupMaterial()
        {
            List<MRUKAnchor> anchors = new();
            anchors.AddRange(MRUK.Instance.GetCurrentRoom().FloorAnchors);
            anchors.AddRange(MRUK.Instance.GetCurrentRoom().CeilingAnchors);
            anchors.AddRange(MRUK.Instance.GetCurrentRoom().WallAnchors);
            foreach (MRUKAnchor anchor in anchors)
            {
                MeshRenderer rend = anchor.GetComponentInChildren<MeshRenderer>();
                if (rend != null)
                {
                    MeshRenderer spawned = Instantiate(rend, transform);
                    spawned.transform.SetWorldPose(rend.transform.ToWorldPose());
                    if (spawned != null)
                    {
                        spawned.material = material;
                        MeshFilter filter = spawned.GetComponent<MeshFilter>();
                        spawned.material.mainTextureScale = new Vector2(filter.mesh.bounds.size.x, filter.mesh.bounds.size.y);
                    }
                }
            }
        }

        private void SetupPS()
        {
            ShapeModule shapeModule = ps.shape;
            Bounds roomBounds = MRUK.Instance.GetCurrentRoom().GetRoomBounds();
            shapeModule.scale = Vector3.one * roomBounds.size.magnitude;
            ps.transform.position = roomBounds.center;
            ps.transform.rotation = MRUK.Instance.GetCurrentRoom().transform.rotation;
            ps.Play();
        }
    }
}
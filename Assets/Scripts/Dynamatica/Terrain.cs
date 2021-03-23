using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dynamatica.Unity.Components
{
    public class TerrainBuilder : MonoBehaviour
    {
        [SerializeField] LayerMask layer;

        [Header("Terrain")]
        [SerializeField] Vector2 size = new Vector2(10, 10);
        [SerializeField] float unitSize = 0.01f;
        [SerializeField] float height = 100;

        public Terrain terrain { get; private set; }

        void Awake()
        {
            uint x = (uint)(size.x / unitSize);
            uint y = (uint)(size.y / unitSize);
            terrain = new Terrain(transform.position, x, y, unitSize);
            Build();
        }

        void OnDrawGizmos()
        {
            Vector3 center = transform.position;
            center.z += this.size.x / 2;
            center.x -= this.size.y / 2;

            Vector3 size = new Vector3(this.size.y, height * 2, this.size.x);

            Gizmos.DrawCube(center, size);
        }

        void Build()
        {
            for (uint i = 0; i <= terrain.x; ++i)
            {
                for (uint j = 0; j <= terrain.y; ++j)
                {
                    var origin = transform.position;
                    origin.z += i * unitSize;
                    origin.x -= j * unitSize;
                    origin.y += height;

                    var ray = new Ray(origin, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, height * 2, layer))
                        terrain.SetHeight(i, j, hit.point.y);
                }
            }
        }
    }

    public class TerrainTracker : MonoBehaviour
    {
        public Terrain terrain;
        [SerializeField] Transform @base;

        void LateUpdate()
        {
            if (terrain == null) return;

            var position = transform.position;
            var projection = new Vector2(position.z, -position.x);

            position.y = (float)terrain.GetHeight(projection);
            transform.position = position;

            var derivatives = terrain.GetHeightDerivatives(projection);

            var forward = new Vector3(0, derivatives.x, 1);
            var left = new Vector3(-1, derivatives.y, 0);
            var up = Vector3.Cross(left, forward);
            @base.rotation = Quaternion.FromToRotation(Vector3.up, up) * transform.rotation;
        }
    }
}

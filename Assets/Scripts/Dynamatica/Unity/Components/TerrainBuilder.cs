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

        void OnDrawGizmosSelected()
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
}

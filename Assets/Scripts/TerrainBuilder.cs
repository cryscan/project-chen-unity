using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBuilder : MonoBehaviour
{
    [SerializeField] LayerMask layer;

    [Header("Terrain")]
    [SerializeField] Vector2 size = new Vector2(10, 10);
    [SerializeField] float unitSize = 0.1f;

    [SerializeField] float maxHeight = 100;

    public HopperAPI.Terrain terrain { get; private set; }

    uint x, y;

    void Awake()
    {
        x = (uint)(size.x / unitSize);
        y = (uint)(size.y / unitSize);
        terrain = new HopperAPI.Terrain(transform.position, x, y, unitSize);
        CastTerrain();
    }

    void OnDrawGizmos()
    {
        Vector3 center = transform.position;
        center.z += size.x / 2;
        center.x -= size.y / 2;
        center.y += maxHeight / 2;

        Vector3 _size = new Vector3(size.y, maxHeight, size.x);

        Gizmos.DrawWireCube(center, _size);
    }

    void CastTerrain()
    {
        for (uint i = 0; i <= x; ++i)
        {
            for (uint j = 0; j <= y; ++j)
            {
                Vector3 start = transform.position;
                start.z += i * unitSize;
                start.x -= j * unitSize;
                start.y += maxHeight;

                var ray = new Ray(start, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, maxHeight, layer))
                {
                    var height = hit.point.y;
                    terrain.SetHeight(i, j, height);
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFollower : MonoBehaviour
{
    public TerrainBuilder terrainBuilder;

    [SerializeField] Transform @base;

    HopperAPI.Terrain terrain;

    void Start()
    {
        terrain = terrainBuilder.terrain;
    }

    void LateUpdate()
    {
        var position = transform.position;
        position.y = (float)terrain.GetHeight(position.z, -position.x);
        transform.position = position;

        Vector2 derivatives = terrain.GetHeightDerivatives(position.z, -position.x);

        var forward = new Vector3(0, derivatives.x, 1);
        var left = new Vector3(-1, derivatives.y, 0);
        var up = Vector3.Cross(left, forward);
        @base.rotation = Quaternion.FromToRotation(Vector3.up, up) * transform.rotation;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTest : MonoBehaviour
{
    [SerializeField] TerrainBuilder terrainBuilder;

    [SerializeField] Transform forward, left;

    HopperAPI.Terrain terrain;

    void Start()
    {
        terrain = terrainBuilder.terrain;
        forward.localPosition = new Vector3(0, 0, 1);
        left.localPosition = new Vector3(-1, 0, 0);
    }

    void Update()
    {
        var position = transform.position;
        position.y = (float)terrain.GetHeight(position.z, -position.x);
        transform.position = position;

        Vector2 derivatives = terrain.GetHeightDerivatives(position.z, -position.x);

        position = forward.localPosition;
        position.y = derivatives.x;
        forward.localPosition = position;

        position = left.localPosition;
        position.y = derivatives.y;
        left.localPosition = position;
    }
}

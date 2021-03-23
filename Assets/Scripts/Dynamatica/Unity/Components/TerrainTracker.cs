using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dynamatica.Unity.Components
{
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

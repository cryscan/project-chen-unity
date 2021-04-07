using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dynamatica.Unity.Components
{
    [DisallowMultipleComponent]
    public class EndEffector : MonoBehaviour
    {
        public Vector3 force;
        public bool contact { get => force.magnitude > 0; }

        [SerializeField] Material stanceMaterial, flightMaterial;

        MeshRenderer mesh;

        void Awake()
        {
            mesh = GetComponentInChildren<MeshRenderer>();
        }

        void Update()
        {
            mesh.material = contact ? stanceMaterial : flightMaterial;
        }
    }
}

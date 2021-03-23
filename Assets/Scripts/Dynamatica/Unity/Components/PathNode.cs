using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dynamatica.Runtime;

namespace Dynamatica.Unity.Components
{
    [DisallowMultipleComponent]
    public class PathNode : MonoBehaviour
    {
        public Dim6D bounds = Dim6D.LX | Dim6D.LZ | Dim6D.AY;
        public Vector3Int revolutions;
    }
}

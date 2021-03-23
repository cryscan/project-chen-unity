using System.Linq;
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

    public class Path : MonoBehaviour
    {
        [Header("Trajectory")]
        [SerializeField] float speed = 1;
        [SerializeField] float minDuration = 1.2f;

        [Header("Gait")]
        [SerializeField] float stepFrequency = 1;
        [SerializeField] Gait nominalGait;

        public bool valid { get; private set; }
        public float length { get; private set; } = 0;
        public float duration { get => Mathf.Max(minDuration, length / speed); }

        public PathPoint[] pathPoints { get; private set; }
        public Gait[] gaits { get; private set; }

        PathNode[] nodes;

        void Awake()
        {
            nodes = GetComponentsInChildren<PathNode>();
            valid = nodes.Length > 1;

            if (valid)
            {
                InitPathPoints();
                InitGaits();
            }
        }

        void InitPathPoints()
        {
            length = 0;

            List<float> accumulator = new List<float>() { 0 };
            for (int i = 0; i < nodes.Length - 1; ++i)
            {
                var distance = Vector3.Distance(nodes[i].transform.position, nodes[i + 1].transform.position);
                accumulator.Add(distance);
                length += distance;
            }

            pathPoints = new PathPoint[nodes.Length];
            for (int i = 0; i < pathPoints.Length; ++i)
            {
                var node = nodes[i];
                ref var pathPoint = ref pathPoints[i];
                pathPoint = new PathPoint();
                pathPoint.time = accumulator[i] / length * duration;
                pathPoint.linear = node.transform.position;

                pathPoint.angular = ConvertEulerAngles(node.transform.eulerAngles);
                pathPoint.angular += node.revolutions * 360;

                pathPoint.bounds = node.bounds;
            }
        }

        void InitGaits()
        {
            int steps = Mathf.CeilToInt(duration * stepFrequency);
            if (steps == 0) return;

            gaits = new Gait[steps];
            gaits[0] = gaits[steps - 1] = Gait.Stand;

            for (int i = 1; i < steps - 1; ++i) gaits[i] = nominalGait;
        }

        static float ConvertAngle(float angle) => angle % 360 - 180;
        static Vector3 ConvertEulerAngles(Vector3 angles)
        {
            angles.x = ConvertAngle(angles.x);
            angles.y = ConvertAngle(angles.y);
            angles.z = ConvertAngle(angles.z);
            return angles;
        }
    }
}
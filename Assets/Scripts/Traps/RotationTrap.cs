using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RotationTrap : MonoBehaviour
{
    [SerializeField] Transform trap;
    [SerializeField] Vector3 angularVelocity;

    [Header("Path")]
    [SerializeField] Transform[] pathPoints;
    [SerializeField] float speed = 4;

    float timer = 0;
    List<float> timePoints = new List<float>();

    void Awake()
    {
        if (pathPoints.Length > 1)
        {
            timePoints.Add(0);
            for (int i = 1; i < pathPoints.Length; ++i)
            {
                var prev = pathPoints[i - 1];
                var curr = pathPoints[i];
                var distance = Vector3.Distance(prev.position, curr.position);
                var time = distance / speed;

                var prevTime = timePoints[i - 1];
                timePoints.Add(prevTime + time);
            }
        }
    }

    void FixedUpdate()
    {
        trap.Rotate(angularVelocity * Time.fixedDeltaTime, Space.Self);

        if (pathPoints.Length > 1)
        {
            var index = timePoints.FindLastIndex(t => t <= timer);
            if (index == timePoints.Count - 1)
            {
                timer = 0;
                index = 0;
            }

            var prev = pathPoints[index];
            var next = pathPoints[index + 1];

            var prevTime = timePoints[index];
            var nextTime = timePoints[index + 1];
            var factor = (timer - prevTime) / (nextTime - prevTime);

            trap.position = Vector3.Lerp(prev.position, next.position, factor);
        }

        timer += Time.fixedDeltaTime;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationCorrection
{
    public class WallAngle : MonoBehaviour
    {
        [SerializeField] Transform wall;
        [SerializeField] Animator animator;
        [SerializeField] AnimationCurve wallAngleCurve;

        bool started = false;
        float timer = 0;

        void Update()
        {
            if (!started) return;

            var rotation = new Vector3();
            rotation.x = wallAngleCurve.Evaluate(timer);
            wall.rotation = Quaternion.Euler(rotation);

            timer += Time.deltaTime;
        }

        void OnGUI()
        {
            if (GUILayout.Button("Start Wall Angle"))
            {
                animator.SetTrigger("Start");
                started = true;
                timer = 0;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnimationCorrection
{
    public class WallAngle : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] Transform[] anchors;
        [SerializeField] AnimationCurve[] anchorAngleCurves;

        float timer = 0;
        bool started => AnimationCorrectionManager.instance.started;

        void Update()
        {
            if (!started) return;

            for (int i = 0; i < anchors.Length; ++i)
            {
                var curve = anchorAngleCurves[i];
                if (timer > curve.keys.Last().time) continue;

                var rotation = new Vector3();
                rotation.x = anchorAngleCurves[i].Evaluate(timer);
                anchors[i].rotation = Quaternion.Euler(rotation);
            }

            timer += Time.deltaTime;
        }
    }
}

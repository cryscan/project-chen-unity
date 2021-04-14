using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationCorrection
{
    public class RootHeight : MonoBehaviour
    {
        [SerializeField] Model self, target;
        [SerializeField] Animator animator;

        [SerializeField] AnimationCurve rootHeightCurve;

        public float timer = 0;
        bool started = false;

        void Update()
        {
            if (!started) return;

            UpdateRoot();

            self.body.position = target.body.position;
            self.body.rotation = target.body.rotation;

            for (int i = 0; i < self.ees.Length; ++i)
            {
                self.ees[i].position = target.ees[i].position;
                self.ees[i].rotation = target.ees[i].rotation;
            }

            timer += Time.deltaTime;
        }

        void OnDrawGizmos()
        {
            if (self.root)
            {
                Gizmos.DrawWireSphere(self.root.position, 0.1f);
            }
        }

        void OnGUI()
        {
            if (GUILayout.Button("Start Root Height"))
            {
                animator.SetTrigger("Start");
                timer = 0;
                started = true;
            }
        }

        void UpdateRoot()
        {
            var position = target.root.position;
            position.y = rootHeightCurve.Evaluate(timer);
            self.root.position = position;

            self.root.rotation = target.root.rotation;
        }
    }
}
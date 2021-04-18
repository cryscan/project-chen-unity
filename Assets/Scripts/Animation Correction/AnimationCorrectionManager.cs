using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationCorrection
{
    public class AnimationCorrectionManager : MonoBehaviour
    {
        public static AnimationCorrectionManager instance { get; private set; }

        public bool started { get; private set; } = false;

        [SerializeField] Animator animator;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this) Destroy(gameObject);
        }

        void OnGUI()
        {
            if (GUILayout.Button("Start"))
            {
                started = true;
                animator.SetTrigger("Start");
            }
        }
    }
}

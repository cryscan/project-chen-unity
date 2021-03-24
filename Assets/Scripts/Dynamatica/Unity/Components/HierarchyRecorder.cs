using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace Dynamatica.Unity.Components
{
    public class HierarchyRecorder : MonoBehaviour
    {
        public AnimationClip clip;
        [SerializeField] bool recording = false;

#if UNITY_EDITOR
        GameObjectRecorder recorder;
#endif

        void Start()
        {
#if UNITY_EDITOR
            recorder = new GameObjectRecorder(gameObject);
            recorder.BindComponentsOfType<Transform>(gameObject, true);
            recorder.BindComponentsOfType<EndEffector>(gameObject, true);
#endif
        }

        void LateUpdate()
        {
            if (!clip) return;
            if (!recording) return;

#if UNITY_EDITOR
            recorder.TakeSnapshot(Time.deltaTime);
#endif
        }

        public void Record() => recording = true;

        public void EndRecording()
        {
            if (!recording) return;
            recording = false;

#if UNITY_EDITOR
            if (clip) recorder.SaveToClip(clip);
#endif
        }
    }
}

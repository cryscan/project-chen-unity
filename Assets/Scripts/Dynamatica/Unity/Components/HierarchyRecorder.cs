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

        [SerializeField] bool _recording = false;
        public bool recording { get => _recording; }

#if UNITY_EDITOR
        GameObjectRecorder recorder;
#endif

        void Start()
        {
#if UNITY_EDITOR
            recorder = new GameObjectRecorder(gameObject);
            recorder.BindComponentsOfType<Animator>(gameObject, false);
            recorder.BindComponentsOfType<EndEffector>(gameObject, true);
#endif
        }

        void LateUpdate()
        {
            if (!clip) return;
            if (!_recording) return;

#if UNITY_EDITOR
            recorder.TakeSnapshot(Time.deltaTime);
#endif
        }

        public void BindTransform(GameObject gameObject)
        {
#if UNITY_EDITOR
            recorder.BindComponentsOfType<Transform>(gameObject, true);
#endif
        }

        public void Record()
        {
            if (clip) clip.ClearCurves();
            _recording = true;
        }

        public void EndRecording()
        {
            if (!_recording) return;
            _recording = false;

#if UNITY_EDITOR
            if (clip) recorder.SaveToClip(clip);
#endif
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

namespace Dynamatica.Unity.Components
{
    public class HierarchyRecorder : MonoBehaviour
    {
        public AnimationClip clip;
        [SerializeField] bool recording = false;

        GameObjectRecorder recorder;

        void Start()
        {
            recorder = new GameObjectRecorder(gameObject);
            recorder.BindComponentsOfType<Transform>(gameObject, true);
            recorder.BindComponentsOfType<EndEffector>(gameObject, true);
        }

        void LateUpdate()
        {
            if (!clip) return;
            if (!recording) return;

            recorder.TakeSnapshot(Time.deltaTime);
        }

        public void Record() => recording = true;

        public void EndRecording()
        {
            if (!recording) return;
            recording = false;

            if (clip) recorder.SaveToClip(clip);
        }
    }
}

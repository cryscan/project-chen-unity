using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

public class HierarchyRecorder : MonoBehaviour
{
    public AnimationClip clip;
    [SerializeField] bool recording = false;

    GameObjectRecorder recorder;

    void Start()
    {
        recorder = new GameObjectRecorder(gameObject);
        recorder.BindComponentsOfType<Transform>(gameObject, true);
    }

    void LateUpdate()
    {
        if (!clip) return;
        if (!recording) return;

        recorder.TakeSnapshot(Time.deltaTime);
    }

    public void BeginRecording() => recording = true;

    public void EndRecording()
    {
        if (!recording) return;
        recording = false;

        if (clip) recorder.SaveToClip(clip);
    }
}

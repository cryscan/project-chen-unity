using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public enum Space { World, Local }

    [SerializeField] Space space;
    [SerializeField] Transform target;
    [SerializeField] Transform source;
    [SerializeField] Vector3 offest;

    Vector3 position;
    Quaternion rotation;

    void Awake()
    {
        position = target.position;
        rotation = target.rotation;
    }

    void Update()
    {
        switch (space)
        {
            case Space.World:
                target.position = source.position + offest;
                break;
            case Space.Local:
                target.localPosition = source.localPosition + offest;
                break;
        }
        // target.rotation = rotation * source.rotation * Quaternion.Inverse(rotation);
        target.rotation = source.rotation * rotation;
    }
}

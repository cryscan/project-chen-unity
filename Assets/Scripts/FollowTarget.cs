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
    [SerializeField] Vector3 scale = new Vector3(1, 1, 1);

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
                target.rotation = source.rotation * rotation;
                break;
            case Space.Local:
                var position = source.localPosition + offest;
                position.x *= scale.x;
                position.y *= scale.y;
                position.z *= scale.z;
                target.localPosition = position;
                target.localRotation = source.localRotation * rotation;
                break;
        }
        // target.rotation = rotation * source.rotation * Quaternion.Inverse(rotation);
    }
}

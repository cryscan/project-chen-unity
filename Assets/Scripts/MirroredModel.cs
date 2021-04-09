using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirroredModel : MonoBehaviour
{
    [System.Serializable]
    struct Model
    {
        public Transform root;
        public Transform body;
        public Transform[] ees;
    }

    [SerializeField] Model self;
    [SerializeField] Model target;

    float anchor;

    void Start()
    {
        anchor = target.root.position.x;
    }

    void Update()
    {
        ReflectTransform(self.root, target.root);
        ReflectTransform(self.body, target.body);

        for (int i = 0; i < self.ees.Length; ++i)
            ReflectTransform(self.ees[i], target.ees[i]);
    }

    Vector3 ReflectPosition(Vector3 pos)
    {
        return new Vector3(2 * anchor - pos.x, pos.y, pos.z);
    }

    Vector3 ReflectDirection(Vector3 dir)
    {
        return new Vector3(-dir.x, dir.y, dir.z);
    }

    Quaternion ReflectRotation(Quaternion rotation)
    {
        Vector3 axis;
        float angle;
        rotation.ToAngleAxis(out angle, out axis);
        return Quaternion.AngleAxis(-angle, ReflectDirection(axis));
    }

    void ReflectTransform(Transform source, Transform target)
    {
        source.position = ReflectPosition(target.position);
        source.rotation = ReflectRotation(target.rotation);
    }
}

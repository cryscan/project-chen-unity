using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Unity.Kinematica;
using Unity.Mathematics;

public class Climber : MonoBehaviour
{
    [System.Serializable]
    public struct Model
    {
        public Transform root;
        public Transform body;
        public Transform[] limbs;
        public float[] ranges;
    }

    [SerializeField] Model model;
    [SerializeField] LayerMask layer;
    [SerializeField] AnimationCurve limbCurve;

    Vector3[] stance;
    bool limbMoving = false;

    float3 movementDirection;
    float moveIntensity;

    void Start()
    {
        stance = model.limbs.Select(x => x.localPosition).ToArray();
    }

    void Update()
    {
        Utility.GetInputMove(ref movementDirection, ref moveIntensity);
    }

    void FixedUpdate()
    {
    }

    bool MatchSurface(Vector3 position, Vector3 direction, float range, out Vector3 point, out Vector3 normal)
    {
        Ray ray = new Ray(position - range * direction, direction);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 2 * range, layer))
        {
            point = hit.point;
            normal = hit.normal;
            return true;
        }
        else
        {
            point = position;
            normal = Vector3.zero;
            return false;
        }
    }

    IEnumerator PerformMove(Transform limb, Vector3 target)
    {
        limbMoving = true;
        float timer = 0;

        var position = limb.position;
        var totalTime = limbCurve.keys.Last().time;

        while (timer < totalTime)
        {
            var factor = limbCurve.Evaluate(timer);
            limb.position = Vector3.Lerp(position, target, factor);

            yield return null;
        }

        limbMoving = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[RequireComponent(typeof(ClimberController))]
public class ClimberAnimation : MonoBehaviour
{
    [System.Serializable]
    public struct Model
    {
        public Transform body;
        public Transform[] limbs;
        public float[] ranges;
    }

    [SerializeField] Model model;
    [SerializeField] LayerMask layer;
    [SerializeField] AnimationCurve limbCurve;
    [SerializeField] float stepHeight;

    [SerializeField] float damp = 10;

    Vector3 bodyStance;
    Quaternion bodyRotation;

    Vector3[] limbStance;
    Vector3[] limbPositions;
    bool limbMoving = false;

    ClimberController controller;

    void Awake()
    {
        controller = GetComponent<ClimberController>();
    }

    void Start()
    {
        limbStance = model.limbs.Select(x => x.localPosition).ToArray();
        limbPositions = model.limbs.Select(x => x.position).ToArray();

        bodyStance = model.body.localPosition - limbStance.Aggregate((acc, x) => acc + x) / limbStance.Length;
        bodyRotation = model.body.localRotation;
    }

    void Update()
    {
        var limbCount = model.limbs.Length;

        Transform limb;
        Vector3 target;
        int index = FindMaximumDeviatedLimb(out limb, out target);

        for (int i = 0; i < limbCount; ++i)
            if (model.limbs[i] != limb)
                model.limbs[i].position = limbPositions[i];

        if (limb != null && !limbMoving)
        {
            StartCoroutine(PerformMove(limb, target));
            limbPositions[index] = target;
        }

        var position = model.limbs.Select(x => x.position).Aggregate((acc, x) => acc + x) / limbCount;
        position += transform.TransformVector(bodyStance);
        model.body.position = model.body.position.Fallout(position, damp);
    }

    void LateUpdate()
    {
        if (model.limbs.Length > 3)
        {
            var v1 = model.limbs[0].position - model.limbs[3].position;
            var v2 = model.limbs[1].position - model.limbs[2].position;
            var normal = Vector3.Cross(v1, v2).normalized;
            var rotation = Quaternion.FromToRotation(transform.up, normal) * bodyRotation;
            model.body.localRotation = model.body.localRotation.Fallout(rotation, damp);

            Debug.DrawRay(model.body.position, v1, Color.cyan);
            Debug.DrawRay(model.body.position, v2, Color.cyan);
            Debug.DrawRay(model.body.position, normal, Color.cyan);
        }
    }

    void OnDrawGizmos()
    {
        foreach (var limb in model.limbs)
            Gizmos.DrawWireSphere(limb.position, 0.1f);
    }

    public void Move(Vector3 position, Quaternion rotation)
    {
        controller.Move(position, rotation);

        StopAllCoroutines();
        limbMoving = false;

        for (int i = 0; i < model.limbs.Length; ++i)
            limbPositions[i] = model.limbs[i].position = transform.TransformPoint(limbStance[i]);
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
            limb.position += 4 * stepHeight * factor * (1 - factor) * transform.up;

            timer += Time.deltaTime;
            yield return null;
        }

        limbMoving = false;
    }

    int FindMaximumDeviatedLimb(out Transform limb, out Vector3 target)
    {
        int index = -1;
        limb = null;
        target = new Vector3();
        float maximumDeviation = 0;

        for (int i = 0; i < model.limbs.Length; ++i)
        {
            var _target = transform.TransformPoint(limbStance[i]) + controller.velocity * Time.deltaTime;
            var _limb = model.limbs[i];

            var distance = Vector3.ProjectOnPlane(_target - limbPositions[i], transform.up).magnitude;
            var deviation = distance - model.ranges[i];

            if (deviation > maximumDeviation)
            {
                maximumDeviation = deviation;
                limb = _limb;
                target = _target;
                index = i;
            }
        }

        return index;
    }
}

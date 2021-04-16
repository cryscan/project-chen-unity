using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClimberController : MonoBehaviour
{
    public float _speed = 3f;
    public float damp = 10;

    public LayerMask climbLayer;
    public int rayCount = 8;
    public float raysEccentricity = 0.2f;
    public float outerRaysOffset = 2f;
    public float innerRaysOffset = 25f;

    Vector3 position;
    Vector3 velocity;

    static bool GetClosestPoint(Vector3 position, Vector3 forward, Vector3 up, float halfRange, float eccentricity, float innerOffset, float outerOffset, int rayCount, LayerMask layer, out Vector3 point, out Vector3 normal)
    {
        Vector3 right = Vector3.Cross(up, forward);
        int normalCount = 1;
        int pointCount = 1;

        Vector3[] dirs = new Vector3[rayCount];
        float angularStep = 2f * Mathf.PI / (float)rayCount;
        float currentAngle = angularStep / 2f;
        for (int i = 0; i < rayCount; ++i)
        {
            dirs[i] = -up + (right * Mathf.Cos(currentAngle) + forward * Mathf.Sin(currentAngle)) * eccentricity;
            currentAngle += angularStep;
        }

        bool grounded = true;
        point = position;
        normal = up;

        foreach (Vector3 dir in dirs)
        {
            RaycastHit hit;
            Vector3 projection = Vector3.ProjectOnPlane(dir, up);
            Ray ray = new Ray(position - (dir + projection) * halfRange + projection.normalized * innerOffset / 100f, dir);
            Debug.DrawRay(ray.origin, ray.direction);

            if (Physics.SphereCast(ray, 0.01f, out hit, 2f * halfRange, layer))
            {
                point += hit.point;
                normal += hit.normal;
                normalCount += 1;
                pointCount += 1;
            }
            else grounded = false;

            ray = new Ray(position - (dir + projection) * halfRange + projection.normalized * outerOffset / 100f, dir);
            Debug.DrawRay(ray.origin, ray.direction, Color.green);

            if (Physics.SphereCast(ray, 0.01f, out hit, 2f * halfRange, layer))
            {
                point += hit.point;
                normal += hit.normal;
                normalCount += 1;
                pointCount += 1;
            }
            else grounded = false;
        }

        point /= pointCount;
        normal /= normalCount;

        return grounded;
    }

    void Start()
    {
        position = transform.position;
    }

    void Update()
    {
        Vector3 point, normal;

        // GetClosestPoint(transform.position, transform.forward, transform.up, 0.5f, 0.1f, 30, -30, 4, out point, out normal);
        // upward = normal;

        var grounded = GetClosestPoint(transform.position, transform.forward, transform.up, 0.5f, raysEccentricity, innerRaysOffset, outerRaysOffset, rayCount, climbLayer, out point, out normal);
        transform.position = transform.position.Fallout(point, damp);

        // forward = velocity.normalized;
        Quaternion q = Quaternion.LookRotation(transform.forward, normal);
        transform.rotation = transform.rotation.Fallout(q, damp);

        if (grounded)
        {
            float valueY = Input.GetAxis("Vertical");
            if (valueY != 0) transform.position += transform.forward * valueY * _speed * Time.deltaTime;

            float valueX = Input.GetAxis("Horizontal");
            if (valueX != 0) transform.position += transform.right * valueX * _speed * Time.deltaTime;

            velocity = (transform.position - position) / Time.deltaTime;
            position = transform.position;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
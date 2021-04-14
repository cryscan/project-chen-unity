using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    Vector3[] stance;

    void Start()
    {
        stance = model.limbs.Select(x => x.localPosition).ToArray();
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
}

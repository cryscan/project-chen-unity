using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hopper : MonoBehaviour
{
    [SerializeField] Transform body, endEffector, target;
    [SerializeField] float duration = 2;

    int session;
    float timer = 0;

    void Awake()
    {
        session = HopperBackend.CreateSession(duration);
    }

    void Start()
    {
        SetInitialStates();
        SetFinalStates();
        HopperBackend.StartOptimization(session);
    }

    void Update()
    {
        GetStates();
    }

    void OnDestroy()
    {
        HopperBackend.EndSession(session);
    }

    void SetInitialStates()
    {
        HopperBackend.SetInitialBaseLinearPosition(session, body.position);
        HopperBackend.SetInitialBaseAngularPosition(session, transform.rotation.eulerAngles);
        HopperBackend.SetInitialEEPosition(session, 0, endEffector.position);
    }

    void SetFinalStates()
    {
        HopperBackend.SetFinalBaseLinearPosition(session, target.position);
        HopperBackend.SetFinalBaseAngularPosition(session, transform.rotation.eulerAngles);
    }

    void GetStates()
    {
        if (timer >= duration) return;

        Vector3 baseLinear, baseAngular, eeMotion, eeForce;
        bool contact;
        if (HopperBackend.GetSolution(session, timer, out baseLinear, out baseAngular, out eeMotion, out eeForce, out contact))
        {
            body.position = baseLinear;
            body.rotation = Quaternion.Euler(baseAngular);
            endEffector.position = eeMotion;

            timer += Time.deltaTime;
        }
    }
}
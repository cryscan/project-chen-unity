using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Hopper))]
public class DisplayStatus : MonoBehaviour
{
    [SerializeField] Text statusText;
    [SerializeField] Text optimizeGaitsText;

    Hopper hopper;

    void Awake()
    {
        hopper = GetComponent<Hopper>();
    }

    void Update()
    {
        if (hopper)
        {
            if (!hopper.session.optimized) statusText.text = "Ready";
            else if (hopper.session.ready) statusText.text = "Optimized";
            else statusText.text = "Optimizing...";

            if (hopper.optimizeGaits) optimizeGaitsText.text = "Optimize Gaits: On";
            else optimizeGaitsText.text = "Optimize Gaits: Off";
        }
    }
}

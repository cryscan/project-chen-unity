using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Dynamatica.Unity.Components.Dynamatica))]
public class DisplayStatus : MonoBehaviour
{
    [SerializeField] Text statusText;
    [SerializeField] Text optimizeGaitsText;

    Dynamatica.Unity.Components.Dynamatica dynamatica;

    void Awake()
    {
        dynamatica = GetComponent<Dynamatica.Unity.Components.Dynamatica>();
    }

    void Update()
    {
        if (!dynamatica) return;

        if (!dynamatica.session.dirty) statusText.text = "Ready";
        else if (dynamatica.session.ready) statusText.text = "Optimized";
        else statusText.text = "Optimizing...";
    }
}

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathSelection : MonoBehaviour
{
    Dynamatica.Unity.Components.Dynamatica dynamatica;
    Dynamatica.Unity.Components.Path[] paths;
    string[] names;

    Dynamatica.Unity.Components.Path path;
    int index = 0;

    void Awake()
    {
        dynamatica = FindObjectOfType<Dynamatica.Unity.Components.Dynamatica>();
        paths = FindObjectsOfType<Dynamatica.Unity.Components.Path>();
        names = paths.Select(x => x.name).ToArray();
    }

    void Update()
    {
        path = paths[index];
        dynamatica.path = path;

        foreach (var path in paths) path.gameObject.SetActive(path == this.path);
    }

    void OnGUI()
    {
        GUILayout.Space(120);
        index = GUILayout.SelectionGrid(index, names, 4);
    }
}

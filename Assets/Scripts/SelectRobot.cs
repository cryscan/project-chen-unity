using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectRobot : MonoBehaviour
{
    [SerializeField] Hopper hopper;
    string[] scenes = { "Monoped", "Biped", "HyQ", "ANYmal" };

    void Start()
    {
        // hopper.SetRobot(GameManager.instance.robot);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SceneManager.LoadScene(scenes[0]);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SceneManager.LoadScene(scenes[1]);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            SceneManager.LoadScene(scenes[2]);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            SceneManager.LoadScene(scenes[3]);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectRobot : MonoBehaviour
{
    [SerializeField] Hopper hopper;
    string sceneName;

    void Awake()
    {
        sceneName = SceneManager.GetActiveScene().name;
    }

    void Start()
    {
        hopper.SetRobot(GameManager.instance.robot);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GameManager.instance.robot = HopperAPI.Robot.Monoped;
            SceneManager.LoadScene(sceneName);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GameManager.instance.robot = HopperAPI.Robot.Biped;
            SceneManager.LoadScene(sceneName);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            GameManager.instance.robot = HopperAPI.Robot.Hyq;
            SceneManager.LoadScene(sceneName);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            GameManager.instance.robot = HopperAPI.Robot.Anymal;
            SceneManager.LoadScene(sceneName);
        }
    }
}

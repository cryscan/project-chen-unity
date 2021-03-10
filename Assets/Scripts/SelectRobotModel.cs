using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectRobotModel : MonoBehaviour
{
    [SerializeField] RobotModelObject robotModelObject;

    string sceneName;

    void Awake()
    {
        sceneName = SceneManager.GetActiveScene().name;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            robotModelObject.model = HopperAPI.RobotModel.Monoped;
            SceneManager.LoadScene(sceneName);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            robotModelObject.model = HopperAPI.RobotModel.Biped;
            SceneManager.LoadScene(sceneName);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            robotModelObject.model = HopperAPI.RobotModel.Hyq;
            SceneManager.LoadScene(sceneName);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            robotModelObject.model = HopperAPI.RobotModel.Anymal;
            SceneManager.LoadScene(sceneName);
        }
    }
}

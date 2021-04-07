using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [SerializeField] float timeScale = 1;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this) Destroy(gameObject);
    }

    void Update()
    {
        Time.timeScale = timeScale;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SceneManager.LoadScene(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SceneManager.LoadScene(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            SceneManager.LoadScene(2);

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
}

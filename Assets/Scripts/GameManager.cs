using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

using Unity.Mathematics;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [SerializeField] float timeScale = 1;

    AffineTransform[] checkpoints;
    [SerializeField] int currentCheckpoint = 0;

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

        if (Input.GetKeyDown(KeyCode.R))
            Reload();

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    public void SetCheckpoints(Transform[] transforms)
    {
        checkpoints = transforms.Select(x => new AffineTransform(x.position, x.rotation)).ToArray();
    }

    public void SetCurrentCheckpoint(int index)
    {
        currentCheckpoint = index;
    }

    public AffineTransform GetCurrentCheckpoint()
    {
        if (checkpoints == null || checkpoints.Length == 0) return new AffineTransform();
        return checkpoints[currentCheckpoint];
    }

    public void Reload()
    {
        var index = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(index);
    }
}

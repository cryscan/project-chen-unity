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

    List<AffineTransform> checkpoints = new List<AffineTransform>();
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

    public void ClearCheckpoints() => checkpoints.Clear();

    public void AddCheckpoint(Checkpoint checkpoint)
    {
        var position = checkpoint.transform.position;
        var rotation = checkpoint.transform.rotation;
        checkpoints.Add(new AffineTransform(position, rotation));
    }

    public void SetCurrentCheckpoint(int index)
    {
        currentCheckpoint = index;
    }

    public void GetCurrentCheckpoint(out Vector3 position, out Quaternion rotation)
    {
        position = new Vector3();
        rotation = new Quaternion();

        if (checkpoints.Count == 0) return;

        var checkpoint = checkpoints[currentCheckpoint];
        position = checkpoint.t;
        rotation = checkpoint.q;
    }

    public void Reload()
    {
        var index = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(index);
    }
}

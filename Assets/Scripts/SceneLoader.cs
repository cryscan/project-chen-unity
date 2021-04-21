using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public string sceneName;
    public Animator fader;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            StartCoroutine(LoadCoroutine());
        }
    }

    IEnumerator LoadCoroutine()
    {
        if (fader) fader.SetTrigger("Fade Out");
        yield return new WaitForSeconds(3);
        GameManager.instance.Load(sceneName);
    }
}

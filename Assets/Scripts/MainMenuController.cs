using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void Load(string sceneName) => GameManager.instance.Load(sceneName);
    public void Quit() => Application.Quit();
}

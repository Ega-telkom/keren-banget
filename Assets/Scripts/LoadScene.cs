using UnityEngine;

public class LoadScene : MonoBehaviour
{
    public string sceneName;

    public void Load() => GameManager.instance.LoadScene(sceneName);
    public void Quit() => GameManager.instance.QuitGame();
}
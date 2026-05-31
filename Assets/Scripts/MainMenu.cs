using UnityEngine;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        GameManager.instance.inputReader.SetUI();
    }

    public void LoadTutorial() => GameManager.instance.LoadScene("Tutorial");
    public void LoadLevel(string levelName) => GameManager.instance.LoadScene(levelName);
    public void QuitGame() => Application.Quit();
}   
using UnityEngine;

public class InputModeManager : MonoBehaviour
{
    public InputReader inputReader;

    public void SetGameplay() => inputReader.SetGameplay();
    public void SetUI() => inputReader.SetUI();
}
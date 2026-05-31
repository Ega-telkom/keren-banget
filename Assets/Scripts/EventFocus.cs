using UnityEngine;
using UnityEngine.EventSystems;

public class EventFocus : MonoBehaviour
{
    public GameObject defaultButton;

    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(defaultButton);
    }
}
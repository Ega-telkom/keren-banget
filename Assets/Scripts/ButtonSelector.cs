using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ButtonSelector : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    void OnEnable()
    {
        if (EventSystem.current.currentSelectedGameObject == gameObject && IsGamepad())
            indicator.SetActive(true);
    }
    
    public GameObject indicator;

    bool IsGamepad() => Gamepad.current != null;

    public void OnSelect(BaseEventData e)
    {
        if (IsGamepad()) indicator.SetActive(true);
    }

    public void OnDeselect(BaseEventData e) => indicator.SetActive(false);

    public void OnPointerEnter(PointerEventData e)
    {
        indicator.SetActive(false);
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OnPointerExit(PointerEventData e) => indicator.SetActive(false);
}
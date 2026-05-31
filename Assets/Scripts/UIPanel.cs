using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIPanel : MonoBehaviour
{
    public InputReader inputReader;
    public GameObject defaultSelected;
    public GameObject previousSelected;

    void OnEnable()
    {
        if (inputReader == null)
            inputReader = GameManager.instance.inputReader;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(defaultSelected);
        StartCoroutine(EnableNextFrame());
        Debug.Log($"{gameObject.name} subscribed Cancel");
    }

    IEnumerator EnableNextFrame()
    {
        yield return null;
        inputReader.OnCancelPerformed += Cancel;
        Debug.Log($"{gameObject.name} subscribed Cancel");
    }

    void OnDisable()
    {
        inputReader.OnCancelPerformed -= Cancel;
        Debug.Log($"{gameObject.name} subscribed Cancel");
    }

    public void Open(GameObject prevSelected)
    {
        previousSelected = prevSelected;
        gameObject.SetActive(true);
    }
    
    public void OpenFromCurrentSelected()
    {
        Open(EventSystem.current.currentSelectedGameObject);
    }

    public void Cancel()
    {
        gameObject.SetActive(false);
        if (previousSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(previousSelected);
        }
    }
}
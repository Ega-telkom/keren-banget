using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Health health;
    public Image fillImage;

    void OnEnable() => health.OnHealthChanged += UpdateBar;
    void OnDisable() => health.OnHealthChanged -= UpdateBar;

    void UpdateBar(float normalized) => fillImage.fillAmount = normalized;
}
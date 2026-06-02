using UnityEngine;

public class LevelTrigger : MonoBehaviour
{
    private bool _triggered = false; // Pengaman lokal tingkat objek

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_triggered) return;

        if (collision.CompareTag("Player"))
        {
            _triggered = true; // Kunci pemicu
            Debug.Log("[LevelTrigger] Player menyentuh portal finish. Memanggil LoadNextLevel.");
            
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadNextLevel();
            }
        }
    }
}
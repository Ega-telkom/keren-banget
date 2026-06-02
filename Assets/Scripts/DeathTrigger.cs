using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Cari apakah objek yang menabrak area maut ini punya komponen PlayerController
        PlayerController player = collision.GetComponent<PlayerController>();

        // Jika ketemu (berarti yang menabrak fix si Player, bukan balok TNT jatuh)
        if (player != null)
        {
            Debug.Log("<color=red>[DeathTrigger]</color> Player menyentuh area maut! Memanggil OnPlayerDeath...");
            
            // LANGSUNG PANGGIL FUNGSI MATI DI PLAYER
            player.OnPlayerDeath();
        }
    }
}
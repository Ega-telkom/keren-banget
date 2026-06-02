using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameInitializer
{
    // Canggihnya Unity: Atribut ini membuat fungsi berjalan otomatis SEBELUM scene apa pun di-play
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitPlaytestSystem()
    {
        // Jika kita mulai dari scene Boot, biarkan alur normal yang berjalan
        if (SceneManager.GetActiveScene().name == "Boot") return;
        
        // Jika GameManager belum instansiasi (berarti kita langsung play dari scene Level)
        if (GameManager.instance == null)
        {
            // Ambil prefab _GameSystem dari folder Assets/Resources/
            GameObject prefab = Resources.Load<GameObject>("_GameSystem");
            if (prefab != null)
            {
                Object.Instantiate(prefab);
                Debug.Log("<color=green>[GameInitializer]</color> Sukses memuat sistem inti otomatis untuk playtesting!");
            }
            else
            {
                Debug.LogError("<color=red>[GameInitializer]</color> Gagal menemukan prefab '_GameSystem' di folder Resources! Pastikan folder dan nama prefab sudah benar.");
            }
        }
    }
}
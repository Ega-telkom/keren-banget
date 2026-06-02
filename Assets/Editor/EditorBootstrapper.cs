using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class EditorBootstrapper
{
    static EditorBootstrapper()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        // KUNCI PERBAIKAN: Hanya jalankan logika JIKA game BELUM benar-benar masuk Play Mode
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // Pastikan tidak mengunci database editing sebelum play mode
            AssetDatabase.StopAssetEditing(); 
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }
    }
}
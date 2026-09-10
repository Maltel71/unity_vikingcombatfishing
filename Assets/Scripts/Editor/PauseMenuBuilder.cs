using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class PauseMenuBuilder
{
    const string MenuPath = "Tools/Ragnar/Build Pause Menu";

    [MenuItem(MenuPath)]
    public static void CreateInScene()
    {
        PauseMenu existing = Object.FindFirstObjectByType<PauseMenu>();

        if (existing != null && existing.menuRoot != null)
        {
            bool replace = EditorUtility.DisplayDialog(
                "Pause menu already exists",
                "A built pause menu is already in the scene. Rebuild it from scratch?\n\n" +
                "Any styling you have done on it will be lost.",
                "Rebuild", "Cancel");

            if (!replace)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing.gameObject);
                return;
            }

            Undo.DestroyObjectImmediate(existing.menuRoot);
            existing.menuRoot = null;
        }

        PauseMenu menu = existing;

        if (menu == null)
        {
            GameObject go = new GameObject("PauseMenu");
            Undo.RegisterCreatedObjectUndo(go, "Create Pause Menu");
            menu = go.AddComponent<PauseMenu>();
        }

        menu.BuildInto(menu.transform);

        if (menu.menuRoot != null)
        {
            Undo.RegisterCreatedObjectUndo(menu.menuRoot, "Create Pause Menu");
            menu.menuRoot.SetActive(false);
        }

        EditorUtility.SetDirty(menu);
        EditorSceneManager.MarkSceneDirty(menu.gameObject.scene);

        Selection.activeGameObject = menu.gameObject;
        EditorGUIUtility.PingObject(menu.gameObject);

        Debug.Log("Pause menu added to the scene. Open PauseMenu > PauseMenuCanvas in the Hierarchy " +
                  "to adjust the panels. Remember to save the scene.");
    }
}

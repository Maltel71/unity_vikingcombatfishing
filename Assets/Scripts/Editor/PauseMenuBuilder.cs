using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editorverktyg som lagger ut pausmenyn som riktiga GameObjects i scenen,
/// sa den gar att flytta och styla om i Hierarchy istallet for att bara byggas
/// i kod vid start.
///
/// Ligger i en Editor-mapp och foljer darfor inte med i nagon build.
/// </summary>
public static class PauseMenuBuilder
{
    const string MenuPath = "Tools/Ragnar/Skapa pausmeny i scenen";

    [MenuItem(MenuPath)]
    public static void CreateInScene()
    {
        PauseMenu existing = Object.FindFirstObjectByType<PauseMenu>();

        if (existing != null && existing.menuRoot != null)
        {
            bool replace = EditorUtility.DisplayDialog(
                "Pausmenyn finns redan",
                "Det ligger redan en utbyggd pausmeny i scenen. Vill du bygga om den fran grunden?\n\n" +
                "All egen styling du gjort pa den forsvinner.",
                "Bygg om", "Avbryt");

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
            Undo.RegisterCreatedObjectUndo(go, "Skapa pausmeny");
            menu = go.AddComponent<PauseMenu>();
        }

        menu.BuildInto(menu.transform);

        // Menyn ska ligga gomd tills spelaren trycker Escape
        if (menu.menuRoot != null)
        {
            Undo.RegisterCreatedObjectUndo(menu.menuRoot, "Skapa pausmeny");
            menu.menuRoot.SetActive(false);
        }

        EditorUtility.SetDirty(menu);
        EditorSceneManager.MarkSceneDirty(menu.gameObject.scene);

        Selection.activeGameObject = menu.gameObject;
        EditorGUIUtility.PingObject(menu.gameObject);

        Debug.Log("Pausmenyn ar utlagd i scenen. Oppna PauseMenu > PauseMenuCanvas i Hierarchy " +
                  "for att justera panelerna. Glom inte att spara scenen.");
    }
}

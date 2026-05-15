using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Unity2019Mcp.Commands
{
    public static class SceneCommands
    {
        public static object GetActive(Dictionary<string, object> parameters)
        {
            var scene = SceneManager.GetActiveScene();
            return new
            {
                name = scene.name,
                path = scene.path,
                isDirty = scene.isDirty,
                isLoaded = scene.isLoaded,
                rootCount = scene.rootCount
            };
        }

        public static object Save(Dictionary<string, object> parameters)
        {
            var scene = SceneManager.GetActiveScene();
            var ok = EditorSceneManager.SaveScene(scene);
            return new { saved = ok, path = scene.path, name = scene.name };
        }
    }
}

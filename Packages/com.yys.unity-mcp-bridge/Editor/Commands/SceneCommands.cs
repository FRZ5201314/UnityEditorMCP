using System.Collections.Generic;
using System.IO;
using UnityMcp.Models;
using UnityMcp.Utils;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace UnityMcp.Commands
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

        public static object New(Dictionary<string, object> parameters)
        {
            var setup = ParamUtil.Get(parameters, "setup", "DefaultGameObjects");
            var mode = ParamUtil.Get(parameters, "mode", "Single");
            var scene = EditorSceneManager.NewScene(ParseNewSceneSetup(setup), ParseNewSceneMode(mode));
            return ToSceneDto(scene);
        }

        public static object Open(Dictionary<string, object> parameters)
        {
            var path = ParamUtil.RequiredString(parameters, "path");
            EnsureSceneAssetPath(path);
            var mode = ParamUtil.Get(parameters, "mode", "Single");
            var scene = EditorSceneManager.OpenScene(path, ParseOpenSceneMode(mode));
            return ToSceneDto(scene);
        }

        public static object SaveAs(Dictionary<string, object> parameters)
        {
            var path = ParamUtil.RequiredString(parameters, "path");
            EnsureSceneAssetPath(path);
            EnsureAssetDirectory(path);
            var scene = SceneManager.GetActiveScene();
            var ok = EditorSceneManager.SaveScene(scene, path);
            return new { saved = ok, path = scene.path, name = scene.name };
        }

        public static object GetDirty(Dictionary<string, object> parameters)
        {
            var scene = SceneManager.GetActiveScene();
            return new { isDirty = scene.isDirty, path = scene.path, name = scene.name };
        }

        private static object ToSceneDto(Scene scene)
        {
            return new
            {
                name = scene.name,
                path = scene.path,
                isDirty = scene.isDirty,
                isLoaded = scene.isLoaded,
                rootCount = scene.rootCount
            };
        }

        private static NewSceneSetup ParseNewSceneSetup(string value)
        {
            return value == "EmptyScene" ? NewSceneSetup.EmptyScene : NewSceneSetup.DefaultGameObjects;
        }

        private static NewSceneMode ParseNewSceneMode(string value)
        {
            return value == "Additive" ? NewSceneMode.Additive : NewSceneMode.Single;
        }

        private static OpenSceneMode ParseOpenSceneMode(string value)
        {
            return value == "Additive" ? OpenSceneMode.Additive : OpenSceneMode.Single;
        }

        private static void EnsureSceneAssetPath(string path)
        {
            if (!path.StartsWith("Assets/") || !path.EndsWith(".unity"))
            {
                throw new McpCommandException("INVALID_PARAMS", "Scene path must be under Assets and end with .unity: " + path, null);
            }
        }

        private static void EnsureAssetDirectory(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}

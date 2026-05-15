using System.Collections.Generic;
using System.IO;
using Unity2019Mcp.Models;
using Unity2019Mcp.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Unity2019Mcp.Commands
{
    public static class PrefabCommands
    {
        public static object Create(Dictionary<string, object> parameters)
        {
            var path = ParamUtil.RequiredString(parameters, "path");
            var assetPath = ParamUtil.RequiredString(parameters, "assetPath");
            EnsurePrefabAssetPath(assetPath);
            EnsureAssetDirectory(assetPath);

            var go = FindRequiredGameObject(path);
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, assetPath);
            if (prefab == null)
            {
                throw new McpCommandException("OPERATION_FAILED", "Failed to create prefab: " + assetPath, null);
            }

            AssetDatabase.ImportAsset(assetPath);
            return new { assetPath = assetPath, gameObject = DtoUtil.ToGameObjectDto(go) };
        }

        public static object Instantiate(Dictionary<string, object> parameters)
        {
            var assetPath = ParamUtil.RequiredString(parameters, "assetPath");
            var parentPath = ParamUtil.Get<string>(parameters, "parentPath", null);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                throw new McpCommandException("ASSET_NOT_FOUND", "Prefab asset not found: " + assetPath, null);
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (instance == null)
            {
                throw new McpCommandException("OPERATION_FAILED", "Failed to instantiate prefab: " + assetPath, null);
            }

            Undo.RegisterCreatedObjectUndo(instance, "MCP Instantiate Prefab");
            if (!string.IsNullOrEmpty(parentPath))
            {
                var parent = GameObjectPathUtil.Find(parentPath);
                if (parent == null)
                {
                    Object.DestroyImmediate(instance);
                    throw new KeyNotFoundException("Parent GameObject not found: " + parentPath);
                }

                instance.transform.SetParent(parent.transform);
            }

            EditorSceneManager.MarkSceneDirty(instance.scene);
            Selection.activeGameObject = instance;
            return DtoUtil.ToGameObjectDto(instance);
        }

        public static object Apply(Dictionary<string, object> parameters)
        {
            var path = ParamUtil.RequiredString(parameters, "path");
            var go = FindRequiredGameObject(path);
            var prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
            if (prefabRoot == null)
            {
                throw new McpCommandException("INVALID_PARAMS", "GameObject is not part of a prefab instance: " + path, null);
            }

            var assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefabRoot);
            if (string.IsNullOrEmpty(assetPath))
            {
                throw new McpCommandException("ASSET_NOT_FOUND", "Prefab asset path not found for instance: " + path, null);
            }

            PrefabUtility.ApplyPrefabInstance(prefabRoot, InteractionMode.AutomatedAction);
            AssetDatabase.SaveAssets();
            return new { applied = true, assetPath = assetPath, gameObject = DtoUtil.ToGameObjectDto(prefabRoot) };
        }

        private static GameObject FindRequiredGameObject(string path)
        {
            var go = GameObjectPathUtil.Find(path);
            if (go == null)
            {
                throw new KeyNotFoundException("GameObject not found: " + path);
            }

            return go;
        }

        private static void EnsurePrefabAssetPath(string assetPath)
        {
            if (!assetPath.StartsWith("Assets/") || !assetPath.EndsWith(".prefab"))
            {
                throw new McpCommandException("INVALID_PARAMS", "Prefab path must be under Assets and end with .prefab: " + assetPath, null);
            }
        }

        private static void EnsureAssetDirectory(string assetPath)
        {
            var directory = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }
        }
    }
}

using System.Collections.Generic;
using UnityMcp.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnityMcp.Commands
{
    public static class GameObjectCommands
    {
        public static object Create(Dictionary<string, object> parameters)
        {
            var name = ParamUtil.Get(parameters, "name", "GameObject");
            var parentPath = ParamUtil.Get<string>(parameters, "parentPath", null);
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "MCP Create GameObject");

            if (!string.IsNullOrEmpty(parentPath))
            {
                var parent = GameObjectPathUtil.Find(parentPath);
                if (parent == null)
                {
                    Object.DestroyImmediate(go);
                    throw new KeyNotFoundException("Parent GameObject not found: " + parentPath);
                }

                go.transform.SetParent(parent.transform);
            }

            EditorSceneManager.MarkSceneDirty(go.scene);
            Selection.activeGameObject = go;
            return DtoUtil.ToGameObjectDto(go);
        }

        public static object Delete(Dictionary<string, object> parameters)
        {
            var path = ParamUtil.RequiredString(parameters, "path");
            var go = GameObjectPathUtil.Find(path);
            if (go == null)
            {
                throw new KeyNotFoundException("GameObject not found: " + path);
            }

            var dto = DtoUtil.ToGameObjectDto(go);
            Undo.DestroyObjectImmediate(go);
            return new { deleted = true, gameObject = dto };
        }

        public static object Rename(Dictionary<string, object> parameters)
        {
            var path = ParamUtil.RequiredString(parameters, "path");
            var name = ParamUtil.RequiredString(parameters, "name");
            var go = GameObjectPathUtil.Find(path);
            if (go == null)
            {
                throw new KeyNotFoundException("GameObject not found: " + path);
            }

            Undo.RecordObject(go, "MCP Rename GameObject");
            go.name = name;
            EditorUtility.SetDirty(go);
            EditorSceneManager.MarkSceneDirty(go.scene);
            return DtoUtil.ToGameObjectDto(go);
        }

        public static object Find(Dictionary<string, object> parameters)
        {
            var path = ParamUtil.RequiredString(parameters, "path");
            var go = GameObjectPathUtil.Find(path);
            if (go == null)
            {
                throw new KeyNotFoundException("GameObject not found: " + path);
            }

            return DtoUtil.ToGameObjectDto(go);
        }
    }
}

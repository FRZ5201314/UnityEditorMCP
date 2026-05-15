using System;
using System.Collections.Generic;
using Unity2019Mcp.Models;
using Unity2019Mcp.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Unity2019Mcp.Commands
{
    public static class ComponentCommands
    {
        public static object List(Dictionary<string, object> parameters)
        {
            var go = FindRequiredGameObject(parameters);
            var components = new List<ComponentDto>();
            foreach (var component in go.GetComponents<Component>())
            {
                if (component == null)
                {
                    continue;
                }

                var type = component.GetType();
                components.Add(new ComponentDto
                {
                    instanceId = component.GetInstanceID(),
                    typeName = type.Name,
                    fullTypeName = type.FullName,
                    assemblyName = type.Assembly.GetName().Name
                });
            }

            return new { components = components };
        }

        public static object Add(Dictionary<string, object> parameters)
        {
            var go = FindRequiredGameObject(parameters);
            var typeName = ParamUtil.RequiredString(parameters, "typeName");
            List<string> candidates;
            var type = TypeResolver.ResolveComponentType(typeName, out candidates);
            if (type == null)
            {
                if (candidates.Count > 1)
                {
                    throw new AmbiguousMatchException("Component type is ambiguous: " + typeName + "|" + string.Join(";", candidates.ToArray()));
                }

                throw new TypeLoadException("Component type not found: " + typeName);
            }

            Undo.AddComponent(go, type);
            EditorSceneManager.MarkSceneDirty(go.scene);
            return List(parameters);
        }

        public static object Remove(Dictionary<string, object> parameters)
        {
            var go = FindRequiredGameObject(parameters);
            var typeName = ParamUtil.RequiredString(parameters, "typeName");
            var component = FindComponent(go, typeName);
            if (component == null)
            {
                throw new KeyNotFoundException("Component not found: " + typeName);
            }

            Undo.DestroyObjectImmediate(component);
            EditorSceneManager.MarkSceneDirty(go.scene);
            return new { removed = true, typeName = typeName };
        }

        public static object Get(Dictionary<string, object> parameters)
        {
            var go = FindRequiredGameObject(parameters);
            var typeName = ParamUtil.RequiredString(parameters, "typeName");
            var component = FindComponent(go, typeName);
            if (component == null)
            {
                throw new KeyNotFoundException("Component not found: " + typeName);
            }

            var type = component.GetType();
            return new ComponentDto
            {
                instanceId = component.GetInstanceID(),
                typeName = type.Name,
                fullTypeName = type.FullName,
                assemblyName = type.Assembly.GetName().Name
            };
        }

        private static GameObject FindRequiredGameObject(Dictionary<string, object> parameters)
        {
            var path = ParamUtil.RequiredString(parameters, "path");
            var go = GameObjectPathUtil.Find(path);
            if (go == null)
            {
                throw new KeyNotFoundException("GameObject not found: " + path);
            }

            return go;
        }

        private static Component FindComponent(GameObject go, string typeName)
        {
            foreach (var component in go.GetComponents<Component>())
            {
                if (component == null)
                {
                    continue;
                }

                var type = component.GetType();
                if (type.Name == typeName || type.FullName == typeName)
                {
                    return component;
                }
            }

            return null;
        }
    }
}

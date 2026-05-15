using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
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

        public static object GetProperty(Dictionary<string, object> parameters)
        {
            var component = FindRequiredComponent(parameters);
            var propertyPath = ParamUtil.RequiredString(parameters, "propertyPath");
            var serializedObject = new SerializedObject(component);
            var property = serializedObject.FindProperty(propertyPath);
            if (property == null)
            {
                throw new McpCommandException("PROPERTY_NOT_FOUND", "SerializedProperty not found: " + propertyPath, null);
            }

            return ToPropertyDto(property);
        }

        public static object SetProperty(Dictionary<string, object> parameters)
        {
            var component = FindRequiredComponent(parameters);
            var propertyPath = ParamUtil.RequiredString(parameters, "propertyPath");
            var serializedObject = new SerializedObject(component);
            var property = serializedObject.FindProperty(propertyPath);
            if (property == null)
            {
                throw new McpCommandException("PROPERTY_NOT_FOUND", "SerializedProperty not found: " + propertyPath, null);
            }

            serializedObject.Update();
            Undo.RecordObject(component, "MCP Set Component Property");
            SetPropertyValue(property, parameters);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(component);
            var scene = component.gameObject.scene;
            if (scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            return ToPropertyDto(property);
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

        private static Component FindRequiredComponent(Dictionary<string, object> parameters)
        {
            var go = FindRequiredGameObject(parameters);
            var typeName = ParamUtil.RequiredString(parameters, "typeName");
            var component = FindComponent(go, typeName);
            if (component == null)
            {
                throw new KeyNotFoundException("Component not found: " + typeName);
            }

            return component;
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

        private static object ToPropertyDto(SerializedProperty property)
        {
            return new
            {
                propertyPath = property.propertyPath,
                displayName = property.displayName,
                propertyType = property.propertyType.ToString(),
                value = GetPropertyValue(property),
                objectReferenceAssetPath = property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null
                    ? AssetDatabase.GetAssetPath(property.objectReferenceValue)
                    : null
            };
        }

        private static object GetPropertyValue(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:
                    return property.intValue;
                case SerializedPropertyType.Boolean:
                    return property.boolValue;
                case SerializedPropertyType.Float:
                    return property.floatValue;
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Color:
                    return new { r = property.colorValue.r, g = property.colorValue.g, b = property.colorValue.b, a = property.colorValue.a };
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue == null ? null : new { name = property.objectReferenceValue.name, typeName = property.objectReferenceValue.GetType().Name };
                case SerializedPropertyType.Enum:
                    return new { index = property.enumValueIndex, name = property.enumDisplayNames[property.enumValueIndex] };
                case SerializedPropertyType.Vector2:
                    return new { x = property.vector2Value.x, y = property.vector2Value.y };
                case SerializedPropertyType.Vector3:
                    return new { x = property.vector3Value.x, y = property.vector3Value.y, z = property.vector3Value.z };
                default:
                    return null;
            }
        }

        private static void SetPropertyValue(SerializedProperty property, Dictionary<string, object> parameters)
        {
            var value = ParamUtil.Get<object>(parameters, "value", null);
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:
                    property.intValue = Convert.ToInt32(UnwrapJToken(value));
                    return;
                case SerializedPropertyType.Boolean:
                    property.boolValue = Convert.ToBoolean(UnwrapJToken(value));
                    return;
                case SerializedPropertyType.Float:
                    property.floatValue = Convert.ToSingle(UnwrapJToken(value));
                    return;
                case SerializedPropertyType.String:
                    property.stringValue = value == null ? null : Convert.ToString(UnwrapJToken(value));
                    return;
                case SerializedPropertyType.Color:
                    property.colorValue = ReadColor(value);
                    return;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = ReadObjectReference(parameters);
                    return;
                case SerializedPropertyType.Enum:
                    SetEnumValue(property, value);
                    return;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = ReadVector2(value);
                    return;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = ReadVector3(value);
                    return;
                default:
                    throw new McpCommandException("UNSUPPORTED_PROPERTY_TYPE", "Unsupported SerializedProperty type: " + property.propertyType, new { propertyPath = property.propertyPath });
            }
        }

        private static object UnwrapJToken(object value)
        {
            var token = value as JToken;
            return token == null ? value : token.ToObject<object>();
        }

        private static Vector2 ReadVector2(object value)
        {
            return new Vector2(ReadFloatField(value, "x"), ReadFloatField(value, "y"));
        }

        private static Vector3 ReadVector3(object value)
        {
            return new Vector3(ReadFloatField(value, "x"), ReadFloatField(value, "y"), ReadFloatField(value, "z"));
        }

        private static Color ReadColor(object value)
        {
            return new Color(
                ReadFloatField(value, "r"),
                ReadFloatField(value, "g"),
                ReadFloatField(value, "b"),
                HasField(value, "a") ? ReadFloatField(value, "a") : 1f);
        }

        private static UnityEngine.Object ReadObjectReference(Dictionary<string, object> parameters)
        {
            var assetPath = ParamUtil.Get<string>(parameters, "objectReferenceAssetPath", null);
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset == null)
            {
                throw new McpCommandException("ASSET_NOT_FOUND", "Object reference asset not found: " + assetPath, null);
            }

            return asset;
        }

        private static void SetEnumValue(SerializedProperty property, object value)
        {
            var unwrapped = UnwrapJToken(value);
            if (unwrapped is string)
            {
                var text = (string)unwrapped;
                for (var i = 0; i < property.enumDisplayNames.Length; i++)
                {
                    if (property.enumDisplayNames[i] == text || property.enumNames[i] == text)
                    {
                        property.enumValueIndex = i;
                        return;
                    }
                }

                throw new McpCommandException("INVALID_PARAMS", "Enum value not found: " + text, property.enumDisplayNames);
            }

            property.enumValueIndex = Convert.ToInt32(unwrapped);
        }

        private static float ReadFloatField(object value, string field)
        {
            var token = value as JToken;
            if (token != null)
            {
                var child = token[field];
                if (child != null)
                {
                    return child.Value<float>();
                }
            }

            var dictionary = value as IDictionary;
            if (dictionary != null && dictionary.Contains(field))
            {
                return Convert.ToSingle(dictionary[field]);
            }

            throw new McpCommandException("INVALID_PARAMS", "Value must contain numeric field: " + field, null);
        }

        private static bool HasField(object value, string field)
        {
            var token = value as JToken;
            if (token != null)
            {
                return token[field] != null;
            }

            var dictionary = value as IDictionary;
            return dictionary != null && dictionary.Contains(field);
        }
    }
}

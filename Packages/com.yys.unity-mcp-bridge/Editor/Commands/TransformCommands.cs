using System.Collections.Generic;
using UnityMcp.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace UnityMcp.Commands
{
    public static class TransformCommands
    {
        public static object Get(Dictionary<string, object> parameters)
        {
            var path = ParamUtil.RequiredString(parameters, "path");
            var go = GameObjectPathUtil.Find(path);
            if (go == null)
            {
                throw new KeyNotFoundException("GameObject not found: " + path);
            }

            return DtoUtil.ToTransformDto(go.transform);
        }

        public static object Set(Dictionary<string, object> parameters)
        {
            var path = ParamUtil.RequiredString(parameters, "path");
            var go = GameObjectPathUtil.Find(path);
            if (go == null)
            {
                throw new KeyNotFoundException("GameObject not found: " + path);
            }

            Undo.RecordObject(go.transform, "MCP Set Transform");
            go.transform.position = DtoUtil.ToVector3(ParamUtil.Vector3(parameters, "position"), go.transform.position);
            go.transform.localPosition = DtoUtil.ToVector3(ParamUtil.Vector3(parameters, "localPosition"), go.transform.localPosition);
            go.transform.eulerAngles = DtoUtil.ToVector3(ParamUtil.Vector3(parameters, "rotation"), go.transform.eulerAngles);
            go.transform.localEulerAngles = DtoUtil.ToVector3(ParamUtil.Vector3(parameters, "localRotation"), go.transform.localEulerAngles);
            go.transform.localScale = DtoUtil.ToVector3(ParamUtil.Vector3(parameters, "scale"), go.transform.localScale);
            EditorUtility.SetDirty(go.transform);
            EditorSceneManager.MarkSceneDirty(go.scene);
            return DtoUtil.ToTransformDto(go.transform);
        }
    }
}

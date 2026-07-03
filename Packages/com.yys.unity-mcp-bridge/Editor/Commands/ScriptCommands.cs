using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityMcp.Models;
using UnityMcp.Utils;
using UnityEditor;
using UnityEditor.Compilation;

namespace UnityMcp.Commands
{
    public static class ScriptCommands
    {
        public static object Create(Dictionary<string, object> parameters)
        {
            var assetPath = ParamUtil.RequiredString(parameters, "assetPath");
            var className = ParamUtil.Get<string>(parameters, "className", null);
            var overwrite = ParamUtil.Get(parameters, "overwrite", false);
            if (!assetPath.StartsWith("Assets/") || !assetPath.EndsWith(".cs"))
            {
                throw new IOException("Script path must be under Assets and end with .cs: " + assetPath);
            }

            if (string.IsNullOrEmpty(className))
            {
                className = Path.GetFileNameWithoutExtension(assetPath);
            }

            if (!Regex.IsMatch(className, "^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                throw new IOException("Invalid C# class name: " + className);
            }

            if (File.Exists(assetPath) && !overwrite)
            {
                throw new IOException("Script already exists: " + assetPath);
            }

            var directory = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var content = ParamUtil.Get<string>(parameters, "content", null);
            if (string.IsNullOrEmpty(content))
            {
                content = "using UnityEngine;\n\npublic class " + className + " : MonoBehaviour\n{\n}\n";
            }

            File.WriteAllText(assetPath, content);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            CompilationPipeline.RequestScriptCompilation();
            EditorApplication.QueuePlayerLoopUpdate();
            return new { assetPath = assetPath, className = className, created = true, requestedCompilation = true };
        }

        public static object Attach(Dictionary<string, object> parameters)
        {
            if (EditorApplication.isCompiling)
            {
                throw new McpCommandException(
                    "UNITY_COMPILING",
                    "Unity is compiling. Retry after compilation finishes.",
                    BuildAttachFailureDetails(parameters, "unity_compiling"));
            }

            try
            {
                return ComponentCommands.Add(parameters);
            }
            catch (TypeLoadException ex)
            {
                throw new McpCommandException("SCRIPT_COMPILE_FAILED", ex.Message, BuildAttachFailureDetails(parameters, "type_not_found"));
            }
        }

        private static object BuildAttachFailureDetails(Dictionary<string, object> parameters, string reason)
        {
            return new
            {
                reason = reason,
                path = ParamUtil.Get<string>(parameters, "path", null),
                typeName = ParamUtil.Get<string>(parameters, "typeName", null),
                isUpdating = EditorApplication.isUpdating,
                isCompiling = EditorApplication.isCompiling,
                hint = "Check Unity Console compile errors and ensure the requested typeName is a non-abstract MonoBehaviour or Component type."
            };
        }
    }
}

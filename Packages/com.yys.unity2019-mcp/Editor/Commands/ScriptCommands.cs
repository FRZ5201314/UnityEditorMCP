using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Unity2019Mcp.Utils;
using UnityEditor;

namespace Unity2019Mcp.Commands
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
            AssetDatabase.Refresh();
            return new { assetPath = assetPath, className = className, created = true };
        }

        public static object Attach(Dictionary<string, object> parameters)
        {
            if (EditorApplication.isCompiling)
            {
                throw new IOException("Unity is compiling. Retry after compilation finishes.");
            }

            return ComponentCommands.Add(parameters);
        }
    }
}

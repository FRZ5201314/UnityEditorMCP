using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity2019Mcp.Commands
{
    public static class ProjectCommands
    {
        public static object GetInfo(Dictionary<string, object> parameters)
        {
            return new
            {
                unityVersion = Application.unityVersion,
                projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length),
                productName = Application.productName,
                isCompiling = EditorApplication.isCompiling,
                isPlaying = EditorApplication.isPlaying
            };
        }
    }
}

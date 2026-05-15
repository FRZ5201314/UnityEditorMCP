using System.Collections.Generic;
using Unity2019Mcp.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity2019Mcp.Commands
{
    public static class HierarchyCommands
    {
        public static object List(Dictionary<string, object> parameters)
        {
            var recursive = ParamUtil.Get(parameters, "recursive", true);
            var rootPath = ParamUtil.Get<string>(parameters, "rootPath", null);
            var results = new List<object>();

            if (!string.IsNullOrEmpty(rootPath))
            {
                var root = GameObjectPathUtil.Find(rootPath);
                if (root == null)
                {
                    throw new KeyNotFoundException("GameObject not found: " + rootPath);
                }

                AddObject(results, root, recursive);
                return new { objects = results };
            }

            var scene = SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                AddObject(results, root, recursive);
            }

            return new { objects = results };
        }

        private static void AddObject(List<object> results, GameObject go, bool recursive)
        {
            results.Add(DtoUtil.ToGameObjectDto(go));
            if (!recursive)
            {
                return;
            }

            for (var i = 0; i < go.transform.childCount; i++)
            {
                AddObject(results, go.transform.GetChild(i).gameObject, true);
            }
        }
    }
}

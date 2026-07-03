using UnityEngine;

namespace UnityMcp.Utils
{
    public static class GameObjectPathUtil
    {
        public static string GetPath(GameObject go)
        {
            if (go == null)
            {
                return null;
            }

            var path = go.name;
            var parent = go.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }

        public static GameObject Find(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return GameObject.Find(path);
        }
    }
}

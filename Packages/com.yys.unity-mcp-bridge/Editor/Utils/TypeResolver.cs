using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityMcp.Utils
{
    public static class TypeResolver
    {
        public static Type ResolveComponentType(string typeName, out List<string> candidates)
        {
            candidates = new List<string>();
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            var direct = Type.GetType(typeName);
            if (IsComponentType(direct))
            {
                return direct;
            }

            var unityEngine = typeof(Component).Assembly.GetType("UnityEngine." + typeName);
            if (IsComponentType(unityEngine))
            {
                return unityEngine;
            }

            var matches = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (!IsComponentType(type))
                    {
                        continue;
                    }

                    if (type.FullName == typeName || type.Name == typeName)
                    {
                        matches.Add(type);
                    }
                }
            }

            candidates = matches.Select(t => t.FullName + ", " + t.Assembly.GetName().Name).ToList();
            return matches.Count == 1 ? matches[0] : null;
        }

        private static bool IsComponentType(Type type)
        {
            return type != null && typeof(Component).IsAssignableFrom(type) && !type.IsAbstract;
        }
    }
}

using System.Collections.Generic;

namespace Unity2019Mcp.Models
{
    public class McpCommandRequest
    {
        public string id;
        public string command;
        public Dictionary<string, object> @params;
    }

    public class McpCommandResponse
    {
        public bool ok;
        public string id;
        public object result;
        public McpError error;

        public static McpCommandResponse Success(string id, object result)
        {
            return new McpCommandResponse { ok = true, id = id, result = result, error = null };
        }

        public static McpCommandResponse Fail(string id, string code, string message, object details)
        {
            return new McpCommandResponse
            {
                ok = false,
                id = id,
                result = null,
                error = new McpError { code = code, message = message, details = details }
            };
        }
    }

    public class McpError
    {
        public string code;
        public string message;
        public object details;
    }

    public class GameObjectDto
    {
        public int instanceId;
        public string name;
        public string path;
        public bool activeSelf;
        public string tag;
        public string layerName;
        public int childCount;
    }

    public class ComponentDto
    {
        public int instanceId;
        public string typeName;
        public string fullTypeName;
        public string assemblyName;
    }

    public class TransformDto
    {
        public Vector3Dto position;
        public Vector3Dto localPosition;
        public Vector3Dto rotation;
        public Vector3Dto localRotation;
        public Vector3Dto scale;
    }

    public class Vector3Dto
    {
        public float x;
        public float y;
        public float z;
    }
}

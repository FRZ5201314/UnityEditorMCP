using Unity2019Mcp.Models;
using UnityEngine;

namespace Unity2019Mcp.Utils
{
    public static class DtoUtil
    {
        public static GameObjectDto ToGameObjectDto(GameObject go)
        {
            if (go == null)
            {
                return null;
            }

            return new GameObjectDto
            {
                instanceId = go.GetInstanceID(),
                name = go.name,
                path = GameObjectPathUtil.GetPath(go),
                activeSelf = go.activeSelf,
                tag = go.tag,
                layerName = LayerMask.LayerToName(go.layer),
                childCount = go.transform.childCount
            };
        }

        public static TransformDto ToTransformDto(Transform transform)
        {
            return new TransformDto
            {
                position = ToVector3Dto(transform.position),
                localPosition = ToVector3Dto(transform.localPosition),
                rotation = ToVector3Dto(transform.eulerAngles),
                localRotation = ToVector3Dto(transform.localEulerAngles),
                scale = ToVector3Dto(transform.localScale)
            };
        }

        public static Vector3Dto ToVector3Dto(Vector3 value)
        {
            return new Vector3Dto { x = value.x, y = value.y, z = value.z };
        }

        public static Vector3 ToVector3(Vector3Dto value, Vector3 fallback)
        {
            if (value == null)
            {
                return fallback;
            }

            return new Vector3(value.x, value.y, value.z);
        }
    }
}

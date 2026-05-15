using System.Collections.Generic;
using Unity2019Mcp.Models;
using Unity2019Mcp.Utils;
using UnityEditor;
using UnityEngine;

namespace Unity2019Mcp.Commands
{
    public static class AssetCommands
    {
        public static object Refresh(Dictionary<string, object> parameters)
        {
            AssetDatabase.Refresh();
            return new { refreshed = true };
        }

        public static object Find(Dictionary<string, object> parameters)
        {
            var filter = ParamUtil.RequiredString(parameters, "filter");
            var folders = ParamUtil.Get<string[]>(parameters, "folders", null);
            var guids = folders == null || folders.Length == 0
                ? AssetDatabase.FindAssets(filter)
                : AssetDatabase.FindAssets(filter, folders);

            var assets = new List<object>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                assets.Add(ToAssetDto(path, guid));
            }

            return new { assets = assets };
        }

        public static object Load(Dictionary<string, object> parameters)
        {
            var assetPath = ParamUtil.RequiredString(parameters, "assetPath");
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset == null)
            {
                throw new McpCommandException("ASSET_NOT_FOUND", "Asset not found: " + assetPath, null);
            }

            return ToAssetDto(assetPath, AssetDatabase.AssetPathToGUID(assetPath));
        }

        public static object CreateFolder(Dictionary<string, object> parameters)
        {
            var parentPath = ParamUtil.Get(parameters, "parentPath", "Assets");
            var name = ParamUtil.RequiredString(parameters, "name");
            if (!parentPath.StartsWith("Assets"))
            {
                throw new McpCommandException("INVALID_PARAMS", "Folder path must be under Assets: " + parentPath, null);
            }

            if (!AssetDatabase.IsValidFolder(parentPath))
            {
                throw new McpCommandException("ASSET_NOT_FOUND", "Parent folder not found: " + parentPath, null);
            }

            var guid = AssetDatabase.CreateFolder(parentPath, name);
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return new { created = true, path = path, guid = guid };
        }

        public static object Delete(Dictionary<string, object> parameters)
        {
            var assetPath = ParamUtil.RequiredString(parameters, "assetPath");
            if (!assetPath.StartsWith("Assets/"))
            {
                throw new McpCommandException("INVALID_PARAMS", "Asset path must be under Assets: " + assetPath, null);
            }

            if (AssetDatabase.LoadAssetAtPath<Object>(assetPath) == null && !AssetDatabase.IsValidFolder(assetPath))
            {
                throw new McpCommandException("ASSET_NOT_FOUND", "Asset not found: " + assetPath, null);
            }

            var ok = AssetDatabase.DeleteAsset(assetPath);
            return new { deleted = ok, assetPath = assetPath };
        }

        private static object ToAssetDto(string path, string guid)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            return new
            {
                path = path,
                guid = guid,
                name = asset == null ? null : asset.name,
                typeName = asset == null ? null : asset.GetType().Name
            };
        }
    }
}

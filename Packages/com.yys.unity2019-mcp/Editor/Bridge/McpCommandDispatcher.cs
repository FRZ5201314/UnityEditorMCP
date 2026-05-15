using System;
using System.Collections.Generic;
using System.Reflection;
using Unity2019Mcp.Commands;
using Unity2019Mcp.Models;

namespace Unity2019Mcp.Bridge
{
    public static class McpCommandDispatcher
    {
        private static readonly Dictionary<string, Func<Dictionary<string, object>, object>> Handlers =
            new Dictionary<string, Func<Dictionary<string, object>, object>>
            {
                { "project.getInfo", ProjectCommands.GetInfo },
                { "scene.getActive", SceneCommands.GetActive },
                { "scene.save", SceneCommands.Save },
                { "hierarchy.list", HierarchyCommands.List },
                { "gameObject.create", GameObjectCommands.Create },
                { "gameObject.delete", GameObjectCommands.Delete },
                { "gameObject.rename", GameObjectCommands.Rename },
                { "gameObject.find", GameObjectCommands.Find },
                { "transform.get", TransformCommands.Get },
                { "transform.set", TransformCommands.Set },
                { "component.list", ComponentCommands.List },
                { "component.add", ComponentCommands.Add },
                { "component.remove", ComponentCommands.Remove },
                { "component.get", ComponentCommands.Get },
                { "script.create", ScriptCommands.Create },
                { "script.attach", ScriptCommands.Attach },
                { "asset.refresh", AssetCommands.Refresh }
            };

        public static McpCommandResponse Execute(McpCommandRequest request)
        {
            if (request == null)
            {
                return McpCommandResponse.Fail(null, "INVALID_PARAMS", "Request body is invalid.", null);
            }

            if (string.IsNullOrEmpty(request.command) || !Handlers.ContainsKey(request.command))
            {
                return McpCommandResponse.Fail(request.id, "COMMAND_NOT_FOUND", "Command not found: " + request.command, null);
            }

            try
            {
                var result = Handlers[request.command](request.@params ?? new Dictionary<string, object>());
                return McpCommandResponse.Success(request.id, result);
            }
            catch (ArgumentException ex)
            {
                return McpCommandResponse.Fail(request.id, "INVALID_PARAMS", ex.Message, null);
            }
            catch (KeyNotFoundException ex)
            {
                var message = ex.Message;
                var code = message.IndexOf("Component", StringComparison.OrdinalIgnoreCase) >= 0 ? "COMPONENT_NOT_FOUND" : "OBJECT_NOT_FOUND";
                return McpCommandResponse.Fail(request.id, code, message, null);
            }
            catch (TypeLoadException ex)
            {
                return McpCommandResponse.Fail(request.id, "TYPE_NOT_FOUND", ex.Message, null);
            }
            catch (AmbiguousMatchException ex)
            {
                var parts = ex.Message.Split('|');
                return McpCommandResponse.Fail(request.id, "TYPE_AMBIGUOUS", parts[0], parts.Length > 1 ? parts[1].Split(';') : null);
            }
            catch (McpCommandException ex)
            {
                return McpCommandResponse.Fail(request.id, ex.code, ex.Message, ex.details);
            }
            catch (Exception ex)
            {
                return McpCommandResponse.Fail(request.id, "OPERATION_FAILED", ex.Message, ex.ToString());
            }
        }
    }
}

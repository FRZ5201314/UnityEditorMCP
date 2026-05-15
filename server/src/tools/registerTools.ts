import type { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import type { UnityBridgeClient } from "../unity/UnityBridgeClient.js";
import { toErrorText } from "../errors.js";
import {
  componentSchema,
  emptySchema,
  gameObjectCreateSchema,
  gameObjectRenameSchema,
  hierarchyListSchema,
  pathSchema,
  scriptAttachSchema,
  scriptCreateSchema,
  transformSetSchema,
} from "./schemas.js";

type ToolSchema = Parameters<McpServer["tool"]>[2];

export function registerTools(server: McpServer, bridge: UnityBridgeClient): void {
  register(server, bridge, "unity_health", "Check whether the Unity Editor bridge is reachable.", emptySchema, async () => bridge.health());
  register(server, bridge, "unity_project_get_info", "Get Unity version and current project information.", emptySchema, async () => bridge.command("project.getInfo"));
  register(server, bridge, "unity_scene_get_active", "Get the active Unity scene.", emptySchema, async () => bridge.command("scene.getActive"));
  register(server, bridge, "unity_scene_save", "Save the active Unity scene.", emptySchema, async () => bridge.command("scene.save"));
  register(server, bridge, "unity_hierarchy_list", "List GameObjects in the active scene hierarchy.", hierarchyListSchema, async args => bridge.command("hierarchy.list", args));
  register(server, bridge, "unity_gameobject_create", "Create a GameObject in the active scene.", gameObjectCreateSchema, async args => bridge.command("gameObject.create", args));
  register(server, bridge, "unity_gameobject_delete", "Delete a GameObject by hierarchy path.", pathSchema, async args => bridge.command("gameObject.delete", args));
  register(server, bridge, "unity_gameobject_find", "Find a GameObject by hierarchy path.", pathSchema, async args => bridge.command("gameObject.find", args));
  register(server, bridge, "unity_gameobject_rename", "Rename a GameObject by hierarchy path.", gameObjectRenameSchema, async args => bridge.command("gameObject.rename", args));
  register(server, bridge, "unity_transform_get", "Get a GameObject transform.", pathSchema, async args => bridge.command("transform.get", args));
  register(server, bridge, "unity_transform_set", "Set a GameObject transform.", transformSetSchema, async args => bridge.command("transform.set", args));
  register(server, bridge, "unity_component_list", "List components on a GameObject.", pathSchema, async args => bridge.command("component.list", args));
  register(server, bridge, "unity_component_add", "Add a component to a GameObject.", componentSchema, async args => bridge.command("component.add", args));
  register(server, bridge, "unity_component_remove", "Remove a component from a GameObject.", componentSchema, async args => bridge.command("component.remove", args));
  register(server, bridge, "unity_component_get", "Get a component on a GameObject.", componentSchema, async args => bridge.command("component.get", args));
  register(server, bridge, "unity_script_create", "Create a C# MonoBehaviour script under Assets.", scriptCreateSchema, async args => bridge.command("script.create", args));
  register(server, bridge, "unity_script_attach", "Attach a compiled script component to a GameObject.", scriptAttachSchema, async args => bridge.command("script.attach", args));
  register(server, bridge, "unity_asset_refresh", "Refresh the Unity AssetDatabase.", emptySchema, async () => bridge.command("asset.refresh"));
}

function register(
  server: McpServer,
  bridge: UnityBridgeClient,
  name: string,
  description: string,
  schema: ToolSchema,
  handler: (args: Record<string, unknown>) => Promise<unknown>,
): void {
  server.tool(name, description, schema, async args => {
    try {
      const result = await handler(args);
      return {
        content: [
          {
            type: "text",
            text: JSON.stringify(result, null, 2),
          },
        ],
      };
    } catch (error) {
      return {
        isError: true,
        content: [
          {
            type: "text",
            text: toErrorText(error),
          },
        ],
      };
    }
  });

  void bridge;
}

import type { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import type { UnityBridgeClient } from "../unity/UnityBridgeClient.js";
import { toErrorText } from "../errors.js";
import {
  assetCreateFolderSchema,
  assetFindSchema,
  assetPathSchema,
  bridgeSelectSchema,
  componentPropertyGetSchema,
  componentPropertySetSchema,
  componentSchema,
  emptySchema,
  gameObjectCreateSchema,
  gameObjectRenameSchema,
  hierarchyListSchema,
  pathSchema,
  prefabCreateSchema,
  prefabInstantiateSchema,
  sceneNewSchema,
  sceneOpenSchema,
  scriptAttachSchema,
  scriptCreateSchema,
  transformSetSchema,
} from "./schemas.js";

type ToolSchema = Parameters<McpServer["tool"]>[2];

export function registerTools(server: McpServer, bridge: UnityBridgeClient): void {
  register(server, bridge, "unity_health", "Check whether the Unity Editor bridge is reachable.", emptySchema, async () => bridge.health());
  register(server, bridge, "unity_bridge_list", "List all Unity bridges discovered on the configured port range.", emptySchema, async () => bridge.listBridges());
  register(server, bridge, "unity_bridge_select", "Bind the current MCP session to a specific Unity bridge.", bridgeSelectSchema, async args => bridge.selectBridge(args));
  register(server, bridge, "unity_bridge_current", "Get the Unity bridge currently bound to this MCP session.", emptySchema, async () => bridge.describeCurrent());
  register(server, bridge, "unity_bridge_get_config", "Get Unity bridge safety configuration.", emptySchema, async () => bridge.command("bridge.getConfig"));
  register(server, bridge, "unity_bridge_get_log_path", "Get Unity bridge log file path.", emptySchema, async () => bridge.command("bridge.getLogPath"));
  register(server, bridge, "unity_project_get_info", "Get Unity version and current project information.", emptySchema, async () => bridge.command("project.getInfo"));
  register(server, bridge, "unity_scene_get_active", "Get the active Unity scene.", emptySchema, async () => bridge.command("scene.getActive"));
  register(server, bridge, "unity_scene_new", "Create a new Unity scene.", sceneNewSchema, async args => bridge.command("scene.new", args));
  register(server, bridge, "unity_scene_open", "Open a Unity scene asset.", sceneOpenSchema, async args => bridge.command("scene.open", args));
  register(server, bridge, "unity_scene_save", "Save the active Unity scene.", emptySchema, async () => bridge.command("scene.save"));
  register(server, bridge, "unity_scene_save_as", "Save the active Unity scene to a path.", pathSchema, async args => bridge.command("scene.saveAs", args));
  register(server, bridge, "unity_scene_get_dirty", "Check whether the active Unity scene has unsaved changes.", emptySchema, async () => bridge.command("scene.getDirty"));
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
  register(server, bridge, "unity_component_get_property", "Get a SerializedProperty value from a component.", componentPropertyGetSchema, async args => bridge.command("component.getProperty", args));
  register(server, bridge, "unity_component_set_property", "Set a SerializedProperty value on a component.", componentPropertySetSchema, async args => bridge.command("component.setProperty", args));
  register(server, bridge, "unity_script_create", "Create a C# MonoBehaviour script under Assets.", scriptCreateSchema, async args => bridge.command("script.create", args));
  register(server, bridge, "unity_script_attach", "Attach a compiled script component to a GameObject.", scriptAttachSchema, async args => bridge.command("script.attach", args));
  register(server, bridge, "unity_asset_refresh", "Refresh the Unity AssetDatabase.", emptySchema, async () => bridge.command("asset.refresh"));
  register(server, bridge, "unity_asset_find", "Find assets with an AssetDatabase filter.", assetFindSchema, async args => bridge.command("asset.find", args));
  register(server, bridge, "unity_asset_load", "Load asset metadata by path.", assetPathSchema, async args => bridge.command("asset.load", args));
  register(server, bridge, "unity_asset_create_folder", "Create a folder under Assets.", assetCreateFolderSchema, async args => bridge.command("asset.createFolder", args));
  register(server, bridge, "unity_asset_delete", "Delete an asset under Assets.", assetPathSchema, async args => bridge.command("asset.delete", args));
  register(server, bridge, "unity_prefab_create", "Create a prefab asset from a scene GameObject.", prefabCreateSchema, async args => bridge.command("prefab.create", args));
  register(server, bridge, "unity_prefab_instantiate", "Instantiate a prefab asset into the active scene.", prefabInstantiateSchema, async args => bridge.command("prefab.instantiate", args));
  register(server, bridge, "unity_prefab_apply", "Apply a prefab instance back to its prefab asset.", pathSchema, async args => bridge.command("prefab.apply", args));
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

# Unity Editor MCP

让支持 MCP 的 AI 客户端稳定操作 Unity Editor，最低兼容 Unity 2019.4 LTS。

本项目包含两部分：

- `Packages/com.yys.unity-mcp-bridge/`：Unity Editor Bridge，本地 UPM 包，运行在 Unity Editor 内。
- `server/`：Node.js + TypeScript MCP stdio server，连接 AI 客户端与 Unity Bridge。

## 特性

- 最低兼容 Unity `2019.4 LTS`，支持更高版本。
- Bridge 在 Unity Editor 加载后自动启动。
- 默认监听 `127.0.0.1:8765`，端口占用时自动回退到 `8766-8775`。
- MCP Server 支持自动探测多个 Unity Bridge。
- 支持多 Unity 工程路由：按工程路径、工程名、当前工作目录或运行时选择目标工程。
- 提供层级、场景、GameObject、Transform、Component、Asset、Prefab、脚本创建与挂载等常用工具。
- Unity 编辑器内置 `Tools > Unity MCP` 窗口，可查看状态、日志和权限开关。

## 版本

- Unity 本地 UPM 包：`com.yys.unity-mcp-bridge@0.6.2`
- MCP Server：`@luoluo123/unity-mcp-server@0.6.2`
- Unity 最低兼容版本：`2019.4 LTS`
- Node.js 要求：`18` 或更高版本

## 快速开始

### 1. 安装 Unity Bridge

在 Unity 2019.4 LTS 或更高版本工程中打开：

```text
Window > Package Manager > + > Add package from git URL...
```

输入 Gitee 地址：

```text
https://gitee.com/furanzhang/unity-editor-mcp.git?path=/Packages/com.yys.unity-mcp-bridge#v0.6.2
```

也可以使用 GitHub 地址：

```text
https://github.com/FRZ5201314/UnityEditorMCP.git?path=/Packages/com.yys.unity-mcp-bridge#v0.6.2
```

导入后 Bridge 会自动启动。也可以在 Unity 菜单中打开：

```text
Tools > Unity MCP
```

该窗口可查看监听地址、运行状态、工程信息、权限开关和 Bridge 日志。

### 2. 配置 MCP Server

MCP Server 已独立发布为 npm 包，MCP 客户端可以通过 `npx` 启动。
下面是 CC Switch 可直接使用的完整 JSON 配置。

```json
{
  "type": "stdio",
  "command": "npx",
  "args": [
    "-y",
    "@luoluo123/unity-mcp-server@0.6.2"
  ],
  "env": {
    "UNITY_MCP_AUTO_DETECT": "true",
    "UNITY_MCP_DETECT_HOST": "127.0.0.1",
    "UNITY_MCP_DETECT_PORT_START": "8765",
    "UNITY_MCP_DETECT_PORT_END": "8775",
    "UNITY_MCP_TIMEOUT_MS": "30000"
  },
  "enabled": true
}
```

多工程场景推荐使用上面的自动探测配置。不要在该配置中设置 `UNITY_MCP_BRIDGE_URL`，否则 MCP Server 会跳过工程发现逻辑，只连接固定端口。

### 3. 验证连接

启动 Unity Editor 和 CC Switch / Codex 后，调用：

```text
unity_health
```

如果返回 Bridge 状态信息，说明 MCP Server 已连接到 Unity Editor Bridge。

## 多 Unity 工程

同时打开多个 Unity 工程时，每个 Bridge 会占用 `8765-8775` 范围内的不同端口。MCP Server 的选择优先级：

1. `UNITY_MCP_BRIDGE_URL`：固定 Bridge 地址，设置后跳过自动发现。
2. `UNITY_MCP_PROJECT_PATH`：按 Unity 工程绝对路径匹配。
3. `UNITY_MCP_PROJECT_NAME`：按 `Application.productName` 匹配。
4. 当前工作目录推断：从 MCP Server 启动目录向上查找包含 `Assets/` 和 `ProjectSettings/` 的 Unity 工程。
5. 只发现一个 Bridge 时直接使用。
6. 多个 Bridge 在线且无法判断时，返回候选列表，需要手动选择。

运行时可以先列出 Bridge：

```text
unity_bridge_list
```

然后选择目标工程：

```json
{ "projectPath": "F:/Projects/ClientB" }
```

或：

```json
{ "projectName": "ClientA" }
```

对应工具为：

```text
unity_bridge_select
```

## 常用工具

Bridge 管理：

- `unity_health`
- `unity_bridge_list`
- `unity_bridge_select`
- `unity_bridge_current`
- `unity_bridge_get_config`
- `unity_bridge_get_log_path`

Unity 工程与场景：

- `unity_project_get_info`
- `unity_scene_get_active`
- `unity_scene_new`
- `unity_scene_open`
- `unity_scene_save`
- `unity_scene_save_as`
- `unity_scene_get_dirty`

对象、组件与资源：

- `unity_hierarchy_list`
- `unity_gameobject_create`
- `unity_gameobject_delete`
- `unity_gameobject_find`
- `unity_gameobject_rename`
- `unity_transform_get`
- `unity_transform_set`
- `unity_component_list`
- `unity_component_add`
- `unity_component_remove`
- `unity_component_get`
- `unity_component_get_property`
- `unity_component_set_property`
- `unity_asset_refresh`
- `unity_asset_find`
- `unity_asset_load`
- `unity_asset_create_folder`
- `unity_asset_delete`
- `unity_prefab_create`
- `unity_prefab_instantiate`
- `unity_prefab_apply`

脚本：

- `unity_script_create`
- `unity_script_attach`

完整参数说明见 [docs/mcp-usage-guide.md](docs/mcp-usage-guide.md)。

## 权限与日志

Bridge 日志写入目标 Unity 工程：

```text
Library/UnityMcp/bridge.log
```

`Tools > Unity MCP` 中的权限开关：

- `Allow Scene Object Delete`：控制 `unity_gameobject_delete` 和 `unity_component_remove`。
- `Allow Asset Delete`：控制 `unity_asset_delete`。
- `Allow Script Write`：控制 `unity_script_create`。

这些开关只限制 MCP Bridge 暴露的命令入口，不是 Codex、Shell、Unity UI 或文件系统层面的全局安全边界。

## 注意事项

- Unity Editor API 会在 Unity 主线程执行。
- 脚本编译会触发 Unity 域重载，Bridge 会在重载完成后自动恢复监听。
- `unity_script_create` 只能在目标 Unity 工程的 `Assets/` 下创建 `.cs` 文件。
- `unity_script_attach` 会等待目标脚本类型可解析后再尝试挂载，可通过 `compileTimeoutMs` 设置等待超时。
- Component 类型名支持完整名称，例如 `UnityEngine.Rigidbody`，也支持短名称，例如 `Rigidbody`。

## 文档

- [完整使用说明](docs/mcp-usage-guide.md)
- [开发进度](docs/development-status.md)
- [实现计划](docs/implementation-plan.md)

## 项目约定

- 新增或维护的 Markdown 文档统一使用中文编写。
- Git 提交描述统一使用中文。
- 涉及命令、API 名称、工具名称、路径、错误码等技术标识时保留原文。
- Unity 包内的 `.meta` 文件需要纳入版本库，避免通过本地 Package Manager 导入时 GUID 不稳定。

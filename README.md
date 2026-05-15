# Unity 2019 MCP

这是一个兼容 Unity 2019.4 LTS 的 MCP Server 与 Unity Editor Bridge，用于让支持 MCP 的 AI 客户端安全、稳定地操作 Unity Editor。

当前仓库分为两部分：

- `Packages/com.yys.unity2019-mcp/`：Unity Editor Bridge，本地 UPM 包。
- `server/`：Node.js + TypeScript MCP stdio server。

当前开发进度与后续计划见：

```text
docs/development-status.md
```

## 项目约定

- 本项目后续新增或维护的 Markdown 文档统一使用中文编写。
- 本项目 Git 提交描述统一使用中文。
- 涉及命令、API 名称、工具名称、路径、错误码等技术标识时保留原文。
- Unity 包内的 `.meta` 文件需要纳入版本库，避免通过本地 Package Manager 导入时 GUID 不稳定。

## Unity 本地包导入

在目标 Unity 2019.4 LTS 工程中通过 Package Manager 导入 Bridge：

1. 打开 `Window > Package Manager`。
2. 点击左上角 `+`。
3. 选择 `Add package from disk...`。
4. 选择本仓库中的包描述文件：

```text
F:\AIProject\Unity2019MCP\Packages\com.yys.unity2019-mcp\package.json
```

导入后，Bridge 会在 Unity Editor 加载后自动启动，默认监听：

```text
http://127.0.0.1:8765
```

也可以通过菜单手动控制：

```text
Tools > Unity 2019 MCP > Start Bridge
Tools > Unity 2019 MCP > Stop Bridge
```

## Unity 包依赖

Bridge 包依赖 Unity 版 Newtonsoft.Json：

```json
{
  "dependencies": {
    "com.unity.nuget.newtonsoft-json": "2.0.0"
  }
}
```

该依赖已声明在 `Packages/com.yys.unity2019-mcp/package.json` 中。通过 Package Manager 导入本地包时，Unity 会尝试自动解析依赖。

## MCP Server 设置

```bash
cd server
npm install
npm run build
```

运行 stdio server：

```bash
node dist/index.js
```

可用环境变量：

```text
UNITY_MCP_BRIDGE_URL=http://127.0.0.1:8765
UNITY_MCP_TIMEOUT_MS=30000
UNITY_MCP_AUTO_DETECT=true
UNITY_MCP_DETECT_HOST=127.0.0.1
UNITY_MCP_DETECT_PORT_START=8765
UNITY_MCP_DETECT_PORT_END=8775
```

## MCP 客户端配置示例

在 MCP 客户端中添加 stdio server，启动命令为：

```bash
node F:\AIProject\Unity2019MCP\server\dist\index.js
```

启动 MCP 客户端前需要先完成 server 构建。

## 工具列表

- `unity_health`
- `unity_bridge_get_config`
- `unity_bridge_get_log_path`
- `unity_project_get_info`
- `unity_scene_get_active`
- `unity_scene_new`
- `unity_scene_open`
- `unity_scene_save`
- `unity_scene_save_as`
- `unity_scene_get_dirty`
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
- `unity_script_create`
- `unity_script_attach`
- `unity_asset_refresh`
- `unity_asset_find`
- `unity_asset_load`
- `unity_asset_create_folder`
- `unity_asset_delete`
- `unity_prefab_create`
- `unity_prefab_instantiate`
- `unity_prefab_apply`

## 注意事项

- Unity Editor API 会在 Unity 主线程执行。
- Bridge 默认监听端口被占用时会尝试 `8765-8775`。
- MCP Server 默认会自动探测 `127.0.0.1:8765-8775` 上的 Bridge。
- Bridge 日志写入 `Library/Unity2019Mcp/bridge.log`。
- `Allow Scene Object Delete` 控制场景对象删除，`Allow Asset Delete` 控制 `Assets/` 下资源删除。
- 脚本编译会触发 Unity 域重载，Bridge 会在重载完成后自动恢复监听。
- 脚本文件只能创建在目标 Unity 工程的 `Assets/` 下，并且必须以 `.cs` 结尾。
- `unity_script_create` 会在写入脚本后请求 Unity 导入资源并编译脚本。
- `unity_script_attach` 会等待目标脚本类型可解析后再尝试挂载，可通过 `compileTimeoutMs` 设置等待超时。
- Component 类型名支持完整名称，例如 `UnityEngine.Rigidbody`，也支持短名称，例如 `Rigidbody`。

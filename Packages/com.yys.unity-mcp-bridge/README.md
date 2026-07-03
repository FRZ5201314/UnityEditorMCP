# Unity MCP Bridge 包

这是 Unity MCP 的 Unity Editor Bridge 本地 UPM 包。

## Git 安装

在 Unity 2019.4 LTS 或更高版本工程中执行：

1. 打开 `Window > Package Manager`。
2. 点击左上角 `+`。
3. 选择 `Add package from git URL...`。
4. 输入 Gitee 地址：

```text
https://gitee.com/furanzhang/unity-editor-mcp.git?path=/Packages/com.yys.unity-mcp-bridge#v0.6.1
```

也可以使用 GitHub 地址：

```text
https://github.com/FRZ5201314/UnityEditorMCP.git?path=/Packages/com.yys.unity-mcp-bridge#v0.6.1
```

导入后，Bridge 会在 Editor 加载后自动启动，默认监听：

```text
http://127.0.0.1:8765
```

打开 `Tools > Unity MCP` 编辑器窗口，可以查看监听地址、启停 Bridge 和切换 Permissions 开关。

## 本地开发导入

如果正在开发本仓库，可以改用 `Add package from disk...` 并选择：

```text
<repo-path>\Packages\com.yys.unity-mcp-bridge\package.json
```

## 依赖

本包不声明额外 Unity Package 依赖，也不要求目标工程安装特定 JSON 库。

为了避免和目标工程中已有的 `JsonDotNet`、`Newtonsoft.Json` 或其他 JSON DLL 冲突，Bridge 内部使用包内轻量 JSON 工具处理 MCP 通信数据。

## 注意事项

- 本包仅包含 Unity Editor Bridge，不包含 Node.js MCP Server。
- MCP Server 位于仓库根目录的 `server/`。
- Unity Editor API 会在 Unity 主线程执行。
- Bridge 默认监听端口被占用时会尝试 `8765-8775`。
- Bridge 日志写入 `Library/UnityMcp/bridge.log`。
- Bridge `/health` 会返回 `projectPath`、`productName`、`instanceId`，便于 MCP Server 在多工程同时打开时识别目标 Bridge。
- 可通过 `Tools > Unity MCP` 编辑器窗口中的 Permissions 区域关闭部分 MCP Bridge 命令入口。
- `Bridge Permissions` 只限制 MCP Bridge 命令本身，不限制 Codex、Shell、Unity UI 或其他方式对项目进行修改。
- `Allow Scene Object Delete` 控制 `gameObject.delete` 和 `component.remove`，`Allow Asset Delete` 控制 `asset.delete`，`Allow Script Write` 控制 `script.create`。
- 脚本编译会触发 Unity 域重载，Bridge 会在重载完成后自动恢复监听。
- 脚本文件只能创建在目标工程的 `Assets/` 下，并且必须以 `.cs` 结尾。
- `script.create` 会在写入脚本后强制导入资源并请求脚本编译。
- `script.attach` 会等待目标脚本组件类型可解析后再尝试挂载，可通过 `compileTimeoutMs` 设置等待超时。
- Scene 工具支持新建、打开、保存为指定路径和查询 dirty 状态。
- Prefab 工具支持从场景对象创建 prefab、实例化 prefab、应用 prefab 实例改动。
- Asset 工具支持查找、加载元数据、创建文件夹和删除 `Assets/` 下资源。
- Inspector 属性工具支持基础 `SerializedProperty` 类型读写。

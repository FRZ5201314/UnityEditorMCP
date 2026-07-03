# 🎮 Unity Editor MCP

让支持 MCP 的 AI 客户端稳定操作 Unity Editor。

最低兼容 Unity `2019.4 LTS`，支持更高版本。

## ✨ 亮点

- 🚀 Unity Bridge 自动随 Editor 启动，默认监听 `127.0.0.1:8765`
- 🔎 MCP Server 自动探测 `8765-8775` 端口范围内的 Unity 工程
- 🧭 支持多 Unity 工程路由，可自动按工程路径、工程名或运行时选择目标
- 🧱 覆盖场景、Hierarchy、GameObject、Transform、Component、Asset、Prefab、脚本等常用编辑器操作
- 🪟 内置 `Tools > Unity MCP` 窗口，可查看状态、日志和权限开关

## 📦 版本

| 模块 | 包名 / 版本 |
| --- | --- |
| Unity Bridge | `com.yys.unity-mcp-bridge@0.6.2` |
| MCP Server | `@luoluo123/unity-mcp-server@0.6.2` |
| Unity | `2019.4 LTS+` |
| Node.js | `18+` |

## ⚡ 快速开始

### 1. 安装 Unity Bridge

在 Unity 中打开：

```text
Window > Package Manager > + > Add package from git URL...
```

输入任一地址：

```text
https://gitee.com/furanzhang/unity-editor-mcp.git?path=/Packages/com.yys.unity-mcp-bridge#v0.6.2
```

```text
https://github.com/FRZ5201314/UnityEditorMCP.git?path=/Packages/com.yys.unity-mcp-bridge#v0.6.2
```

导入后可在 Unity 菜单打开：

```text
Tools > Unity MCP
```

### 2. 配置 MCP Server

MCP Server 已发布为 npm 包，推荐用 `npx` 启动：

```json
{
  "type": "stdio",
  "command": "npx",
  "args": ["-y", "@luoluo123/unity-mcp-server@0.6.2"],
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

### 3. 验证连接

启动 Unity Editor 和 MCP 客户端后调用：

```text
unity_health
```

返回 Bridge 状态信息即连接成功。

## 🧩 项目结构

```text
Packages/com.yys.unity-mcp-bridge/   Unity Editor Bridge UPM 包
server/                              Node.js MCP stdio server
docs/                                使用说明、测试记录和实现文档
```

## 🧭 多工程

同时打开多个 Unity 工程时，每个 Bridge 会自动占用 `8765-8775` 中的可用端口。MCP Server 的选择优先级：

1. `UNITY_MCP_BRIDGE_URL`
2. `UNITY_MCP_PROJECT_PATH`
3. `UNITY_MCP_PROJECT_NAME`
4. 当前工作目录推断
5. 唯一在线 Bridge
6. 多个候选时通过 `unity_bridge_list` / `unity_bridge_select` 手动选择

## 🔐 权限与日志

Bridge 日志：

```text
Library/UnityMcp/bridge.log
```

`Tools > Unity MCP` 中可切换：

- `Allow Scene Object Delete`
- `Allow Asset Delete`
- `Allow Script Write`

这些开关只限制 MCP Bridge 暴露的命令入口，不是 Codex、Shell、Unity UI 或文件系统层面的全局安全边界。

## 🛠 常用工具

- `unity_health`
- `unity_bridge_list`
- `unity_bridge_select`
- `unity_project_get_info`
- `unity_hierarchy_list`
- `unity_gameobject_create`
- `unity_transform_set`
- `unity_component_add`
- `unity_asset_find`
- `unity_prefab_create`
- `unity_script_create`
- `unity_script_attach`

完整工具和参数见 [完整使用说明](docs/mcp-usage-guide.md)。

## 📚 文档

- [完整使用说明](docs/mcp-usage-guide.md)
- [开发进度](docs/development-status.md)
- [实现计划](docs/implementation-plan.md)

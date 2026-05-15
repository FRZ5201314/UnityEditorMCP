# Unity 2019 MCP

这是一个兼容 Unity 2019.4 LTS 的 MCP Server 与 Unity Editor Bridge，用于让支持 MCP 的 AI 客户端安全、稳定地操作 Unity Editor。

## 已包含内容

- Unity Editor HTTP Bridge，默认地址为 `http://127.0.0.1:8765`
- 使用 Node.js 与 TypeScript 实现的 MCP stdio server
- 项目信息、场景、Hierarchy、GameObject、Transform、Component、脚本与 AssetDatabase 刷新工具
- Bridge 结构化响应与 MCP 错误文本

## 文档约定

本项目后续新增或维护的 Markdown 文档统一使用中文编写。涉及命令、API 名称、工具名称、路径、错误码等技术标识时保留原文。

## Unity 设置

1. 使用 Unity 2019.4 LTS 打开本目录。
2. 确认 `Packages/manifest.json` 中已安装 Unity 版 Newtonsoft.Json：

```json
{
  "dependencies": {
    "com.unity.nuget.newtonsoft-json": "2.0.0"
  }
}
```

3. 等待 Unity 编译 Editor 脚本。
4. Bridge 会在 Editor 加载后自动启动，也可以通过菜单手动控制：

```text
Tools > Unity 2019 MCP > Start Bridge
Tools > Unity 2019 MCP > Stop Bridge
```

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
```

## MCP 客户端配置示例

在 MCP 客户端中添加 stdio server，启动命令为：

```bash
node F:\AIProject\Unity2019MCP\server\dist\index.js
```

启动 MCP 客户端前需要先完成 server 构建。

## 工具列表

- `unity_health`
- `unity_project_get_info`
- `unity_scene_get_active`
- `unity_scene_save`
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
- `unity_script_create`
- `unity_script_attach`
- `unity_asset_refresh`

## 注意事项

- Unity Editor API 会在 Unity 主线程执行。
- 脚本文件只能创建在 `Assets/` 下，并且必须以 `.cs` 结尾。
- `unity_script_attach` 需要等待 Unity 编译完成后，脚本类型才可被挂载。
- Component 类型名支持完整名称，例如 `UnityEngine.Rigidbody`，也支持短名称，例如 `Rigidbody`。

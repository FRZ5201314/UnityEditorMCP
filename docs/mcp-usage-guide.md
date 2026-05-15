# Unity 2019 MCP 使用说明

本文档说明当前 `Unity2019MCP` 的安装、配置、工具调用方式和常见排查方法。

当前版本：

- Unity 本地 UPM 包：`com.yys.unity2019-mcp@0.5.2`
- MCP Server：`unity2019-mcp-server@0.5.2`
- Unity 目标版本：Unity 2019.4 LTS

## 架构说明

当前 MCP 由两部分组成：

- Unity Editor Bridge：运行在 Unity Editor 内，负责调用 UnityEditor / UnityEngine API。
- MCP Server：运行在 Unity 外部，负责向 MCP 客户端暴露工具，并把请求转发给 Unity Editor Bridge。

通信链路：

```text
MCP Client
  -> Node.js MCP Server
  -> http://127.0.0.1:8765/command
  -> Unity Editor Bridge
  -> Unity Editor API
```

Bridge 默认监听：

```text
http://127.0.0.1:8765
```

如果 `8765` 被占用，Bridge 会尝试 `8766-8775`。MCP Server 默认会自动探测这些端口。

## 安装 Unity Bridge

在目标 Unity 2019.4 LTS 工程中导入本地 UPM 包：

1. 打开 `Window > Package Manager`。
2. 点击左上角 `+`。
3. 选择 `Add package from disk...`。
4. 选择：

```text
F:\AIProject\Unity2019MCP\Packages\com.yys.unity2019-mcp\package.json
```

导入后，Bridge 会在 Unity Editor 加载时自动启动。

也可以通过菜单手动控制：

```text
Tools > Unity 2019 MCP > Start Bridge
Tools > Unity 2019 MCP > Stop Bridge
```

Bridge 日志位置：

```text
Library/Unity2019Mcp/bridge.log
```

## 构建 MCP Server

在仓库根目录执行：

```powershell
cd server
npm install
npm run build
```

本项目开发环境要求 shell 命令通过 `rtk` 执行时，可使用：

```powershell
rtk proxy npm install
rtk proxy npm run build
```

构建完成后，入口文件为：

```text
F:\AIProject\Unity2019MCP\server\dist\index.js
```

## MCP 客户端配置

在 MCP 客户端中添加 stdio server，启动命令使用：

```bash
node F:\AIProject\Unity2019MCP\server\dist\index.js
```

如果客户端配置需要拆分 `command` 和 `args`，可使用：

```json
{
  "command": "node",
  "args": [
    "F:\\AIProject\\Unity2019MCP\\server\\dist\\index.js"
  ]
}
```

启动 MCP 客户端前，需要先完成 MCP Server 构建，并保持 Unity Editor 打开。

### Codex 配置示例

如果 Codex 使用 MCP JSON 配置，把下面配置合并到 Codex 的 MCP 配置文件中。若配置文件中已经存在 `mcpServers`，只需要把 `unity2019-mcp` 这一项加入已有对象。

```json
{
  "mcpServers": {
    "unity2019-mcp": {
      "command": "node",
      "args": [
        "F:\\AIProject\\Unity2019MCP\\server\\dist\\index.js"
      ],
      "env": {
        "UNITY_MCP_AUTO_DETECT": "true",
        "UNITY_MCP_DETECT_HOST": "127.0.0.1",
        "UNITY_MCP_DETECT_PORT_START": "8765",
        "UNITY_MCP_DETECT_PORT_END": "8775",
        "UNITY_MCP_TIMEOUT_MS": "30000"
      }
    }
  }
}
```

字段说明：

- `command`：启动 MCP Server 的可执行程序。这里使用 `node`。
- `args`：传给 `node` 的参数，即编译后的 MCP Server 入口文件。
- `UNITY_MCP_AUTO_DETECT`：开启 Bridge 自动探测。推荐保持 `true`。
- `UNITY_MCP_DETECT_PORT_START` / `UNITY_MCP_DETECT_PORT_END`：Bridge 端口探测范围，默认对应 Unity Bridge 的 `8765-8775` 回退范围。
- `UNITY_MCP_TIMEOUT_MS`：等待 Unity Bridge 响应的超时时间。

如果明确知道 Bridge 监听地址，也可以改成固定地址配置：

```json
{
  "mcpServers": {
    "unity2019-mcp": {
      "command": "node",
      "args": [
        "F:\\AIProject\\Unity2019MCP\\server\\dist\\index.js"
      ],
      "env": {
        "UNITY_MCP_BRIDGE_URL": "http://127.0.0.1:8765",
        "UNITY_MCP_AUTO_DETECT": "false",
        "UNITY_MCP_TIMEOUT_MS": "30000"
      }
    }
  }
}
```

推荐优先使用自动探测配置，因为 Unity Bridge 在 `8765` 被占用时会自动切换到 `8766-8775`。

## 环境变量

默认情况下不需要设置环境变量。

可用环境变量：

```text
UNITY_MCP_BRIDGE_URL=http://127.0.0.1:8765
UNITY_MCP_TIMEOUT_MS=30000
UNITY_MCP_AUTO_DETECT=true
UNITY_MCP_DETECT_HOST=127.0.0.1
UNITY_MCP_DETECT_PORT_START=8765
UNITY_MCP_DETECT_PORT_END=8775
```

说明：

- `UNITY_MCP_BRIDGE_URL`：指定 Bridge 地址。
- `UNITY_MCP_TIMEOUT_MS`：MCP Server 等待 Bridge 响应的超时时间。
- `UNITY_MCP_AUTO_DETECT`：为 `true` 时自动扫描本地端口。
- `UNITY_MCP_DETECT_PORT_START` / `UNITY_MCP_DETECT_PORT_END`：自动探测端口范围。

## 快速验证

本节用于确认 Codex 或其他 MCP 客户端已经正确连接 Unity Editor。

### 1. 确认 Unity 侧状态

先确认：

- Unity Editor 已打开目标工程。
- Unity Console 没有 C# 编译错误。
- 已导入 `com.yys.unity2019-mcp` 本地包。
- 菜单中能看到 `Tools > Unity 2019 MCP`。

如果不确定 Bridge 是否已启动，执行：

```text
Tools > Unity 2019 MCP > Start Bridge
```

预期结果：

- Unity Console 不出现 Bridge 启动错误。
- `Library/Unity2019Mcp/bridge.log` 中出现 `Bridge started on ...`。

### 2. 验证 MCP 到 Bridge 的连接

调用工具：

```text
unity_health
```

输入参数：

```json
{}
```

预期结果：

- 返回成功结果。
- 如果失败，优先检查 Unity Editor 是否打开、Bridge 是否启动、Codex MCP 配置中的 `args` 路径是否正确。

### 3. 读取工程信息

调用工具：

```text
unity_project_get_info
```

输入参数：

```json
{}
```

预期结果：

- 返回 Unity 版本。
- 返回当前工程路径。
- 返回 Bridge 所在 Unity Editor 的基本信息。

### 4. 查询当前场景

调用工具：

```text
unity_scene_get_active
```

输入参数：

```json
{}
```

预期结果：

- 返回当前打开场景的路径、名称和保存状态。

### 5. 列出 Hierarchy

调用工具：

```text
unity_hierarchy_list
```

输入参数：

```json
{
  "recursive": true
}
```

预期结果：

- 返回当前场景中的 GameObject 列表。
- 如果是空场景，也应返回结构化结果，而不是连接错误。

### 6. 创建测试对象

调用工具：

```text
unity_gameobject_create
```

输入参数：

```json
{
  "name": "McpTestObject"
}
```

预期结果：

- Unity Hierarchy 中出现 `McpTestObject`。
- 工具返回对象路径、实例 ID 等信息。

### 7. 修改测试对象 Transform

调用工具：

```text
unity_transform_set
```

输入参数：

```json
{
  "path": "McpTestObject",
  "position": { "x": 1, "y": 2, "z": 3 },
  "rotation": { "x": 0, "y": 45, "z": 0 },
  "scale": { "x": 1, "y": 1, "z": 1 }
}
```

预期结果：

- Unity Inspector 中 `McpTestObject` 的 Transform 更新。
- 工具返回成功结果。

### 8. 保存验证场景

如果当前场景还没有保存，调用：

```text
unity_scene_save_as
```

输入参数：

```json
{
  "path": "Assets/Scenes/McpQuickTest.unity"
}
```

如果当前场景已有路径，也可以调用：

```text
unity_scene_save
```

输入参数：

```json
{}
```

预期结果：

- 场景保存成功。
- `Assets/Scenes/McpQuickTest.unity` 出现在 Project 视图中，或当前场景保存状态变为 clean。

完成以上步骤后，可以认为 MCP Server、Unity Bridge、端口探测和基础 Unity Editor 操作链路都已经可用。

## 常用工作流

### 查询场景对象

列出当前场景 Hierarchy：

```json
{
  "recursive": true
}
```

工具：

```text
unity_hierarchy_list
```

查找指定对象：

```json
{
  "path": "Player"
}
```

工具：

```text
unity_gameobject_find
```

对象路径使用 Unity Hierarchy 路径，例如：

```text
Player
Environment/Light
Canvas/MainPanel/Button
```

### 创建和修改 GameObject

创建对象：

```json
{
  "name": "Enemy",
  "parentPath": "Enemies"
}
```

工具：

```text
unity_gameobject_create
```

修改 Transform：

```json
{
  "path": "Enemy",
  "position": { "x": 0, "y": 1, "z": 5 },
  "rotation": { "x": 0, "y": 45, "z": 0 },
  "scale": { "x": 1, "y": 1, "z": 1 }
}
```

工具：

```text
unity_transform_set
```

读取 Transform：

```json
{
  "path": "Enemy"
}
```

工具：

```text
unity_transform_get
```

删除对象：

```json
{
  "path": "Enemy"
}
```

工具：

```text
unity_gameobject_delete
```

### 组件操作

列出组件：

```json
{
  "path": "Player"
}
```

工具：

```text
unity_component_list
```

添加组件：

```json
{
  "path": "Player",
  "typeName": "Rigidbody"
}
```

工具：

```text
unity_component_add
```

`typeName` 支持短名称和完整名称：

```text
Rigidbody
UnityEngine.Rigidbody
BoxCollider
UnityEngine.BoxCollider
```

读取组件：

```json
{
  "path": "Player",
  "typeName": "Rigidbody"
}
```

工具：

```text
unity_component_get
```

移除组件：

```json
{
  "path": "Player",
  "typeName": "Rigidbody"
}
```

工具：

```text
unity_component_remove
```

### Inspector 属性读写

读取 SerializedProperty：

```json
{
  "path": "Main Camera",
  "typeName": "Camera",
  "propertyPath": "m_FieldOfView"
}
```

工具：

```text
unity_component_get_property
```

写入 SerializedProperty：

```json
{
  "path": "Main Camera",
  "typeName": "Camera",
  "propertyPath": "m_FieldOfView",
  "value": 70
}
```

工具：

```text
unity_component_set_property
```

支持的基础值类型：

- `string`
- `number`
- `boolean`
- `null`
- `Vector2`：`{ "x": 1, "y": 2 }`
- `Vector3`：`{ "x": 1, "y": 2, "z": 3 }`
- `Color`：`{ "r": 1, "g": 1, "b": 1, "a": 1 }`

设置资源引用：

```json
{
  "path": "RendererObject",
  "typeName": "MeshRenderer",
  "propertyPath": "m_Materials.Array.data[0]",
  "objectReferenceAssetPath": "Assets/Materials/Main.mat"
}
```

### 脚本创建与挂载

创建脚本：

```json
{
  "assetPath": "Assets/Scripts/MyBehaviour.cs",
  "className": "MyBehaviour",
  "overwrite": false
}
```

工具：

```text
unity_script_create
```

也可以传入完整内容：

```json
{
  "assetPath": "Assets/Scripts/MyBehaviour.cs",
  "content": "using UnityEngine;\n\npublic class MyBehaviour : MonoBehaviour\n{\n}\n",
  "overwrite": true
}
```

脚本创建后，Bridge 会请求 Unity 导入资源并编译脚本。编译会触发域重载，Bridge 会在重载后自动恢复监听。

挂载脚本：

```json
{
  "path": "Player",
  "typeName": "MyBehaviour",
  "compileTimeoutMs": 30000
}
```

工具：

```text
unity_script_attach
```

如果脚本编译失败，工具会返回结构化错误，通常包含近期 Unity Console 错误。

### Scene 操作

读取当前场景：

```text
unity_scene_get_active
```

新建场景：

```json
{
  "setup": "DefaultGameObjects",
  "mode": "Single"
}
```

工具：

```text
unity_scene_new
```

可选值：

- `setup`：`DefaultGameObjects`、`EmptyScene`
- `mode`：`Single`、`Additive`

保存当前场景：

```text
unity_scene_save
```

另存当前场景：

```json
{
  "path": "Assets/Scenes/TestScene.unity"
}
```

工具：

```text
unity_scene_save_as
```

打开场景：

```json
{
  "path": "Assets/Scenes/TestScene.unity",
  "mode": "Single"
}
```

工具：

```text
unity_scene_open
```

查询当前场景是否有未保存改动：

```text
unity_scene_get_dirty
```

### Prefab 操作

从场景对象创建 Prefab：

```json
{
  "path": "Player",
  "assetPath": "Assets/Prefabs/Player.prefab"
}
```

工具：

```text
unity_prefab_create
```

实例化 Prefab：

```json
{
  "assetPath": "Assets/Prefabs/Player.prefab",
  "parentPath": "SpawnRoot"
}
```

工具：

```text
unity_prefab_instantiate
```

应用 Prefab 实例改动：

```json
{
  "path": "Player"
}
```

工具：

```text
unity_prefab_apply
```

### Asset 操作

刷新 AssetDatabase：

```text
unity_asset_refresh
```

查找资源：

```json
{
  "filter": "t:Prefab Player",
  "folders": ["Assets/Prefabs"]
}
```

工具：

```text
unity_asset_find
```

加载资源元数据：

```json
{
  "assetPath": "Assets/Prefabs/Player.prefab"
}
```

工具：

```text
unity_asset_load
```

创建文件夹：

```json
{
  "parentPath": "Assets",
  "name": "Generated"
}
```

工具：

```text
unity_asset_create_folder
```

删除资源：

```json
{
  "assetPath": "Assets/Prefabs/Player.prefab"
}
```

工具：

```text
unity_asset_delete
```

资源路径必须位于 `Assets/` 下。

## Bridge Permissions

Unity 菜单：

```text
Tools > Unity 2019 MCP > Bridge Permissions
```

当前开关：

- `Allow Scene Object Delete`：控制 `unity_gameobject_delete` 和 `unity_component_remove`。
- `Allow Script Write`：控制 `unity_script_create`。
- `Allow Asset Delete`：控制 `unity_asset_delete`。

关闭后，对应 MCP Bridge 命令会返回：

```text
OPERATION_BLOCKED
```

重要边界：

这些开关只限制 MCP Bridge 暴露的命令入口，不是 Codex、Shell、Unity UI 或文件系统层面的全局安全边界。即使关闭这些开关，具备其他权限的工具仍可能通过直接编辑文件、执行命令或用户手动操作完成相同目标。

真正的项目保护应依赖：

- Git 提交和回滚策略
- 备份
- 操作系统权限
- 只读工作区
- MCP 客户端审批策略

## 日志与诊断

查看 Bridge 配置：

```text
unity_bridge_get_config
```

返回内容包含：

- `allowSceneDelete`
- `allowDelete`
- `allowScriptWrite`
- `allowAssetDelete`
- `logPath`

查看日志路径：

```text
unity_bridge_get_log_path
```

日志默认写入：

```text
Library/Unity2019Mcp/bridge.log
```

日志会记录命令接收、完成和失败信息。

## 常见问题

### MCP 客户端连接不上 Unity

检查：

1. Unity Editor 是否已打开。
2. Unity Console 是否有编译错误。
3. Bridge 是否已启动。
4. `unity_health` 是否可返回。
5. `Library/Unity2019Mcp/bridge.log` 是否有启动记录。

可以在 Unity 菜单中手动执行：

```text
Tools > Unity 2019 MCP > Start Bridge
```

### 端口 8765 被占用

Bridge 会自动尝试 `8766-8775`。

MCP Server 默认 `UNITY_MCP_AUTO_DETECT=true`，会扫描本地端口并连接可用 Bridge。

如果关闭了自动探测，需要手动设置：

```text
UNITY_MCP_BRIDGE_URL=http://127.0.0.1:8766
```

### 创建脚本后暂时无法挂载

Unity 编译脚本是异步过程。建议：

1. 检查 Unity Console 是否有编译错误。
2. 调用 `unity_script_attach` 时设置更长的 `compileTimeoutMs`。
3. 查看工具返回的结构化错误 details。
4. 查看 `Library/Unity2019Mcp/bridge.log`。

### 组件类型找不到或冲突

建议优先使用完整类型名，例如：

```text
UnityEngine.Rigidbody
Namespace.MyBehaviour
```

如果短名称匹配多个类型，工具会返回 `TYPE_AMBIGUOUS`。

### 属性路径不知道怎么写

`component.getProperty` 和 `component.setProperty` 使用 Unity `SerializedProperty.propertyPath`。

常见路径示例：

```text
m_Enabled
m_FieldOfView
m_LocalPosition
m_Materials.Array.data[0]
```

复杂组件建议先通过 Unity Inspector、调试脚本或已有序列化结构确认 property path。

## 推荐使用顺序

新项目接入时建议按以下顺序验证：

1. `unity_health`
2. `unity_project_get_info`
3. `unity_hierarchy_list`
4. `unity_gameobject_create`
5. `unity_transform_set`
6. `unity_component_add`
7. `unity_script_create`
8. `unity_script_attach`
9. `unity_scene_save_as`
10. `unity_prefab_create`

完成以上验证后，再使用 Asset、Inspector 属性和批量场景修改能力。

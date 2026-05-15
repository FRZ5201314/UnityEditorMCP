# Unity 2019.4 LTS MCP 实现方案

## 目标

实现一个明确适配 Unity 2019.4 LTS 的 MCP，用于让支持 MCP 的 AI 客户端稳定地操作 Unity Editor。

核心目标不是只做文件操作，而是提供 Unity Editor 场景级能力：

- 查询当前工程、Unity 版本、打开场景、选中对象。
- 查询 Hierarchy 中的 GameObject。
- 创建、删除、重命名 GameObject。
- 修改 Transform。
- 添加、移除、查询 Component。
- 将脚本组件挂载到 GameObject。
- 创建、打开、保存 Scene。
- 创建、读取、写入脚本文件，并触发 AssetDatabase 刷新。
- 后续扩展 Prefab、Material、Asset、Build Settings 等能力。

## 非目标

第一阶段不做以下能力：

- 不执行任意 C# 代码。
- 不开放任意文件系统读写。
- 不直接修改 Unity 内部序列化文件。
- 不依赖 Unity 2020+ 才有的 API。
- 不要求 Unity Package Manager 的现代化功能。
- 不默认支持 Play Mode 自动化测试。

这些能力可以在后续作为可选模块扩展。

## 总体架构

采用两段式架构：

```text
AI Client
  |
  | MCP stdio / SSE
  v
MCP Server
  |
  | HTTP JSON
  v
Unity Editor Bridge
  |
  | UnityEditor API
  v
Unity 2019.4 LTS Project
```

### MCP Server

运行在 Unity 外部，负责：

- 对外暴露 MCP tools。
- 校验 MCP tool 参数。
- 将 MCP 请求转换为 Unity Bridge HTTP 请求。
- 将 Unity 返回结果转换为 MCP tool result。
- 管理 Unity Bridge 地址、超时、错误格式。

建议技术栈：

- Node.js + TypeScript。
- MCP TypeScript SDK。
- 输出兼容常见 MCP 客户端的 stdio server。

### Unity Editor Bridge

作为 Unity 工程内的 Editor 插件存在，负责：

- 在 Unity Editor 内启动本地 HTTP 服务。
- 接收 MCP Server 的 JSON 请求。
- 在主线程调用 UnityEditor / UnityEngine API。
- 返回结构化 JSON。
- 管理 Editor 生命周期、异常捕获、AssetDatabase 刷新、Undo 记录、Dirty 标记。

建议位置：

```text
UnityProject/
  Assets/
    Editor/
      Unity2019Mcp/
        Bridge/
        Commands/
        Models/
        Utils/
```

## Unity 2019.4 兼容性原则

Unity 2019.4 LTS 的关键约束：

- 默认 C# 语言能力较旧，应避免使用现代 C# 语法。
- 避免使用 `record`、`init`、nullable reference types、switch expression、file-scoped namespace。
- 避免依赖 Unity 2020+ / 2021+ API。
- Editor 代码必须放在 `Assets/Editor` 或 Editor-only asmdef 中。
- 尽量兼容 `.NET 4.x Equivalent`。
- JSON 序列化优先使用 Newtonsoft.Json，但需要处理不同工程中可能不存在或版本不同的问题。

推荐 C# 代码风格：

- 使用 C# 7.3 以内语法。
- 显式 class DTO，不使用 record。
- 使用普通属性或字段。
- 使用 `try/catch` 包裹每个命令。
- 所有 API 返回统一结果结构。

## Newtonsoft.Json 策略

Unity 2019.4 工程中 Newtonsoft.Json 可能有三种情况：

1. 工程已安装 `com.unity.nuget.newtonsoft-json`。
2. 工程中存在第三方 Newtonsoft.Json DLL。
3. 工程完全没有 Newtonsoft.Json。

建议策略：

- 首选检测并使用 `Newtonsoft.Json`。
- 文档中要求用户安装 `com.unity.nuget.newtonsoft-json`，但实现上保留降级空间。
- 第一阶段可以将 Newtonsoft.Json 作为明确依赖，降低复杂度。
- 后续可加入最小 JSON fallback，或者提供内置 DLL 方案。

Unity 2019.4 中推荐依赖：

```json
{
  "dependencies": {
    "com.unity.nuget.newtonsoft-json": "2.0.0"
  }
}
```

如果目标工程无法安装包，则使用 `Assets/Plugins/Newtonsoft.Json.dll` 作为兼容方案，但要避免和现有 DLL 冲突。

## 通信协议

MCP Server 和 Unity Bridge 之间使用 HTTP JSON。

默认地址：

```text
http://127.0.0.1:8765
```

基础接口：

```text
GET  /health
POST /command
```

请求结构：

```json
{
  "id": "request-id",
  "command": "gameObject.create",
  "params": {
    "name": "Cube",
    "parentPath": null
  }
}
```

成功响应：

```json
{
  "ok": true,
  "id": "request-id",
  "result": {
    "instanceId": 12345,
    "path": "Cube"
  },
  "error": null
}
```

失败响应：

```json
{
  "ok": false,
  "id": "request-id",
  "result": null,
  "error": {
    "code": "OBJECT_NOT_FOUND",
    "message": "GameObject not found: Player",
    "details": null
  }
}
```

## 命令命名规范

采用领域加动作的命名：

```text
project.getInfo
editor.getSelection
scene.getActive
scene.save
scene.open
hierarchy.list
gameObject.create
gameObject.delete
gameObject.rename
gameObject.find
transform.get
transform.set
component.list
component.add
component.remove
component.get
script.create
script.attach
asset.refresh
```

MCP tool 名称建议使用下划线：

```text
unity_project_get_info
unity_hierarchy_list
unity_gameobject_create
unity_transform_set
unity_component_add
unity_script_attach
```

## 第一阶段：最小可用闭环

目标：AI 客户端能确认 Unity 连接，并在当前场景中新建物体。

Unity Bridge：

- 启动本地 HTTP 服务。
- 实现 `/health`。
- 实现 `/command`。
- 实现命令分发器。
- 实现统一 DTO。
- 实现 `project.getInfo`。
- 实现 `scene.getActive`。
- 实现 `hierarchy.list`。
- 实现 `gameObject.create`。
- 实现 `gameObject.delete`。
- 实现 `transform.get`。
- 实现 `transform.set`。

MCP Server：

- stdio server。
- Unity Bridge client。
- 暴露对应 MCP tools。
- 提供连接失败时的清晰错误。

验收标准：

- MCP 客户端可以调用 `unity_project_get_info` 获取 Unity 版本。
- MCP 客户端可以调用 `unity_gameobject_create` 创建空物体。
- MCP 客户端可以调用 `unity_transform_set` 修改位置、旋转、缩放。
- Unity Editor 中能看到对象变化。
- Unity Console 无未捕获异常。

## 第二阶段：组件和脚本挂载

目标：支持给 GameObject 添加 Unity 内置组件和用户脚本组件。

Unity Bridge：

- 实现 `component.list`。
- 实现 `component.add`。
- 实现 `component.remove`。
- 实现 `component.get`。
- 实现 `script.create`。
- 实现 `script.attach`。
- 实现 `asset.refresh`。

组件类型解析策略：

- 支持完整类型名，例如 `UnityEngine.Rigidbody`。
- 支持常见短名，例如 `Rigidbody`、`BoxCollider`。
- 用户脚本通过遍历 loaded assemblies 查找 `MonoBehaviour` 子类。
- 若存在重名类型，返回明确错误并列出候选项。

脚本挂载流程：

1. 创建 `.cs` 文件到 `Assets/...`。
2. 调用 `AssetDatabase.Refresh()`。
3. 等待 Unity 编译完成。
4. 查找脚本类型。
5. 调用 `GameObject.AddComponent(type)`。

Unity 2019 编译等待策略：

- 检查 `EditorApplication.isCompiling`。
- 必要时通过延迟队列等待编译完成。
- MCP Server 侧设置较长超时。
- 对长任务返回 pending 状态可作为后续扩展。

验收标准：

- 能给对象添加 `Rigidbody`。
- 能创建一个继承 `MonoBehaviour` 的脚本。
- 编译完成后能把脚本挂载到指定 GameObject。
- 脚本编译失败时返回明确错误。

## 第三阶段：Scene 和 Prefab

目标：支持基础场景管理和 Prefab 工作流。

Unity Bridge：

- `scene.new`
- `scene.open`
- `scene.save`
- `scene.saveAs`
- `scene.getDirty`
- `prefab.create`
- `prefab.instantiate`
- `prefab.apply`

Unity 2019 API 注意事项：

- 使用 `UnityEditor.SceneManagement.EditorSceneManager`。
- Prefab 操作使用 `PrefabUtility`，避免使用 Unity 2020+ 新增 API。
- 修改对象后调用 `EditorUtility.SetDirty` 和 `EditorSceneManager.MarkSceneDirty`。

验收标准：

- 能创建并保存新场景。
- 能打开已有场景。
- 能从场景对象创建 Prefab。
- 能实例化 Prefab 到当前场景。

## 第四阶段：Asset 和 Inspector 属性

目标：支持更细粒度的资源和组件属性修改。

Unity Bridge：

- `asset.find`
- `asset.load`
- `asset.createFolder`
- `asset.delete`
- `component.setProperty`
- `component.getProperty`

属性修改策略：

- 优先使用 `SerializedObject` / `SerializedProperty`。
- 支持基础类型：int、float、bool、string、Vector2、Vector3、Color、Object reference。
- 对复杂类型先只读或返回不支持。
- 所有修改记录 Undo。

验收标准：

- 能修改组件上的公开字段。
- 能修改常见 Unity 组件属性。
- 能查找资源并作为引用赋值。

## 第五阶段：稳定性与 Bridge 命令权限

目标：降低通过 MCP Bridge 命令误操作的风险，提升复杂项目可用性。

能力：

- 命令白名单。
- 工程根目录限制。
- 文件写入路径限制在 `Assets` 下。
- 部分高风险 MCP Bridge 命令可配置为禁用。
- 操作前自动记录 Undo。
- 可选 dry-run。
- 日志文件。
- Bridge 端口冲突处理。
- MCP Server 自动探测 Bridge。

边界说明：这些配置只限制 MCP Bridge 命令入口，不构成 Codex、Shell、Unity UI 或文件系统层面的全局安全边界。真正的项目保护应依赖 Git、备份、操作系统权限、只读工作区或客户端审批策略。

建议配置文件：

```json
{
  "unityBridge": {
    "host": "127.0.0.1",
    "port": 8765,
    "timeoutMs": 30000
  },
  "bridgePermissions": {
    "allowSceneDelete": true,
    "allowScriptWrite": true,
    "allowAssetDelete": true,
    "assetsRootOnly": true
  }
}
```

## Unity Bridge 目录设计

建议结构：

```text
Assets/Editor/Unity2019Mcp/
  Unity2019Mcp.asmdef
  Bridge/
    McpBridgeServer.cs
    McpHttpListener.cs
    McpCommandDispatcher.cs
  Commands/
    ProjectCommands.cs
    SceneCommands.cs
    HierarchyCommands.cs
    GameObjectCommands.cs
    TransformCommands.cs
    ComponentCommands.cs
    ScriptCommands.cs
    AssetCommands.cs
  Models/
    McpCommandRequest.cs
    McpCommandResponse.cs
    McpError.cs
    GameObjectDto.cs
    ComponentDto.cs
    TransformDto.cs
  Utils/
    JsonUtil.cs
    MainThreadDispatcher.cs
    GameObjectPathUtil.cs
    TypeResolver.cs
    AssetPathUtil.cs
    UnityVersionUtil.cs
```

## MCP Server 目录设计

建议结构：

```text
server/
  package.json
  tsconfig.json
  src/
    index.ts
    unity/
      UnityBridgeClient.ts
      UnityBridgeTypes.ts
    tools/
      projectTools.ts
      sceneTools.ts
      hierarchyTools.ts
      gameObjectTools.ts
      transformTools.ts
      componentTools.ts
      scriptTools.ts
    config.ts
    errors.ts
```

## 错误码

建议统一错误码：

```text
BRIDGE_UNAVAILABLE
INVALID_PARAMS
COMMAND_NOT_FOUND
OBJECT_NOT_FOUND
COMPONENT_NOT_FOUND
TYPE_NOT_FOUND
TYPE_AMBIGUOUS
ASSET_NOT_FOUND
SCRIPT_COMPILE_FAILED
UNITY_COMPILING
OPERATION_FAILED
UNSUPPORTED_UNITY_VERSION
```

## 测试策略

### MCP Server 测试

- TypeScript 单元测试。
- 参数校验测试。
- Unity Bridge client mock 测试。
- 错误转换测试。

### Unity Bridge 测试

Unity 2019 的 Editor 测试环境较慢，建议分层：

- 命令 DTO 和工具类做 EditMode tests。
- 真实 Editor 操作用手动验收场景。
- 后续再补 Unity Test Framework 自动化。

### 手动验收清单

- 打开 Unity 2019.4 LTS 工程。
- Bridge 自动启动或通过菜单启动。
- MCP Server 成功连接。
- 创建 GameObject。
- 修改 Transform。
- 添加 Rigidbody。
- 创建脚本。
- 编译后挂载脚本。
- 保存场景。
- 重启 Unity 后对象和组件仍存在。

## 实施顺序

推荐按以下顺序实现：

1. 初始化仓库结构。
2. 实现 MCP Server 骨架。
3. 实现 Unity Bridge HTTP 服务。
4. 打通 `/health`。
5. 打通 `project.getInfo`。
6. 实现 GameObject 创建和 Hierarchy 查询。
7. 实现 Transform 修改。
8. 实现 Component 添加。
9. 实现脚本创建。
10. 实现脚本挂载。
11. 加入 Scene 保存。
12. 补齐错误码和日志。
13. 编写 README 和接入说明。

## 关键风险

### Unity 主线程

UnityEditor API 必须在主线程调用。HTTP 请求线程不能直接操作 Editor 对象。

处理方式：

- HTTP 层只接收请求。
- 命令投递到主线程队列。
- 在 `EditorApplication.update` 中执行命令。
- HTTP 层等待执行结果或超时。

### 脚本编译异步

脚本写入后 Unity 编译不是同步完成。

处理方式：

- 第一阶段只创建脚本，不立即保证挂载。
- 第二阶段实现等待编译完成后挂载。
- 对长时间编译返回明确超时错误。

### 类型解析

用户输入的组件名可能重名。

处理方式：

- 优先完整类型名。
- 短名匹配多个类型时返回 `TYPE_AMBIGUOUS`。
- 返回候选类型列表。

### Newtonsoft 冲突

不同工程可能已有不同版本 Newtonsoft。

处理方式：

- 第一版要求用户显式安装 `com.unity.nuget.newtonsoft-json`。
- 后续增加检测和说明。
- 避免随意放入重复 DLL。

## 第一版完成定义

第一版可以定义为：

- 支持 Unity 2019.4 LTS。
- MCP Server 可被 Claude Desktop、Cursor 或其他 MCP Client 启动。
- Unity Bridge 能在 Editor 中运行。
- 能创建 GameObject。
- 能修改 Transform。
- 能添加内置组件。
- 能创建脚本并挂载到 GameObject。
- 能保存当前场景。
- 所有失败都有结构化错误。

达到这个标准后，该项目就已经区别于只做文件操作的 Unity MCP，具备真实 Unity Editor 场景操作能力。

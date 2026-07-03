# 第二阶段手动验收

本文档用于验证 Component 与脚本挂载能力是否可用。

## 当前验收结论

- 第一阶段能力已通过测试。
- 第二阶段中“创建脚本后无需手动聚焦 Unity 即可继续完成编译/恢复 Bridge”已通过用户测试。
- 后续如果修改脚本创建、脚本挂载、Bridge 重载恢复逻辑，需要重新执行本文档测试。

## 前置条件

- 目标 Unity 工程已通过 Package Manager 导入 `com.yys.unity-mcp-bridge`。
- Unity Console 无编译错误。
- Bridge 已启动，`unity_health` 可正常返回。
- MCP Server 已构建并在 MCP 客户端中可用。

## 添加内置组件

1. 调用 `unity_gameobject_create` 创建对象：

```json
{
  "name": "MCP_Phase2_Object"
}
```

2. 调用 `unity_component_add` 添加 Rigidbody：

```json
{
  "path": "MCP_Phase2_Object",
  "typeName": "Rigidbody"
}
```

3. 调用 `unity_component_list`：

```json
{
  "path": "MCP_Phase2_Object"
}
```

预期结果：返回组件列表中包含 `Rigidbody`。

## 创建脚本

调用 `unity_script_create`：

```json
{
  "assetPath": "Assets/McpPhase2Behaviour.cs",
  "className": "McpPhase2Behaviour",
  "overwrite": true
}
```

预期结果：

- 目标 Unity 工程的 `Assets/` 下出现 `McpPhase2Behaviour.cs`。
- Unity 开始导入脚本并最终完成编译。即使 Unity Editor 未聚焦，也应该由 Bridge 主动请求导入与编译。
- 脚本编译导致域重载后，Bridge 应自动恢复监听，无需手动执行 Start Bridge。
- Unity Console 无脚本编译错误。

## 挂载脚本

调用 `unity_script_attach`：

```json
{
  "path": "MCP_Phase2_Object",
  "typeName": "McpPhase2Behaviour",
  "compileTimeoutMs": 60000
}
```

预期结果：

- 如果 Unity 正在编译，Bridge 会等待编译完成。
- 如果编译尚未启动，Bridge 会等待目标脚本类型实际出现在 AppDomain 中。
- 编译完成后，`MCP_Phase2_Object` 上出现 `McpPhase2Behaviour` 组件。
- 如果超时，返回 `UNITY_COMPILING`。
- 如果脚本类型不可用，返回 `SCRIPT_COMPILE_FAILED`。
- `SCRIPT_COMPILE_FAILED` 的 details 应包含 `reason`、`typeName`、`timeoutMs`、`isUpdating`、`isCompiling`、`candidateCount`、`candidates`、`recentErrors` 和 `hint`。

## 编译失败诊断

1. 调用 `unity_script_create` 创建一个包含 C# 语法错误的脚本：

```json
{
  "assetPath": "Assets/McpPhase2BrokenBehaviour.cs",
  "className": "McpPhase2BrokenBehaviour",
  "content": "using UnityEngine;\n\npublic class McpPhase2BrokenBehaviour : MonoBehaviour\n{\n    void Start()\n    {\n        Debug.Log(\"broken\")\n    }\n}\n",
  "overwrite": true
}
```

2. 调用 `unity_script_attach`：

```json
{
  "path": "MCP_Phase2_Object",
  "typeName": "McpPhase2BrokenBehaviour",
  "compileTimeoutMs": 60000
}
```

预期结果：

- 返回 `SCRIPT_COMPILE_FAILED`，或者 Unity 仍在导入/编译时返回 `UNITY_COMPILING`。
- 错误文本中包含 details JSON。
- details 中的 `recentErrors` 应包含 Unity Console 捕获到的编译错误；如果目标 Unity 版本未通过 log callback 暴露编译错误，也至少应包含等待状态、候选类型和 `hint`。

## 清理

可调用 `unity_gameobject_delete` 删除测试对象：

```json
{
  "path": "MCP_Phase2_Object"
}
```

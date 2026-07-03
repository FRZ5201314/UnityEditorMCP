# 第三阶段手动验收

本文档用于验证 Scene 与 Prefab 能力是否可用。

## 前置条件

- 目标 Unity 工程已通过 Package Manager 安装 `com.yys.unity-mcp-bridge`。
- Unity Console 无编译错误。
- Bridge 已启动，`unity_health` 可正常返回。
- MCP Server 已构建并在 MCP 客户端中可用。

## Scene 管理

1. 调用 `unity_scene_new` 创建新场景：

```json
{
  "setup": "DefaultGameObjects",
  "mode": "Single"
}
```

预期结果：返回新场景信息，`isLoaded` 为 `true`。

2. 调用 `unity_scene_get_dirty`：

```json
{}
```

预期结果：返回当前场景 dirty 状态。

3. 调用 `unity_scene_save_as`：

```json
{
  "path": "Assets/McpPhase3Scene.unity"
}
```

预期结果：返回 `saved: true`，并在 `Assets/` 下生成场景文件。

4. 调用 `unity_scene_open`：

```json
{
  "path": "Assets/McpPhase3Scene.unity",
  "mode": "Single"
}
```

预期结果：重新打开该场景并返回场景信息。

## Prefab 工作流

1. 调用 `unity_gameobject_create`：

```json
{
  "name": "MCP_Phase3_PrefabSource"
}
```

2. 调用 `unity_prefab_create`：

```json
{
  "path": "MCP_Phase3_PrefabSource",
  "assetPath": "Assets/McpPhase3Prefab.prefab"
}
```

预期结果：返回 prefab 资源路径，并在 `Assets/` 下生成 prefab 文件。

3. 调用 `unity_prefab_instantiate`：

```json
{
  "assetPath": "Assets/McpPhase3Prefab.prefab"
}
```

预期结果：场景中出现 prefab 实例，并返回实例 GameObject 信息。

4. 修改实例，例如调用 `unity_gameobject_rename`：

```json
{
  "path": "MCP_Phase3_PrefabSource",
  "name": "MCP_Phase3_PrefabInstance"
}
```

5. 调用 `unity_prefab_apply`：

```json
{
  "path": "MCP_Phase3_PrefabInstance"
}
```

预期结果：返回 `applied: true`，Prefab 资源被保存。

## 清理

可手动删除以下测试资源：

```text
Assets/McpPhase3Scene.unity
Assets/McpPhase3Prefab.prefab
```

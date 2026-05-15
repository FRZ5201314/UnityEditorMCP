# 第五阶段手动验收

本文档用于验证稳定性与安全能力是否可用。

## 前置条件

- 目标 Unity 工程已通过 Package Manager 导入 `com.yys.unity2019-mcp`。
- Unity Console 无编译错误。
- Bridge 已启动，`unity_health` 可正常返回。
- MCP Server 已构建并在 MCP 客户端中可用。

## Bridge 配置与日志

1. 调用 `unity_bridge_get_config`：

```json
{}
```

预期结果：返回 `allowSceneDelete`、`allowScriptWrite`、`allowAssetDelete` 和 `logPath`。

2. 调用 `unity_bridge_get_log_path`：

```json
{}
```

预期结果：返回 `Library/Unity2019Mcp/bridge.log`。

3. 调用任意工具后检查日志文件。

预期结果：日志中记录命令接收、完成或失败信息。

## 安全开关

1. 在 Unity 菜单中关闭：

```text
Tools > Unity 2019 MCP > Safety > Allow Scene Object Delete
```

2. 调用 `unity_gameobject_delete` 删除任意测试对象。

预期结果：返回 `OPERATION_BLOCKED`。

3. 重新打开 `Allow Scene Object Delete`，再次调用删除。

预期结果：删除恢复正常。

4. 依次验证：

```text
Tools > Unity 2019 MCP > Safety > Allow Script Write
Tools > Unity 2019 MCP > Safety > Allow Asset Delete
```

预期结果：关闭后，`unity_script_create` 和 `unity_asset_delete` 分别返回 `OPERATION_BLOCKED`。

注意：`Allow Asset Delete` 只控制 `unity_asset_delete` 这类项目资源删除，不控制场景 Hierarchy 中的 `unity_gameobject_delete`。场景对象删除由 `Allow Scene Object Delete` 控制。

## 端口回退与自动探测

1. 如果 `8765` 被占用，重启 Unity Editor 或手动执行 Start Bridge。

预期结果：Bridge 自动尝试 `8766-8775` 并在 Console 或日志中记录实际监听地址。

2. MCP Server 使用默认环境变量启动。

预期结果：当默认 `8765` 不可用但 `8766-8775` 有 Bridge 时，MCP Server 会自动探测并继续转发命令。

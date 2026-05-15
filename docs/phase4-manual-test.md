# 第四阶段手动验收

本文档用于验证 Asset 与 Inspector 属性能力是否可用。

## 前置条件

- 目标 Unity 工程已通过 Package Manager 导入 `com.yys.unity2019-mcp`。
- Unity Console 无编译错误。
- Bridge 已启动，`unity_health` 可正常返回。
- MCP Server 已构建并在 MCP 客户端中可用。

## Asset 工具

1. 调用 `unity_asset_create_folder`：

```json
{
  "parentPath": "Assets",
  "name": "McpPhase4"
}
```

预期结果：返回新文件夹路径和 guid。

2. 调用 `unity_asset_find`：

```json
{
  "filter": "McpPhase4",
  "folders": ["Assets"]
}
```

预期结果：返回 assets 列表，包含 `Assets/McpPhase4`。

3. 调用 `unity_asset_load`：

```json
{
  "assetPath": "Assets/McpPhase4"
}
```

预期结果：返回资源路径、guid、名称和类型。

## Inspector 属性

1. 调用 `unity_gameobject_create` 创建对象：

```json
{
  "name": "MCP_Phase4_Object"
}
```

2. 调用 `unity_component_get_property` 读取 Transform 的本地位置：

```json
{
  "path": "MCP_Phase4_Object",
  "typeName": "Transform",
  "propertyPath": "m_LocalPosition"
}
```

预期结果：返回 `propertyType: "Vector3"` 和当前位置。

3. 调用 `unity_component_set_property` 修改 Transform 的本地位置：

```json
{
  "path": "MCP_Phase4_Object",
  "typeName": "Transform",
  "propertyPath": "m_LocalPosition",
  "value": {
    "x": 1,
    "y": 2,
    "z": 3
  }
}
```

预期结果：对象移动到本地坐标 `(1, 2, 3)`，返回更新后的属性值。

4. 调用 `unity_component_add` 添加 `Rigidbody`：

```json
{
  "path": "MCP_Phase4_Object",
  "typeName": "Rigidbody"
}
```

5. 调用 `unity_component_set_property` 修改 Rigidbody 质量：

```json
{
  "path": "MCP_Phase4_Object",
  "typeName": "Rigidbody",
  "propertyPath": "m_Mass",
  "value": 5
}
```

预期结果：Rigidbody 的 Mass 变为 `5`。

## 清理

可调用 `unity_gameobject_delete` 删除测试对象：

```json
{
  "path": "MCP_Phase4_Object"
}
```

可调用 `unity_asset_delete` 删除测试文件夹：

```json
{
  "assetPath": "Assets/McpPhase4"
}
```

# 更新日志

## 0.6.0

- Bridge `/health` 增加 `projectPath`、`productName`、`instanceId` 字段，用于多工程场景下识别 Bridge 所属工程。
- 配合新版 MCP Server 的多工程路由能力：同时打开多个 Unity 工程时，Server 可按工程路径或工程名定位到正确 Bridge。

## 0.5.2

- 将 Unity 菜单 `Safety` 改名为 `Bridge Permissions`，明确这些开关只限制 MCP Bridge 命令入口。
- 更新 `OPERATION_BLOCKED` 文案和文档，避免将 Bridge 命令权限误解为 Codex、Shell、Unity UI 或文件系统层面的全局安全边界。

## 0.5.1

- 将场景对象删除安全开关菜单改名为 `Allow Scene Object Delete`，避免与资源删除开关混淆。
- 在 `asset.delete` 命令内部增加 `allowAssetDelete` 二次拦截，确保资源删除安全开关生效。

## 0.5.0

- 新增第五阶段稳定性能力：Bridge 文件日志、启动端口回退和 MCP Server 本地端口自动探测。
- 新增安全开关：可通过 Unity 菜单关闭删除、脚本写入和资源删除操作，关闭后返回 `OPERATION_BLOCKED`。
- 新增只读工具：`bridge.getConfig`、`bridge.getLogPath`。

## 0.4.0

- 新增第四阶段 Asset 工具：`asset.find`、`asset.load`、`asset.createFolder`、`asset.delete`。
- 新增 Inspector 属性工具：`component.getProperty`、`component.setProperty`，支持基础 `SerializedProperty` 类型读写和资源引用赋值。

## 0.3.0

- 新增第三阶段 Scene 工具：`scene.new`、`scene.open`、`scene.saveAs`、`scene.getDirty`。
- 新增第三阶段 Prefab 工具：`prefab.create`、`prefab.instantiate`、`prefab.apply`。

## 0.2.3

- 增强 `script.attach` 失败诊断，`SCRIPT_COMPILE_FAILED`、`UNITY_COMPILING` 和脚本类型歧义会返回结构化 details。
- `script.attach` 等待期间会记录近期 Unity Console 错误，便于定位脚本编译失败原因。

## 0.2.2

- Bridge 初始化时立即启动监听，并在脚本域重载完成后通过 `DidReloadScripts` 主动恢复监听。
- 调整 `script.attach` 等待逻辑，不再循环强制刷新 AssetDatabase，改为等待导入、编译和目标类型加载状态稳定。

## 0.2.1

- 创建脚本后强制导入脚本资源并请求脚本编译，降低 Unity Editor 未聚焦时编译不推进的概率。
- `script.attach` 等待目标脚本组件类型实际可解析后再尝试挂载，避免编译尚未启动或类型尚未加载时过早失败。

## 0.2.0

- 完善第二阶段脚本挂载流程。
- `script.attach` 支持等待 Unity 编译完成后再挂载脚本组件。
- `script.attach` 支持通过 `compileTimeoutMs` 设置编译等待超时。
- 脚本类型不可用时返回 `SCRIPT_COMPILE_FAILED` 结构化错误。

## 0.1.0

- 初始本地 UPM 包版本。
- 支持 Unity Editor Bridge 自动启动与手动启停。
- 支持第一阶段 MCP 命令：项目信息、场景、Hierarchy、GameObject、Transform、Component、脚本与 AssetDatabase 刷新。

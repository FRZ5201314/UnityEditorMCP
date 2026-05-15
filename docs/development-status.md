# 当前开发进度

本文档记录项目当前状态、已验证能力、已解决问题和后续开发入口。后续继续开发时，优先阅读本文档，再阅读 `docs/phase2-manual-test.md` 和包内 `CHANGELOG.md`。

## 项目约定

- Markdown 文档统一使用中文。
- Git 提交描述统一使用中文。
- Unity 包内 `.meta` 文件必须纳入版本库。
- Shell 命令必须通过 `rtk` 执行。

## 当前版本

- Unity 本地 UPM 包：`com.yys.unity2019-mcp@0.4.0`
- MCP Server：`unity2019-mcp-server@0.4.0`
- Unity 目标版本：Unity 2019.4 LTS
- Bridge 默认地址：`http://127.0.0.1:8765`

## 已完成内容

### 包结构

- Unity Bridge 已迁移为本地 UPM 包：

```text
Packages/com.yys.unity2019-mcp/package.json
```

- MCP Server 位于：

```text
server/
```

### 第一阶段

第一阶段已经完成，并已通过用户手动测试。

已验证链路：

- Unity Bridge `/health`
- Unity Bridge `/command`
- MCP Server 在客户端中配置与调用
- `unity_health`
- `unity_project_get_info`
- `unity_gameobject_create`
- `unity_transform_set`
- `unity_hierarchy_list`

第一阶段结论：MCP 客户端可以通过 MCP Server 操作 Unity Editor 中的场景对象。

### 第二阶段

第二阶段核心能力已经实现，并已验证“创建脚本后无需手动聚焦 Unity 即可继续完成编译/恢复 Bridge”的问题已解决。

已实现工具：

- `unity_component_list`
- `unity_component_add`
- `unity_component_remove`
- `unity_component_get`
- `unity_script_create`
- `unity_script_attach`
- `unity_asset_refresh`

当前实现细节：

- `script.create` 写入 `.cs` 后会导入脚本资源，并请求 Unity 脚本编译。
- `script.attach` 支持 `compileTimeoutMs`。
- `script.attach` 会等待目标脚本类型可解析后再尝试挂载。
- Bridge 在脚本域重载后通过 `DidReloadScripts` 自动恢复监听。
- `script.attach` 失败时会返回结构化 details，包含等待状态、候选类型和近期 Unity Console 错误。

## 已解决问题

### 脚本创建后 Unity 需要手动聚焦才继续编译

历史现象：

- 通过 MCP 创建脚本后，Unity 曾需要手动聚焦窗口才会继续完成编译。
- 或者脚本编译触发域重载后，Bridge/MCP 监听需要 Unity 聚焦后才恢复。

修复记录：

1. `0.2.1`：创建脚本后强制导入资源、请求编译；挂载时等待目标类型加载。
2. `0.2.2`：Bridge 初始化时立即启动，脚本域重载完成后通过 `DidReloadScripts` 主动恢复监听；等待逻辑加入 `EditorApplication.isUpdating`。

当前结论：用户测试确认该问题已经不存在。

## 下一步优先级

### P0：第二阶段收尾

- 增强脚本编译失败诊断。已完成。
- 明确返回 `SCRIPT_COMPILE_FAILED` 的 details。已完成。
- 补充第二阶段测试结果记录。已补充失败诊断验收项，仍建议在 Unity 2019.4 中手动跑一遍。
- 如条件允许，增加 Unity EditMode 测试。

### P1：第三阶段 Scene 与 Prefab

完成第二阶段收尾后，进入 Scene 与 Prefab 能力：

- `scene.new`。已实现。
- `scene.open`。已实现。
- `scene.saveAs`。已实现。
- `scene.getDirty`。已实现。
- `prefab.create`。已实现。
- `prefab.instantiate`。已实现。
- `prefab.apply`。已实现。

第三阶段已完成代码实现，并已由用户手动验证创建场景和 Prefab 正常。

### P2：第四阶段 Asset 与 Inspector 属性

- `asset.find`。已实现。
- `asset.load`。已实现。
- `asset.createFolder`。已实现。
- `asset.delete`。已实现。
- `component.setProperty`。已实现，支持基础 `SerializedProperty` 类型。
- `component.getProperty`。已实现，支持基础 `SerializedProperty` 类型。

第四阶段已完成代码实现，仍需要在 Unity 2019.4 中执行 `docs/phase4-manual-test.md` 手动验收。

### P3：稳定性与安全性

- 命令白名单与配置化安全策略。
- 删除、脚本写入等危险操作的配置开关。
- 日志文件。
- Bridge 端口冲突处理。
- MCP Server 自动探测 Bridge。

## 常用验证命令

MCP Server 类型检查：

```powershell
rtk npm run typecheck
```

MCP Server 构建：

```powershell
rtk npm run build
```

检查 Git 状态：

```powershell
rtk git status --short
```

## 重要文件

- 根说明：`README.md`
- 当前进度：`docs/development-status.md`
- 第二阶段手动验收：`docs/phase2-manual-test.md`
- Unity 包变更日志：`Packages/com.yys.unity2019-mcp/CHANGELOG.md`
- Unity Bridge 入口：`Packages/com.yys.unity2019-mcp/Editor/Bridge/McpBridgeServer.cs`
- HTTP Bridge：`Packages/com.yys.unity2019-mcp/Editor/Bridge/McpHttpListener.cs`
- 脚本命令：`Packages/com.yys.unity2019-mcp/Editor/Commands/ScriptCommands.cs`
- MCP 工具注册：`server/src/tools/registerTools.ts`
- MCP 参数 schema：`server/src/tools/schemas.ts`

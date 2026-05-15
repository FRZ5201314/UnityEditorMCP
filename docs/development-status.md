# 当前开发进度

本文档记录项目当前状态、已验证能力、已知问题和后续开发入口。后续继续开发时，优先阅读本文档，再阅读 `docs/phase2-manual-test.md` 和包内 `CHANGELOG.md`。

## 项目约定

- Markdown 文档统一使用中文。
- Git 提交描述统一使用中文。
- Unity 包内 `.meta` 文件必须纳入版本库。
- Shell 命令必须通过 `rtk` 执行。

## 当前版本

- Unity 本地 UPM 包：`com.yys.unity2019-mcp@0.2.2`
- MCP Server：`unity2019-mcp-server@0.2.0`
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

### 第二阶段已实现能力

当前已实现以下工具：

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
- Bridge 在脚本域重载后通过 `DidReloadScripts` 尝试自动恢复监听。

## 当前待验证问题

用户测试中发现：通过 MCP 创建脚本后，Unity 似乎需要手动聚焦窗口才会继续完成编译，或脚本编译后 Bridge/MCP 监听才恢复。

已针对该问题完成两轮修复：

1. `0.2.1`：创建脚本后强制导入资源、请求编译；挂载时等待目标类型加载。
2. `0.2.2`：Bridge 初始化时立即启动，脚本域重载完成后通过 `DidReloadScripts` 主动恢复监听；等待逻辑加入 `EditorApplication.isUpdating`。

最新 `0.2.2` 仍需要在目标 Unity 工程中重新测试确认。

## 下一步优先级

### P0：验证 `0.2.2`

按 `docs/phase2-manual-test.md` 验证：

1. 创建测试 GameObject。
2. 添加 `Rigidbody`。
3. 调用 `unity_script_create` 创建脚本。
4. 不手动聚焦 Unity，直接调用 `unity_script_attach`。
5. 观察是否能自动完成编译、恢复 Bridge 并挂载脚本。

### P1：如果仍需手动聚焦 Unity

如果 `0.2.2` 仍不能解决，需要改为更稳的异步任务模型：

- 新增 `script.createAndAttach` 或 `job.startScriptAttach`。
- 首次调用只创建脚本并返回 `jobId`。
- Bridge 域重载后从 EditorPrefs 或临时状态文件恢复任务。
- MCP Server 轮询 `job.getStatus`。
- 编译完成后再执行挂载。

原因：Unity 脚本编译会触发域重载，HTTP 请求本身可能跨不过重载过程。长任务模型比单个 HTTP 请求等待更可靠。

### P2：第二阶段收尾

- 增强脚本编译失败诊断。
- 明确返回 `SCRIPT_COMPILE_FAILED` 的 details。
- 补充手动测试记录。
- 如条件允许，增加 Unity EditMode 测试。

### P3：第三阶段

完成第二阶段稳定后，再进入 Scene 与 Prefab：

- `scene.new`
- `scene.open`
- `scene.saveAs`
- `scene.getDirty`
- `prefab.create`
- `prefab.instantiate`
- `prefab.apply`

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

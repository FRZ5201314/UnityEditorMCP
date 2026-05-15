# 更新日志

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

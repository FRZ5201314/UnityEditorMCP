# @luoluo123/unity-mcp-server

这是 Unity MCP 的 MCP stdio server，负责把 MCP tool 调用转发到 Unity Editor Bridge。

## 使用

MCP 客户端推荐通过 `npx` 启动：

```bash
npx -y @luoluo123/unity-mcp-server@0.6.2
```

默认会自动探测本机 `127.0.0.1:8765-8775` 范围内的 Unity Bridge。

运行 server 前，需要先在目标 Unity 工程中通过 UPM Git 安装并启动 `com.yys.unity-mcp-bridge`。

## MCP 配置示例

```json
{
  "type": "stdio",
  "command": "npx",
  "args": [
    "-y",
    "@luoluo123/unity-mcp-server@0.6.2"
  ],
  "env": {
    "UNITY_MCP_AUTO_DETECT": "true",
    "UNITY_MCP_DETECT_HOST": "127.0.0.1",
    "UNITY_MCP_DETECT_PORT_START": "8765",
    "UNITY_MCP_DETECT_PORT_END": "8775",
    "UNITY_MCP_TIMEOUT_MS": "30000"
  },
  "enabled": true
}
```

## 本地开发

```bash
npm install
npm run typecheck
npm run build
```

本地运行：

```bash
node dist/index.js
```

发布前检查 npm 包内容：

```bash
npm pack --dry-run
```

## 环境变量

```text
UNITY_MCP_BRIDGE_URL=http://127.0.0.1:8765
UNITY_MCP_TIMEOUT_MS=30000
UNITY_MCP_AUTO_DETECT=true
UNITY_MCP_DETECT_HOST=127.0.0.1
UNITY_MCP_DETECT_PORT_START=8765
UNITY_MCP_DETECT_PORT_END=8775
UNITY_MCP_PROJECT_PATH=
UNITY_MCP_PROJECT_NAME=
```

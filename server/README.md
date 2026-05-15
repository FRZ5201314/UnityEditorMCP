# unity2019-mcp-server

这是 Unity 2019 MCP 的 MCP stdio server，负责把 MCP tool 调用转发到 Unity Editor Bridge。

## 安装与构建

```bash
npm install
npm run build
```

## 运行

```bash
node dist/index.js
```

默认连接 Unity Bridge：

```text
http://127.0.0.1:8765
```

可通过环境变量覆盖：

```text
UNITY_MCP_BRIDGE_URL=http://127.0.0.1:8765
UNITY_MCP_TIMEOUT_MS=30000
UNITY_MCP_AUTO_DETECT=true
UNITY_MCP_DETECT_HOST=127.0.0.1
UNITY_MCP_DETECT_PORT_START=8765
UNITY_MCP_DETECT_PORT_END=8775
```

运行 server 前，需要先在目标 Unity 工程中通过 Package Manager 导入并启动 `com.yys.unity2019-mcp`。

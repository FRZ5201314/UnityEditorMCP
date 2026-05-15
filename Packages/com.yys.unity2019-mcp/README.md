# Unity 2019 MCP Bridge 包

这是 Unity 2019 MCP 的 Unity Editor Bridge 本地 UPM 包。

## 本地导入

在目标 Unity 2019.4 LTS 工程中执行：

1. 打开 `Window > Package Manager`。
2. 点击左上角 `+`。
3. 选择 `Add package from disk...`。
4. 选择本仓库中的文件：

```text
F:\AIProject\Unity2019MCP\Packages\com.yys.unity2019-mcp\package.json
```

导入后，Bridge 会在 Editor 加载后自动启动，默认监听：

```text
http://127.0.0.1:8765
```

也可以通过菜单手动控制：

```text
Tools > Unity 2019 MCP > Start Bridge
Tools > Unity 2019 MCP > Stop Bridge
```

## 依赖

包依赖 Unity 版 Newtonsoft.Json：

```json
{
  "com.unity.nuget.newtonsoft-json": "2.0.0"
}
```

通过 Package Manager 导入本包时，Unity 会根据包的 `dependencies` 尝试解析该依赖。

## 注意事项

- 本包仅包含 Unity Editor Bridge，不包含 Node.js MCP Server。
- MCP Server 位于仓库根目录的 `server/`。
- Unity Editor API 会在 Unity 主线程执行。
- 脚本文件只能创建在目标工程的 `Assets/` 下，并且必须以 `.cs` 结尾。

#!/usr/bin/env node
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { promises as fs } from "node:fs";
import path from "node:path";
import { loadConfig } from "./config.js";
import { BridgeRegistry } from "./unity/BridgeRegistry.js";
import { UnityBridgeClient } from "./unity/UnityBridgeClient.js";
import { registerTools } from "./tools/registerTools.js";

async function main(): Promise<void> {
  const config = loadConfig();
  const detectUrls = config.autoDetect ? buildDetectUrls(config.detectHost, config.detectPortStart, config.detectPortEnd) : [];
  const cwd = process.cwd();
  const cwdProjectPath = await findProjectFromCwd(cwd);
  const registry = new BridgeRegistry({
    detectUrls,
    probeTimeoutMs: Math.min(config.timeoutMs, 1500),
    bridgeUrl: config.bridgeUrl,
    bridgeUrlExplicit: config.bridgeUrlExplicit,
    projectPath: config.projectPath,
    projectName: config.projectName,
    cwd,
    cwdProjectPath,
  });
  const bridge = new UnityBridgeClient(registry, config.timeoutMs);
  const server = new McpServer({
    name: "unity-mcp",
    version: "0.6.0",
  });

  registerTools(server, bridge);
  await server.connect(new StdioServerTransport());
}

main().catch(error => {
  const message = error instanceof Error ? error.stack ?? error.message : String(error);
  console.error(message);
  process.exit(1);
});

function buildDetectUrls(host: string, start: number, end: number): string[] {
  const urls: string[] = [];
  const first = Math.min(start, end);
  const last = Math.max(start, end);
  for (let port = first; port <= last; port += 1) {
    urls.push(`http://${host}:${port}`);
  }

  return urls;
}

async function findProjectFromCwd(cwd: string): Promise<string | null> {
  let current = path.resolve(cwd);
  for (let i = 0; i < 16; i += 1) {
    if (await isUnityProjectRoot(current)) {
      return current;
    }
    const parent = path.dirname(current);
    if (parent === current) {
      return null;
    }
    current = parent;
  }
  return null;
}

async function isUnityProjectRoot(dir: string): Promise<boolean> {
  try {
    const [assets, projectSettings] = await Promise.all([
      fs.stat(path.join(dir, "Assets")).then(s => s.isDirectory()).catch(() => false),
      fs.stat(path.join(dir, "ProjectSettings")).then(s => s.isDirectory()).catch(() => false),
    ]);
    return assets && projectSettings;
  } catch {
    return false;
  }
}

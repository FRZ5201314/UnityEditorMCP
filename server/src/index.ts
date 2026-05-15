#!/usr/bin/env node
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { loadConfig } from "./config.js";
import { UnityBridgeClient } from "./unity/UnityBridgeClient.js";
import { registerTools } from "./tools/registerTools.js";

async function main(): Promise<void> {
  const config = loadConfig();
  const detectUrls = config.autoDetect ? buildDetectUrls(config.detectHost, config.detectPortStart, config.detectPortEnd) : [];
  const bridge = new UnityBridgeClient(config.bridgeUrl, config.timeoutMs, detectUrls);
  const server = new McpServer({
    name: "unity2019-mcp",
    version: "0.5.0",
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

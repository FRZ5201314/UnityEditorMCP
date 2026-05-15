#!/usr/bin/env node
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { loadConfig } from "./config.js";
import { UnityBridgeClient } from "./unity/UnityBridgeClient.js";
import { registerTools } from "./tools/registerTools.js";

async function main(): Promise<void> {
  const config = loadConfig();
  const bridge = new UnityBridgeClient(config.bridgeUrl, config.timeoutMs);
  const server = new McpServer({
    name: "unity2019-mcp",
    version: "0.1.0",
  });

  registerTools(server, bridge);
  await server.connect(new StdioServerTransport());
}

main().catch(error => {
  const message = error instanceof Error ? error.stack ?? error.message : String(error);
  console.error(message);
  process.exit(1);
});

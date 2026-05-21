export interface ServerConfig {
  bridgeUrl: string;
  bridgeUrlExplicit: boolean;
  timeoutMs: number;
  autoDetect: boolean;
  detectHost: string;
  detectPortStart: number;
  detectPortEnd: number;
  projectPath: string | null;
  projectName: string | null;
}

export function loadConfig(): ServerConfig {
  return {
    bridgeUrl: process.env.UNITY_MCP_BRIDGE_URL ?? "http://127.0.0.1:8765",
    bridgeUrlExplicit: typeof process.env.UNITY_MCP_BRIDGE_URL === "string" && process.env.UNITY_MCP_BRIDGE_URL.length > 0,
    timeoutMs: numberFromEnv("UNITY_MCP_TIMEOUT_MS", 30_000),
    autoDetect: boolFromEnv("UNITY_MCP_AUTO_DETECT", true),
    detectHost: process.env.UNITY_MCP_DETECT_HOST ?? "127.0.0.1",
    detectPortStart: numberFromEnv("UNITY_MCP_DETECT_PORT_START", 8765),
    detectPortEnd: numberFromEnv("UNITY_MCP_DETECT_PORT_END", 8775),
    projectPath: stringFromEnv("UNITY_MCP_PROJECT_PATH"),
    projectName: stringFromEnv("UNITY_MCP_PROJECT_NAME"),
  };
}

function numberFromEnv(name: string, fallback: number): number {
  const value = process.env[name];
  if (!value) {
    return fallback;
  }

  const parsed = Number(value);
  return Number.isFinite(parsed) && parsed > 0 ? parsed : fallback;
}

function boolFromEnv(name: string, fallback: boolean): boolean {
  const value = process.env[name];
  if (!value) {
    return fallback;
  }

  return value !== "0" && value.toLowerCase() !== "false";
}

function stringFromEnv(name: string): string | null {
  const value = process.env[name];
  if (typeof value !== "string") {
    return null;
  }

  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : null;
}

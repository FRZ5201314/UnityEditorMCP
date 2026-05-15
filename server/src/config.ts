export interface ServerConfig {
  bridgeUrl: string;
  timeoutMs: number;
}

export function loadConfig(): ServerConfig {
  return {
    bridgeUrl: process.env.UNITY_MCP_BRIDGE_URL ?? "http://127.0.0.1:8765",
    timeoutMs: numberFromEnv("UNITY_MCP_TIMEOUT_MS", 30_000),
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

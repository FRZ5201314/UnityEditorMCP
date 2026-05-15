import { BridgeError } from "../errors.js";
import type { UnityBridgeRequest, UnityBridgeResponse } from "./UnityBridgeTypes.js";

export class UnityBridgeClient {
  private readonly baseUrl: string;
  private readonly timeoutMs: number;

  public constructor(baseUrl: string, timeoutMs: number) {
    this.baseUrl = baseUrl.replace(/\/$/, "");
    this.timeoutMs = timeoutMs;
  }

  public async health(): Promise<unknown> {
    return this.fetchJson(`${this.baseUrl}/health`, { method: "GET" });
  }

  public async command<T>(command: string, params: Record<string, unknown> = {}): Promise<T> {
    const request: UnityBridgeRequest = {
      id: crypto.randomUUID(),
      command,
      params,
    };

    const response = await this.fetchJson<UnityBridgeResponse<T>>(`${this.baseUrl}/command`, {
      method: "POST",
      headers: {
        "content-type": "application/json",
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      throw new BridgeError(
        response.error?.code ?? "OPERATION_FAILED",
        response.error?.message ?? "Unity bridge command failed.",
        response.error?.details,
      );
    }

    return response.result as T;
  }

  private async fetchJson<T = unknown>(url: string, init: RequestInit): Promise<T> {
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), this.timeoutMs);

    try {
      const response = await fetch(url, { ...init, signal: controller.signal });
      if (!response.ok) {
        throw new BridgeError("BRIDGE_UNAVAILABLE", `Unity bridge returned HTTP ${response.status}.`);
      }

      return (await response.json()) as T;
    } catch (error) {
      if (error instanceof BridgeError) {
        throw error;
      }

      const message = error instanceof Error ? error.message : String(error);
      throw new BridgeError(
        "BRIDGE_UNAVAILABLE",
        `Could not reach Unity bridge at ${this.baseUrl}. Open the Unity project and start Tools > Unity 2019 MCP > Start Bridge. ${message}`,
      );
    } finally {
      clearTimeout(timeout);
    }
  }
}

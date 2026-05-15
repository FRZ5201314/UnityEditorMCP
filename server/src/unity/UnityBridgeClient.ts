import { BridgeError } from "../errors.js";
import type { UnityBridgeRequest, UnityBridgeResponse } from "./UnityBridgeTypes.js";

export class UnityBridgeClient {
  private baseUrl: string;
  private readonly timeoutMs: number;
  private readonly detectUrls: string[];

  public constructor(baseUrl: string, timeoutMs: number, detectUrls: string[] = []) {
    this.baseUrl = baseUrl.replace(/\/$/, "");
    this.timeoutMs = timeoutMs;
    this.detectUrls = detectUrls.map(url => url.replace(/\/$/, "")).filter(url => url !== this.baseUrl);
  }

  public async health(): Promise<unknown> {
    return this.fetchJsonWithDetect("/health", { method: "GET" });
  }

  public async command<T>(command: string, params: Record<string, unknown> = {}): Promise<T> {
    const request: UnityBridgeRequest = {
      id: crypto.randomUUID(),
      command,
      params,
    };

    const response = await this.fetchJsonWithDetect<UnityBridgeResponse<T>>("/command", {
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

  private async fetchJsonWithDetect<T = unknown>(path: string, init: RequestInit): Promise<T> {
    try {
      return await this.fetchJson<T>(`${this.baseUrl}${path}`, init);
    } catch (error) {
      if (!(error instanceof BridgeError) || error.code !== "BRIDGE_UNAVAILABLE") {
        throw error;
      }

      const detected = await this.detectBridge();
      if (!detected) {
        throw error;
      }

      return this.fetchJson<T>(`${this.baseUrl}${path}`, init);
    }
  }

  private async detectBridge(): Promise<boolean> {
    for (const candidate of this.detectUrls) {
      try {
        await this.fetchJson(`${candidate}/health`, { method: "GET" }, Math.min(this.timeoutMs, 1_000), candidate);
        this.baseUrl = candidate;
        return true;
      } catch {
        // Try the next configured bridge URL.
      }
    }

    return false;
  }

  private async fetchJson<T = unknown>(url: string, init: RequestInit, timeoutMs = this.timeoutMs, errorBaseUrl = this.baseUrl): Promise<T> {
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), timeoutMs);

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
        `Could not reach Unity bridge at ${errorBaseUrl}. Open the Unity project and start Tools > Unity 2019 MCP > Start Bridge. ${message}`,
      );
    } finally {
      clearTimeout(timeout);
    }
  }
}

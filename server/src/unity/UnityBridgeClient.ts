import { BridgeError } from "../errors.js";
import type { BridgeRegistry, BridgeSelection } from "./BridgeRegistry.js";
import type { UnityBridgeRequest, UnityBridgeResponse } from "./UnityBridgeTypes.js";

export class UnityBridgeClient {
  private readonly registry: BridgeRegistry;
  private readonly timeoutMs: number;
  private currentSelection: BridgeSelection | null = null;
  private resolvePromise: Promise<BridgeSelection> | null = null;

  public constructor(registry: BridgeRegistry, timeoutMs: number) {
    this.registry = registry;
    this.timeoutMs = timeoutMs;
  }

  public async health(): Promise<unknown> {
    const selection = await this.ensureSelection();
    const url = `${selection.url}/health`;
    try {
      return await this.fetchJson(url, { method: "GET" });
    } catch (error) {
      if (error instanceof BridgeError && error.code === "BRIDGE_UNAVAILABLE") {
        await this.refreshSelection();
        const next = await this.ensureSelection();
        return this.fetchJson(`${next.url}/health`, { method: "GET" });
      }
      throw error;
    }
  }

  public async command<T>(command: string, params: Record<string, unknown> = {}): Promise<T> {
    const request: UnityBridgeRequest = {
      id: crypto.randomUUID(),
      command,
      params,
    };

    const init: RequestInit = {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(request),
    };

    let selection = await this.ensureSelection();
    let response: UnityBridgeResponse<T>;
    try {
      response = await this.fetchJson<UnityBridgeResponse<T>>(`${selection.url}/command`, init);
    } catch (error) {
      if (!(error instanceof BridgeError) || error.code !== "BRIDGE_UNAVAILABLE") {
        throw error;
      }
      await this.refreshSelection();
      selection = await this.ensureSelection();
      response = await this.fetchJson<UnityBridgeResponse<T>>(`${selection.url}/command`, init);
    }

    if (!response.ok) {
      throw new BridgeError(
        response.error?.code ?? "OPERATION_FAILED",
        response.error?.message ?? "Unity bridge command failed.",
        response.error?.details,
      );
    }

    return response.result as T;
  }

  public async listBridges(): Promise<unknown[]> {
    const entries = await this.registry.list(true);
    const current = this.currentSelection;
    return entries.map(entry => ({
      url: entry.url,
      projectPath: entry.projectPath,
      productName: entry.productName,
      instanceId: entry.instanceId,
      unityVersion: entry.unityVersion,
      isCurrent: current ? current.url === entry.url : false,
    }));
  }

  public async selectBridge(filter: { url?: string; projectPath?: string; projectName?: string; instanceId?: string }): Promise<unknown> {
    const selection = await this.registry.select(filter);
    this.currentSelection = selection;
    return {
      url: selection.url,
      reason: selection.reason,
      projectPath: selection.entry?.projectPath ?? null,
      productName: selection.entry?.productName ?? null,
      instanceId: selection.entry?.instanceId ?? null,
      unityVersion: selection.entry?.unityVersion ?? null,
    };
  }

  public async describeCurrent(): Promise<unknown> {
    const selection = await this.ensureSelection();
    return {
      url: selection.url,
      reason: selection.reason,
      projectPath: selection.entry?.projectPath ?? null,
      productName: selection.entry?.productName ?? null,
      instanceId: selection.entry?.instanceId ?? null,
      unityVersion: selection.entry?.unityVersion ?? null,
    };
  }

  private async ensureSelection(): Promise<BridgeSelection> {
    if (this.currentSelection) {
      return this.currentSelection;
    }
    if (!this.resolvePromise) {
      this.resolvePromise = this.registry.resolve();
    }
    try {
      this.currentSelection = await this.resolvePromise;
      return this.currentSelection;
    } finally {
      this.resolvePromise = null;
    }
  }

  private async refreshSelection(): Promise<void> {
    this.currentSelection = null;
    this.resolvePromise = this.registry.resolve(true);
    try {
      this.currentSelection = await this.resolvePromise;
    } finally {
      this.resolvePromise = null;
    }
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
        `Could not reach Unity bridge at ${url}. Open the Unity project and start the bridge from the Tools > Unity MCP window. ${message}`,
      );
    } finally {
      clearTimeout(timeout);
    }
  }
}

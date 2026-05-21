import { BridgeError } from "../errors.js";

export interface BridgeHealth {
  ok: boolean;
  service?: string;
  unityVersion?: string;
  bridgeUrl?: string;
  projectPath?: string;
  productName?: string;
  instanceId?: string;
  config?: unknown;
}

export interface BridgeEntry {
  url: string;
  projectPath: string | null;
  productName: string | null;
  instanceId: string | null;
  unityVersion: string | null;
  rawHealth: BridgeHealth;
}

export interface BridgeSelection {
  url: string;
  reason: string;
  entry: BridgeEntry | null;
}

export interface RegistryOptions {
  detectUrls: string[];
  probeTimeoutMs: number;
  bridgeUrl: string;
  bridgeUrlExplicit: boolean;
  projectPath: string | null;
  projectName: string | null;
  cwd: string;
  cwdProjectPath: string | null;
}

export class BridgeRegistry {
  private readonly options: RegistryOptions;
  private readonly probeUrls: string[];
  private entries: BridgeEntry[] = [];
  private lastDiscoveredAt = 0;
  private discoverPromise: Promise<BridgeEntry[]> | null = null;

  public constructor(options: RegistryOptions) {
    this.options = options;
    const all = new Set<string>();
    all.add(stripTrailingSlash(options.bridgeUrl));
    for (const url of options.detectUrls) {
      all.add(stripTrailingSlash(url));
    }
    this.probeUrls = Array.from(all);
  }

  public async list(force = false): Promise<BridgeEntry[]> {
    if (!force && this.entries.length > 0 && Date.now() - this.lastDiscoveredAt < 1500) {
      return this.entries;
    }
    return this.discover();
  }

  public async resolve(force = false): Promise<BridgeSelection> {
    if (this.options.bridgeUrlExplicit) {
      const url = stripTrailingSlash(this.options.bridgeUrl);
      const entry = await this.probeOne(url);
      return {
        url,
        reason: "UNITY_MCP_BRIDGE_URL is set explicitly.",
        entry,
      };
    }

    const entries = await this.list(force);
    if (entries.length === 0) {
      throw new BridgeError(
        "BRIDGE_UNAVAILABLE",
        `No Unity bridge found in range ${this.probeRange()}. Open the Unity project and ensure the bridge is running via the Tools > Unity 2019 MCP window.`,
      );
    }

    if (this.options.projectPath) {
      const target = normalizePath(this.options.projectPath);
      const match = entries.find(e => e.projectPath && normalizePath(e.projectPath) === target);
      if (match) {
        return { url: match.url, reason: `Matched UNITY_MCP_PROJECT_PATH=${this.options.projectPath}.`, entry: match };
      }
      throw new BridgeError(
        "PROJECT_NOT_FOUND",
        `No bridge matched UNITY_MCP_PROJECT_PATH=${this.options.projectPath}. Candidates: ${describeEntries(entries)}`,
      );
    }

    if (this.options.projectName) {
      const target = this.options.projectName.toLowerCase();
      const match = entries.find(e => e.productName && e.productName.toLowerCase() === target);
      if (match) {
        return { url: match.url, reason: `Matched UNITY_MCP_PROJECT_NAME=${this.options.projectName}.`, entry: match };
      }
      throw new BridgeError(
        "PROJECT_NOT_FOUND",
        `No bridge matched UNITY_MCP_PROJECT_NAME=${this.options.projectName}. Candidates: ${describeEntries(entries)}`,
      );
    }

    if (this.options.cwdProjectPath) {
      const target = normalizePath(this.options.cwdProjectPath);
      const match = entries.find(e => e.projectPath && normalizePath(e.projectPath) === target);
      if (match) {
        return { url: match.url, reason: `Matched current working directory project ${this.options.cwdProjectPath}.`, entry: match };
      }
    }

    if (entries.length === 1) {
      return { url: entries[0].url, reason: "Only one bridge online.", entry: entries[0] };
    }

    throw new BridgeError(
      "BRIDGE_AMBIGUOUS",
      `Multiple Unity bridges online and no selector matched. Set UNITY_MCP_PROJECT_PATH or UNITY_MCP_PROJECT_NAME, or call unity_bridge_select. Candidates: ${describeEntries(entries)}`,
      { candidates: entries.map(toCandidateDto) },
    );
  }

  public async select(filter: { url?: string; projectPath?: string; projectName?: string; instanceId?: string }): Promise<BridgeSelection> {
    const entries = await this.list(true);
    const url = filter.url ? stripTrailingSlash(filter.url) : null;
    const projectPath = filter.projectPath ? normalizePath(filter.projectPath) : null;
    const projectName = filter.projectName ? filter.projectName.toLowerCase() : null;
    const instanceId = filter.instanceId ?? null;

    const match = entries.find(e => {
      if (url && e.url !== url) return false;
      if (projectPath && (!e.projectPath || normalizePath(e.projectPath) !== projectPath)) return false;
      if (projectName && (!e.productName || e.productName.toLowerCase() !== projectName)) return false;
      if (instanceId && e.instanceId !== instanceId) return false;
      return true;
    });

    if (!match) {
      throw new BridgeError(
        "PROJECT_NOT_FOUND",
        `No bridge matches the requested selector. Candidates: ${describeEntries(entries)}`,
        { candidates: entries.map(toCandidateDto) },
      );
    }

    return { url: match.url, reason: "Selected via unity_bridge_select.", entry: match };
  }

  public async probeOne(url: string): Promise<BridgeEntry | null> {
    try {
      const health = await fetchHealth(url, this.options.probeTimeoutMs);
      return entryFromHealth(url, health);
    } catch {
      return null;
    }
  }

  private async discover(): Promise<BridgeEntry[]> {
    if (this.discoverPromise) {
      return this.discoverPromise;
    }

    this.discoverPromise = (async () => {
      const results = await Promise.all(this.probeUrls.map(url => this.probeOne(url)));
      const entries = results.filter((entry): entry is BridgeEntry => entry !== null);
      this.entries = entries;
      this.lastDiscoveredAt = Date.now();
      return entries;
    })();

    try {
      return await this.discoverPromise;
    } finally {
      this.discoverPromise = null;
    }
  }

  private probeRange(): string {
    if (this.probeUrls.length === 0) return "(none)";
    if (this.probeUrls.length === 1) return this.probeUrls[0];
    return `${this.probeUrls[0]} .. ${this.probeUrls[this.probeUrls.length - 1]}`;
  }
}

async function fetchHealth(url: string, timeoutMs: number): Promise<BridgeHealth> {
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), timeoutMs);
  try {
    const response = await fetch(`${url}/health`, { method: "GET", signal: controller.signal });
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}`);
    }
    return (await response.json()) as BridgeHealth;
  } finally {
    clearTimeout(timer);
  }
}

function entryFromHealth(url: string, health: BridgeHealth): BridgeEntry {
  return {
    url,
    projectPath: typeof health.projectPath === "string" ? health.projectPath : null,
    productName: typeof health.productName === "string" ? health.productName : null,
    instanceId: typeof health.instanceId === "string" ? health.instanceId : null,
    unityVersion: typeof health.unityVersion === "string" ? health.unityVersion : null,
    rawHealth: health,
  };
}

function stripTrailingSlash(url: string): string {
  return url.replace(/\/$/, "");
}

function normalizePath(value: string): string {
  return value.replace(/\\/g, "/").replace(/\/+$/, "").toLowerCase();
}

function describeEntries(entries: BridgeEntry[]): string {
  if (entries.length === 0) return "(none)";
  return entries
    .map(e => `[${e.url} ${e.productName ?? "?"} (${e.projectPath ?? "?"})]`)
    .join(", ");
}

export function toCandidateDto(entry: BridgeEntry): Record<string, unknown> {
  return {
    url: entry.url,
    projectPath: entry.projectPath,
    productName: entry.productName,
    instanceId: entry.instanceId,
    unityVersion: entry.unityVersion,
  };
}

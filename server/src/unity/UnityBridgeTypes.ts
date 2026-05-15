export interface UnityBridgeRequest {
  id: string;
  command: string;
  params: Record<string, unknown>;
}

export interface UnityBridgeResponse<T = unknown> {
  ok: boolean;
  id: string;
  result: T | null;
  error: UnityBridgeError | null;
}

export interface UnityBridgeError {
  code: string;
  message: string;
  details: unknown;
}

export interface Vector3Dto {
  x: number;
  y: number;
  z: number;
}

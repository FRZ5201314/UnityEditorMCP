export class BridgeError extends Error {
  public readonly code: string;
  public readonly details: unknown;

  public constructor(code: string, message: string, details?: unknown) {
    super(message);
    this.name = "BridgeError";
    this.code = code;
    this.details = details;
  }
}

export function toErrorText(error: unknown): string {
  if (error instanceof BridgeError) {
    return `${error.code}: ${error.message}`;
  }

  if (error instanceof Error) {
    return error.message;
  }

  return String(error);
}

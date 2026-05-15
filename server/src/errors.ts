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
    if (error.details === undefined || error.details === null) {
      return `${error.code}: ${error.message}`;
    }

    return `${error.code}: ${error.message}\n${JSON.stringify(error.details, null, 2)}`;
  }

  if (error instanceof Error) {
    return error.message;
  }

  return String(error);
}

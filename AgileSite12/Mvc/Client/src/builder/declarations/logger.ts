export interface Logger {
  logError: (message: string) => void;
  logException: (exception: Error) => void;
  logWarning: (message: string) => void;
}

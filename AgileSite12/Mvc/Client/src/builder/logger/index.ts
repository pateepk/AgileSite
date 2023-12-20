/**
 * Provides logging and notifications.
 * @module logger
 */

import { getService } from "@/builder/container";
import { LocalizationService } from "@/builder/declarations";
import { postMessage } from "@/builder/services/post-message";
import { MessageTypes, SERVICE_TYPES } from "@/builder/types";

/**
 * Logs warning message.
 * @param message Message text to be logged.
 */
const logWarning = (message: string): void => {
  postMessage(MessageTypes.MESSAGING_WARNING, message, "*");
};

/**
 * Logs the custom error message.
 * @param message Message text to be logged.
 */
const logError = (message: string): void => {
   postMessage(MessageTypes.MESSAGING_ERROR, message, "*");
};

/**
 * Logs the unexpected error (exception).
 * @param exception Error to be logged.
 */
const logException = (exception: Error): void => {
  // tslint:disable-next-line:no-console
  console.error(exception);
  postMessage(MessageTypes.MESSAGING_EXCEPTION, getService<LocalizationService>(SERVICE_TYPES.LocalizationService).getLocalization("errors.generalerror"), "*");
};

/**
 * Logs the unexpected error (exception) to browser console and sends message to admin UI.
 * @param exception Error to be logged.
 * @param message Message text to be logged.
 */
const logExceptionWithMessage = (exception: Error, message: string): void => {
  // tslint:disable-next-line:no-console
  console.error(exception);
  postMessage(MessageTypes.MESSAGING_EXCEPTION, message, "*");
};

export const logger = {
  logError,
  logException,
  logExceptionWithMessage,
  logWarning,
};

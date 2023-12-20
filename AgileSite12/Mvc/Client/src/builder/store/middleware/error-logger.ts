/**
 * Logs errors from dispatching actions.
 * @module error-logger
 */
import { Middleware } from "@/builder/declarations/store";
import { logger } from "@/builder/logger";

const logErrors: Middleware = (_) => (next) => (action) => {
  try {
    return next(action);
  } catch (err) {
    logger.logException(err);
  }
};

export {
  logErrors,
};

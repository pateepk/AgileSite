import { logger } from "@/builder/logger";
import * as messaging from "@/builder/services/post-message";

import { getService } from "../container";
import { LocalizationService, ThunkServices } from "../declarations";
import { SERVICE_TYPES } from "../types";

/**
 * Returns services which are provided to thunks by redux-thunk middleware.
 */
export const getThunkServices = (): ThunkServices => ({
  logger,
  messaging,
  localizationService: getService<LocalizationService>(SERVICE_TYPES.LocalizationService),
});

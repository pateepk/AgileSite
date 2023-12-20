import { MessageTypes } from "@/builder/types";
import { ThunkAction } from "@/page-builder/declarations";
import * as actions from "./actions";

/**
 * Sets page template properties.
 * @param identifier Identifier of page template whose properties to set.
 * @param properties Properties to be set.
 */
export const setPageTemplateProperties = (identifier: string, properties: object): ThunkAction<Promise<void>> =>
  async (dispatch, _, { logger, messaging }) => {
    try {
      dispatch(actions.updateTemplateProperties(identifier, properties));
      messaging.postMessage(MessageTypes.SAVE_TEMP_CONFIGURATION, null, "*");
    } catch (error) {
      logger.logException(error);
    }
  };

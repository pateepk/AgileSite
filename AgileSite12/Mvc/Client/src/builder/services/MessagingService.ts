import { injectable } from "inversify";
import { Dispatch } from "redux";

import { saveState } from "@/builder/api";
import { getService } from "@/builder/container";
import { LocalizationService, State } from "@/builder/declarations";
import { arrayHelper, stringHelper } from "@/builder/helpers";
import { logger } from "@/builder/logger";
import { postMessage } from "@/builder/services/post-message";
import { restoreDisplayedWidgetVariants } from "@/builder/store/displayed-widget-variants/actions";
import { MessageTypes, SERVICE_TYPES } from "@/builder/types";

@injectable()
export class MessagingService {

  /**
   * Adds event listener for receiving messages.
   * @param getStateFunction Function to retrieve current state.
   * @param dispatch Store dispatch function.
   * @param allowedOrigins List of allowed origins.
   */
  public registerListener(getStateFunction: () => State, dispatch: Dispatch, allowedOrigins: string[]): void {
    window.addEventListener("message", this.makeReceiveMessage(getStateFunction, dispatch, allowedOrigins));
  }

  protected makeReceiveMessage(getState: () => State, dispatch: Dispatch, allowedOrigins: string[]) {
    return async (event: MessageEvent): Promise<void> => {
      if (!event.data) {
        return;
      }

      if (!event.data.hasOwnProperty("msg")) {
        return;
      }

      if (!event.data.msg.startsWith("Kentico.")) {
        return;
      }

      if (!arrayHelper.contains(allowedOrigins, event.origin.toLowerCase())) {
        let message = getService<LocalizationService>(SERVICE_TYPES.LocalizationService).getLocalization("messaging.forbiddenorigin");
        message = stringHelper.format(message, event.origin);
        logger.logError(message);
        return;
      }

      return this.processIncomingMessage(getState, dispatch)(event);
    };
  }

  protected processIncomingMessage(getState: () => State, dispatch: Dispatch) {
    return async (event: MessageEvent): Promise<void> => {
      if (event.data.msg === MessageTypes.SAVE_CONFIGURATION) {
        try {
          if (event.data.contentModified) {
            await saveState(getState(), event.data.guid);
          }

          postMessage(MessageTypes.CONFIGURATION_STORED, JSON.stringify(getState().displayedWidgetVariants), event.origin);
        } catch (err) {
          logger.logException(err);
        }
      }

      if (event.data.msg === MessageTypes.LOAD_DISPLAYED_WIDGET_VARIANTS) {
        const savedDisplayedWidgetVariants = JSON.parse(event.data.data) || {};

        // In case displayed widget variants were received from administration, restore these values
        dispatch(restoreDisplayedWidgetVariants(savedDisplayedWidgetVariants));
      }

      if (event.data.msg === MessageTypes.SAVE_TEMP_CONFIGURATION) {
        if (event.data.contentModified) {
          await saveState(getState(), event.data.guid);
        }
        postMessage(MessageTypes.TEMP_CONFIGURATION_STORED, null, event.origin);
      }
    };
  }
}

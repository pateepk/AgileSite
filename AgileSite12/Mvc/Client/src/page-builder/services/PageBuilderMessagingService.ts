import { injectable } from "inversify";
import { Dispatch } from "redux";

import { MessagingService } from "@/builder/services/MessagingService";
import { postMessage } from "@/builder/services/post-message";
import { State } from "@/builder/store/types";
import { MessageTypes } from "@/builder/types";
import { changeTemplate } from "@/page-builder/api";

@injectable()
export class PageBuilderMessagingService extends MessagingService {

  protected processIncomingMessage(getState: () => State, dispatch: Dispatch) {
    return async (event: MessageEvent): Promise<void> => {
      await super.processIncomingMessage(getState, dispatch)(event);

      if (event.data.msg === MessageTypes.CHANGE_TEMPLATE) {
        await changeTemplate(event.data.template, event.data.guid);
        postMessage(MessageTypes.TEMP_CONFIGURATION_STORED, null, event.origin);
      }
    };
  }
}

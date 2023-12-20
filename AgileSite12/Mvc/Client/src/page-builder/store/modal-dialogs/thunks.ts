import { ThunkAction } from "@/page-builder/declarations";
import { getPropertiesDialogMarkup } from "@/page-builder/services/modal-dialog";

import * as modalDialogActions from "./actions";

/**
 * Retrieves the modal dialog markup.
 * @param dialogIndex Modal dialog index.
 * @param dialogMarkupUrl URL where the dialog markup can be retrieved.
 * @param model Model which should be sent when retrieving dialog markup.
 */
export const fetchModalDialogMarkup = (dialogIndex: number, dialogMarkupUrl: string, model: object): ThunkAction<Promise<void>> =>
  async (dispatch, _, { logger }) => {
    try {
      const propertiesFormMarkup = await getPropertiesDialogMarkup(dialogMarkupUrl, model);

      dispatch(modalDialogActions.updateModalDialogMarkup(dialogIndex, propertiesFormMarkup));
    } catch (error) {
      logger.logException(error);
    }
  };

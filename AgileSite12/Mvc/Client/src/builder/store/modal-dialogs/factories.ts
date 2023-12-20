import { LocalizationService, State, ThunkAction } from "@/builder/declarations";
import { MessageTypes, Theme } from "@/builder/types";

import * as modalDialogActions from "./actions";
import { DialogData, OpenModalDialogOptions } from "./types";

type DialogDataMapper = (options: OpenModalDialogOptions, state: State, localizationService: LocalizationService) => DialogData;

export class OpenModalDialogFactory {
  constructor(private readonly dialogDataCollector: DialogDataMapper) { }

  /**
   * Opens a modal dialog.
   * @param options Modal dialog options.
   */
  public openModalDialog = (options: OpenModalDialogOptions): ThunkAction =>
    (dispatch, getState, { localizationService, messaging }) => {
      const state = getState();
      const { componentIdentifier, dialogType, showFooter, applyButtonText, cancelButtonText, dialogWidth, maximized } = options;
      const dialogData = this.dialogDataCollector(options, state, localizationService);

      // First dialog's theme dictates the theme for all other dialogs, if first dialog's theme is not set,
      // the Widget theme is used, since we expect that the dialog was opened from an inline editor or form component
      const theme = state.modalDialogs.theme || dialogData.theme || Theme.Widget;

      messaging.postMessage(MessageTypes.OPEN_MODAL_DIALOG, null, "*");
      dispatch(modalDialogActions.openModalDialog(
        componentIdentifier,
        dialogType,
        dialogData.title,
        dialogData.markupUrl,
        dialogData.model,
        theme,
        dialogWidth,
        maximized,
        showFooter,
        applyButtonText,
        cancelButtonText,
      ));
    }
}

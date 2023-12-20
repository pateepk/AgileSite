import _cloneDeep from "lodash.clonedeep";

import { CustomModalDialogOptions, LocalizationService, ModalDialogCallbacks, ThunkAction } from "@/builder/declarations";
import { objectHelper } from "@/builder/helpers";
import { MessageTypes, Theme } from "@/builder/types";

import * as modalDialogActions from "./actions";
import { OpenModalDialogFactory } from "./factories";
import { DialogData, ModalDialogType, OpenModalDialogOptions } from "./types";

/**
 * Array of custom modal dialog callbacks. Exported for testing purposes.
 */
export const modalDialogCallbacks: ModalDialogCallbacks[] = [];

/**
 * Validates options, pushes apply and close callbacks to the callback array and dispatches openModalDialog thunk.
 * @param options Options for the to be opened dialog.
 */
export const openDialog = (options: CustomModalDialogOptions): ThunkAction =>
(dispatch, _, { localizationService }) => {
    validateOpenDialogOptions(options);
    const { title, url, data, theme,
      width, maximized, showFooter, applyButtonText, cancelButtonText } = combineWithDefaultOptions(options, localizationService);

    modalDialogCallbacks.push({
      applyCallback: options.applyCallback,
      cancelCallback: options.cancelCallback || (() => null),
    });

    const openModalDialogThunk = new OpenModalDialogFactory(getModalDialogData).openModalDialog;
    dispatch(openModalDialogThunk({
      componentIdentifier: `CustomDialog${modalDialogCallbacks.length}`,
      dialogType: ModalDialogType.Custom,
      dialogTitle: title,
      dialogMarkupUrl: url,
      dialogMarkupModel: data,
      dialogTheme: theme,
      dialogWidth: width,
      maximized,
      showFooter,
      applyButtonText,
      cancelButtonText,
    }));
  };

/**
 * Gets the last applyCallback from the callback array, calls it and if successful dispatches closeModalDialog thunk.
 * If an error is thrown the logger logs it.
 * @param dialogWindow Modal dialog window object.
 */
export const applyDialog = (dialogWindow: Window): ThunkAction<Promise<void>> =>
  async (dispatch, getState, { logger }) => {
    try {
      const applyCallbackResult = await modalDialogCallbacks[modalDialogCallbacks.length - 1].applyCallback(dialogWindow);
      if (applyCallbackResult && applyCallbackResult.closeDialog === false) {
        return;
      }
      const dialogIndex = getState().modalDialogs.dialogs.length - 1;
      dispatch(closeModalDialog(dialogIndex));
      modalDialogCallbacks.pop();
    } catch (ex) {
      logger.logException(ex);
    }
  };

/**
 * Close modal dialog with a registered close callback.
 * @param dialogWindow Modal dialog window object.
 */
export const closeDialog = (dialogWindow: Window): ThunkAction<Promise<void>> =>
  async (dispatch, getState, { logger }) => {
    try {
      await modalDialogCallbacks[modalDialogCallbacks.length - 1].cancelCallback(dialogWindow);
    } catch (ex) {
      logger.logException(ex);
    } finally {
      const dialogIndex = getState().modalDialogs.dialogs.length - 1;
      dispatch(closeModalDialog(dialogIndex));
      modalDialogCallbacks.pop();
    }
  };

/**
 * Closes modal dialog.
 * @param dialogIndex Modal dialog index.
 */
export const closeModalDialog = (dialogIndex: number): ThunkAction =>
  (dispatch, _, { messaging }) => {
    messaging.postMessage(MessageTypes.CLOSE_MODAL_DIALOG, null, "*");
    dispatch(modalDialogActions.closeModalDialog(dialogIndex));
  };

/**
 * Gets model for the last opened custom dialog.
 */
export const getData = (): ThunkAction<object> =>
  (_, getState) => {
    const dialogs = getState().modalDialogs.dialogs;

    // Return null value if there is no dialog opened or dialog is not custom
    if ((dialogs.length <= 0) || (dialogs[dialogs.length - 1].type !== ModalDialogType.Custom)) {
      return null;
    }

    return _cloneDeep(dialogs[dialogs.length - 1].model);
  };

/**
 * Validates options of the to be opened dialog. If unsuccessful a TypeError is thrown.
 * @param options options for the to be opened dialog.
 */
const validateOpenDialogOptions = (options: CustomModalDialogOptions) => {
  if (!options.url || !options.applyCallback) {
    throw new TypeError("Passed options do not have required properties.");
  }
  if (typeof (options.applyCallback) !== "function") {
    throw new TypeError("applyCallback is not an instance of a Function type.");
  }
  if (options.cancelCallback && typeof (options.cancelCallback) !== "function") {
    throw new TypeError("cancelCallback is not an instance of a Function type.");
  }
  if (typeof (options.url) !== "string") {
    throw new TypeError("url is not of the type string.");
  }
  if (options.title && typeof (options.title) !== "string") {
    throw new TypeError("title is not of the type string.");
  }
  if (options.theme && Object.values(Theme).indexOf(options.theme) === -1) {
    throw new TypeError("theme does not match any of the existing dialog themes.");
  }
  if (options.width !== undefined && (typeof options.width !== "string" || !(options.width.endsWith("px") || options.width.endsWith("%")))) {
    throw new TypeError("width must be represented by 'XXXpx' or 'XX%' string values.");
  }
  if (options.maximized !== undefined && typeof options.maximized !== "boolean") {
    throw new TypeError("maximized must be of type boolean.");
  }
  if (options.showFooter !== undefined && typeof (options.showFooter) !== "boolean") {
    throw new TypeError("showFooter must be of type boolean.");
  }
};

/**
 * Combines user options with the defaults and ensures correct default values for optional properties.
 * @param userOptions Dialog options provided by user.
 * @returns Combined options.
 */
const combineWithDefaultOptions = (userOptions: CustomModalDialogOptions, localizationService: LocalizationService): CustomModalDialogOptions => {
  const defaults = {
    showFooter: true,
    width: "66%",
    maximized: false,
    applyButtonText: localizationService.getLocalization("modaldialog.buttons.apply"),
    cancelButtonText: localizationService.getLocalization("modaldialog.buttons.cancel"),
  } as CustomModalDialogOptions;

  return objectHelper.assignDefined(defaults, userOptions);
};

/**
 * Maps dialog options to dialog data.
 * @param dialogOptions Dialog options.
 */
const getModalDialogData = (dialogOptions: OpenModalDialogOptions): DialogData => ({
  title: dialogOptions.dialogTitle,
  markupUrl: dialogOptions.dialogMarkupUrl,
  model: dialogOptions.dialogMarkupModel,
  theme: dialogOptions.dialogTheme,
});

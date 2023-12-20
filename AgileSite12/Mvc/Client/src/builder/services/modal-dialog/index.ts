import { CustomModalDialogOptions, LocalizationService, ModalDialogService, ThunkDispatch } from "@/builder/declarations";
import { applyDialog, closeDialog, getData, openDialog } from "@/builder/store/modal-dialogs/thunks";

/**
 * Registers the modal dialogs API.
 * @param dispatch Store dispatch function.
 * @param localizationService Localization service.
 */
export const registerModalDialogApi = (dispatch: ThunkDispatch, localizationService: LocalizationService) => {
  const kenticoNamespace = window.kentico || {};
  kenticoNamespace.localization = kenticoNamespace.localization || {};
  kenticoNamespace.localization.getString = localizationService.getLocalizationWithoutPrefix;

  const modalDialogService: ModalDialogService = {
    open: (options: CustomModalDialogOptions) => dispatch(openDialog(options)),
    apply: (window: Window) => dispatch(applyDialog(window)),
    cancel: (window: Window) => dispatch(closeDialog(window)),
    getData: () => dispatch(getData()),
  };

  const modalDialog = kenticoNamespace.modalDialog = kenticoNamespace.modalDialog || {};
  Object.assign(modalDialog, modalDialogService);
};

import { BuilderConfig } from "@/builder/builderConfig";
import { LocalizationService, ThunkDispatch } from "@/builder/declarations";
import { openDialog } from "@/builder/store/modal-dialogs/thunks";
import { MediaFilesDialogOptions } from "@/builder/web-components/selectors/media-files-selector/types";

/**
 * Registers the API for media files selector dialog.
 * @param localizationService Localization service.
 */
export const registerMediaFilesSelectorDialogApi = (
  namespace,
  config: BuilderConfig,
  dispatch: ThunkDispatch,
  localizationService: LocalizationService,
) => {
  const mediaFilesSelector = namespace.mediaFilesSelector = namespace.mediaFilesSelector || {};

  mediaFilesSelector.open = (options: MediaFilesDialogOptions) => {
    validateOpenDialogOptions(options);
    const {
      libraryName = "",
      maxFilesLimit = 1,
      allowedExtensions = null,
      selectedValues = [],
      applyCallback,
      maximized = true,
      ...modalDialogOptions } = options;
    const url = `${config.selectors.dialogEndpoints.mediaFilesSelector}&libraryName=${encodeURIComponent(libraryName)}`;

    dispatch(openDialog({
      title: options.title || maxFilesLimit === 1 ?
        localizationService.getLocalizationWithoutPrefix("kentico.components.mediafileselector.dialogtitle.single") :
        localizationService.getLocalizationWithoutPrefix("kentico.components.mediafileselector.dialogtitle.multiple"),
      data: {
        libraryName,
        maxFilesLimit,
        allowedExtensions,
        selectedValues,
      },
      url,
      applyCallback: async (dialogWindow) => {
        const grid = dialogWindow.document.querySelector("kentico-media-files");
        const selectedFiles = grid !== null ? await grid.getSelectedFiles() : null;
        return applyCallback(selectedFiles);
      },
      maximized,
      applyButtonText: localizationService.getLocalizationWithoutPrefix("kentico.components.mediafileselector.dialog.button.apply"),
      ...modalDialogOptions,
    }));
  };
};

/**
 * Validates options of the to be opened dialog. If unsuccessful a TypeError is thrown.
 * @param options options for the to be opened dialog.
 */
const validateOpenDialogOptions = (options: MediaFilesDialogOptions) => {
  if (options === undefined) {
    throw new TypeError("Media files selector needs to have a configuration object defined.");
  }

  if (typeof (options.applyCallback) !== "function") {
    throw new TypeError("The 'applyCallback' parameter must be an instance of a Function type.");
  }

  if (options.libraryName && typeof (options.libraryName) !== "string") {
    throw new TypeError("The 'libraryName' parameter is not of the type string.");
  }

  if (options.maxFilesLimit && typeof (options.maxFilesLimit) !== "number") {
    throw new TypeError("The 'maxFilesLimit' parameter is not of the type number.");
  }
};

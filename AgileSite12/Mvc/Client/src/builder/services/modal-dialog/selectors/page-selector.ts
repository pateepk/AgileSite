import { BuilderConfig } from "@/builder/BuilderConfig";
import { LocalizationService, ModalDialogApplyCallback, ModalDialogOptions, ThunkDispatch } from "@/builder/declarations";
import { objectHelper } from "@/builder/helpers";
import { openDialog } from "@/builder/store/modal-dialogs/thunks";
import { IdentifierMode, SelectedPage, SelectedValue } from "@/builder/web-components/selectors/page-selector/page-selector-types";

/**
 * Registers the page selector API.
 */
export const registerPageSelectorApi = (namespace, config: BuilderConfig, dispatch: ThunkDispatch, localizationService: LocalizationService) => {
  namespace.pageSelector = namespace.pageSelector || {};

  namespace.pageSelector.open = (options: PageSelectorDialogOptions) => {
    validateOptions(options);
    const { identifierMode, selectedValues, applyCallback, rootPath, ...modalDialogOptions } = combineWithDefaultOptions(options, localizationService);

    let dialogUrl = config.selectors.dialogEndpoints.pageSelector;
    if (rootPath !== undefined) {
      dialogUrl += ((dialogUrl.indexOf("?") > -1) ? "&" : "?") + "rootPath=" + rootPath;
    }

    dispatch(openDialog({
      url: dialogUrl,
      applyCallback(dialogWindow) {
        const pageSelector = dialogWindow.document.querySelector("kentico-page-selector");
        const selectedPage = pageSelector.getSelectedPage();
        const selectedPages = [];
        if (selectedPage !== null) {
          selectedPages.push(selectedPage);
        }

        applyCallback(selectedPages);
      },
      data: {
        selectedValues,
        identifierMode,
      },
      ...modalDialogOptions,
    }));
  };
};

const validateOptions = (options: PageSelectorDialogOptions) => {
  if (options === undefined) {
    throw new TypeError("Page selector needs to have a configuration object defined.");
  }

  if (typeof (options.applyCallback) !== "function") {
    throw new TypeError("The 'applyCallback' parameter must be an instance of a Function type.");
  }

  if (options.selectedValues !== undefined) {
    if (options.identifierMode === undefined) {
      throw new TypeError("The 'selectedValues' parameter can be used only together with the 'identifierMode' specified.");
    }

    if (!(options.selectedValues instanceof Array)) {
      throw new TypeError("The 'selectedValues' parameter must be an array.");
    }

    if (options.selectedValues.length && (options.selectedValues[0].identifier === undefined)) {
      throw new TypeError("The 'selectedValues' parameter must contain objects with an 'identifier' variable specified.");
    }

    if (Object.values(IdentifierMode).indexOf(options.identifierMode) === -1) {
      throw new TypeError(`The 'identifierMode' parameter must be one of the following values: ${IdentifierMode.Guid}, ${IdentifierMode.Path}`);
    }
  }

  if (options.rootPath !== undefined) {
    if ((options.rootPath.lastIndexOf("/") === options.rootPath.length - 1) &&  (options.rootPath.length > 1)) {
      throw new TypeError("The 'rootPath' cannot end with a slash character unless it points at the site root.");
    }
  }
};

const combineWithDefaultOptions = (userOptions: PageSelectorDialogOptions, localizationService: LocalizationService) => {
  const defaults = {
    title: localizationService.getLocalizationWithoutPrefix("kentico.components.pageselector.dialogtitle"),
    identifierMode: IdentifierMode.Guid,
    applyButtonText: localizationService.getLocalizationWithoutPrefix("kentico.components.pageselector.dialog.button.apply"),
  } as PageSelectorDialogOptions;

  return objectHelper.assignDefined(defaults, userOptions);
};

/**
 * Represents options of the opened page selector dialog.
 */
export interface PageSelectorDialogOptions extends ModalDialogOptions {
  /**
   * Determines the semantics of the identifier; eg. whether identifier corresponds to "guid" or "path".
   */
  readonly identifierMode?: IdentifierMode;

  /**
   * Array of selected values. Each selected value object contain a property "identifier" that that identifies the selected page.
   * Identifier can be either the page "NodeGuid" or the "NodeAliasPath".
   */
  readonly selectedValues?: SelectedValue[];

  /**
   * Dialog root path. Accepts NodeAliasPath value.
   */
  readonly rootPath?: string;

  /**
   * Function which is called when the apply button on the dialog is clicked.
   * @param pages Selected pages array.
   */
  readonly applyCallback: (pages: SelectedPage[]) => ModalDialogApplyCallback;
}

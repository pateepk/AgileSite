import { getService } from "@/builder/container";
import { InlineEditorHandlers, LocalizationService } from "@/builder/declarations";
import { logger } from "@/builder/logger";
import { SERVICE_TYPES } from "@/builder/types";

const registeredInlineEditors = {};

/**
 * Performs validation and registers inline editor.
 * @param editorName Editor name.
 * @param editorFunctions Editor handlers object.
 */
const registerInlineEditor = (editorName: string, editorFunctions: InlineEditorHandlers): void => {
  try {
    validateEditor(editorName, editorFunctions);
  } catch (err) {
    logger.logError(err.message);
    return;
  }

  registeredInlineEditors[editorName] = editorFunctions;
};

const validateEditor = (editorName: string, editorFunctions: InlineEditorHandlers): void => {
  const localizationService = getService<LocalizationService>(SERVICE_TYPES.LocalizationService);
  if (typeof editorName !== "string") {
    const message = localizationService.getLocalization("inlineeditors.initinvalidname");
    throw new Error(message);
  }

  if (typeof editorFunctions !== "object") {
    const message = localizationService.getLocalization("inlineeditors.wrongtype", editorName);
    throw new Error(message);
  }

  if (!editorFunctions.hasOwnProperty("init")) {
    const message = localizationService.getLocalization("inlineeditors.initmissing", editorName);
    throw new Error(message);
  }

  if (typeof editorFunctions.init !== "function") {
    const message = localizationService.getLocalization("inlineeditors.initwrongtype", editorName);
    throw new Error(message);
  }

  if (registeredInlineEditors[editorName]) {
    const message = localizationService.getLocalization("inlineeditors.duplicateregistration", editorName);
    throw new Error(message);
  }
};

/**
 * Initializes inline editors in specified widget.
 * @param widgetMarkupElement Widget for which inline editors should be initialized.
 * @param widgetProperties Widget properties.
 */
const initializeInlineEditors = (widgetMarkupElement: HTMLElement, widgetProperties: object): void => {
  const inlineEditors = widgetMarkupElement.querySelectorAll("[data-inline-editor]") as NodeListOf<HTMLElement>;
  const localizationService = getService<LocalizationService>(SERVICE_TYPES.LocalizationService);
  const translationService = {
    cultureCode: localizationService.getCultureCode(),
    getString: localizationService.getLocalizationWithoutPrefix,
  };

  for (const editor of inlineEditors) {
    const inlineEditorName = editor.dataset.inlineEditor;
    const inlineEditorHandlers = registeredInlineEditors[inlineEditorName];
    if (!inlineEditorHandlers) {
      const message = localizationService.getLocalization("inlineeditors.notregistered", inlineEditorName);
      logger.logError(message);
      continue;
    }

    const propertyName = editor.dataset.propertyName;
    if (!propertyName || !propertyName.length) {
      const message = localizationService.getLocalization("inlineeditors.missingpropertyname", inlineEditorName);
      logger.logError(message);
      continue;
    }

    const options = {
      editor,
      propertyName,
      localizationService: translationService,
      propertyValue: widgetProperties[propertyName],
    };
    try {
      inlineEditorHandlers.init(options);
    } catch (err) {
      logger.logException(err);
    }
  }
};

/**
 * Invokes dragStart inline editors function for all inline editors in element if they provide this method.
 * @param element Element with inline editors.
 */
const invokeInlineEditorsDragStart = (element: HTMLElement): void => {
  invokeInlineEditorsFunction(element, "dragStart");
};

/**
 * Invokes drop inline editors function for all inline editors in element if they provide this method.
 * @param element Element with inline editors.
 */
const invokeInlineEditorsDrop = (element: HTMLElement): void => {
  invokeInlineEditorsFunction(element, "drop");
};

/**
 * Invokes destroy inline editors function for all inline editors in element if they provide this method.
 * @param element Element with inline editors.
 */
const invokeInlineEditorsDestroy = (element: HTMLElement): void => {
  invokeInlineEditorsFunction(element, "destroy");
};

const invokeInlineEditorsFunction = (element: HTMLElement, eventName: string): void => {
  const inlineEditorElements = element.querySelectorAll("[data-inline-editor]") as NodeListOf<HTMLElement>;

  for (const editorElement of inlineEditorElements) {
    const inlineEditorName = editorElement.dataset.inlineEditor;
    const inlineEditorHandlers = registeredInlineEditors[inlineEditorName];

    if (inlineEditorHandlers.hasOwnProperty(eventName)) {
      const handler = inlineEditorHandlers[eventName];
      const options = {
        editor: editorElement,
      };
      handler.call(handler, options);
    }
  }
};

export {
  registerInlineEditor,
  initializeInlineEditors,
  invokeInlineEditorsDragStart,
  invokeInlineEditorsDrop,
  invokeInlineEditorsDestroy,
};

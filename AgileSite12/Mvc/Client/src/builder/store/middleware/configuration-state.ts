/**
 * Detects changes of page configuration.
 * @module configuration-state
 */
import { AnyAction } from "redux";

import { Middleware } from "@/builder/declarations/store";
import { postMessage } from "@/builder/services/post-message";

import { MessageTypes } from "../../types";

/**
 * Detects changes of page configuration.
 * @param store Redux store with page configuration.
 */
const detectConfigurationChange: Middleware = (store) => (next) => (action: AnyAction) => {
  const {
    editableAreas: previousAreas,
    sections: previousSections,
    pageTemplate: previousTemplate,
    widgets: previousWidgets,
    zones: previousZones,
    widgetVariants: previousWidgetVariants,
  } = store.getState();

  const result = next(action);

  const {
    editableAreas: currentAreas,
    sections: currentSections,
    pageTemplate: currentTemplate,
    widgets: currentWidgets,
    zones: currentZones,
    widgetVariants: currentWidgetVariants,
  } = store.getState();

  if (previousTemplate !== currentTemplate || previousAreas !== currentAreas ||
      previousSections !== currentSections || previousZones !== currentZones ||
      previousWidgets !== currentWidgets || previousWidgetVariants !== currentWidgetVariants) {
    postMessage(MessageTypes.CONFIGURATION_CHANGED, null, "*");
  }

  return result;
};

export {
  detectConfigurationChange,
};

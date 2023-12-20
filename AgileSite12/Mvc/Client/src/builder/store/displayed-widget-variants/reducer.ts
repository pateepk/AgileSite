import _omit from "lodash.omit";
import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { Entities } from "@/builder/declarations";

import * as actions from "../actions";
import { BuilderAction } from "../types";

const INIT_STATE: Entities<string> = {};

const displayedWidgetVariants: Reducer<Entities<string>, BuilderAction> = (state = INIT_STATE, action) => {
  switch (action.type) {
    case getType(actions.addWidget):
      return setDisplayedWidgetVariant(state, action.payload.widget.identifier, action.payload.variant.identifier);

    case getType(actions.removeWidget):
      return _omit(state, action.payload.widget.identifier);

    case getType(actions.addWidgetVariant):
      return setDisplayedWidgetVariant(state, action.payload.widgetIdentifier, action.payload.variant.identifier);

    case getType(actions.selectWidgetVariant):
      return setDisplayedWidgetVariant(state, action.payload.widgetIdentifier, action.payload.variantIdentifier);

    case getType(actions.changeSection):
    case getType(actions.removeSection):
      return _omit(state, action.payload.widgetsToRemove.map((widget) => widget.identifier));

    case getType(actions.restoreDisplayedWidgetVariants):
      return {
        ...state,
        ...action.payload.savedWidgetVariants,
      };

    default:
      return state;
  }
};

const setDisplayedWidgetVariant = (state: Entities<string>, widgetIdentifier: string, variantIdentifier: string) => {
  return {
    ...state,
    [widgetIdentifier]: variantIdentifier,
  };
};

export {
  displayedWidgetVariants,
};

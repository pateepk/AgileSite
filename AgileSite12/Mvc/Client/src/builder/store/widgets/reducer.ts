import _omit from "lodash.omit";
import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { Entities, Widget } from "@/builder/declarations";

import * as actions from "../actions";
import { BuilderAction } from "../types";

const INIT_STATE: Entities<Widget> = {};

export const widgets: Reducer<Entities<Widget>, BuilderAction> = (state = INIT_STATE, action) => {
  switch (action.type) {
    case getType(actions.addWidget):
      return {
        ...state,
        [action.payload.widget.identifier]: action.payload.widget,
      };

    case getType(actions.removeWidget):
      const { [action.payload.widget.identifier]: removedWidget, ...newState } = state;
      return newState;

    case getType(actions.changeSection):
    case getType(actions.removeSection):
      return _omit(state, action.payload.widgetsToRemove.map((widget) => widget.identifier));

    case getType(actions.removeWidgetVariant):
      const originalWidget = state[action.payload.widgetIdentifier];
      const newVariantList = originalWidget.variants.filter((item) => item !== action.payload.variantIdentifier);

      return {
        ...state,
        [originalWidget.identifier]: {
          identifier: originalWidget.identifier,
          type: originalWidget.type,
          conditionType: newVariantList.length > 1 ? originalWidget.conditionType : undefined,
          variants: newVariantList,
        },
      };

    default:
      return state;
  }
};

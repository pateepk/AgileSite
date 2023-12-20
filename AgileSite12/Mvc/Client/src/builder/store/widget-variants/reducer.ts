import _flatmap from "lodash.flatmap";
import _omit from "lodash.omit";
import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { Entities, Widget, WidgetVariant } from "@/builder/declarations";

import * as actions from "../actions";
import { BuilderAction } from "../types";

const INIT_STATE: Entities<WidgetVariant> = {};

export const widgetVariants: Reducer<Entities<WidgetVariant>, BuilderAction> = (state = INIT_STATE, action) => {
  switch (action.type) {
    case getType(actions.addWidget):
    case getType(actions.addWidgetVariant):
      return {
        ...state,
        [action.payload.variant.identifier]: action.payload.variant,
      };

    case getType(actions.removeWidgetVariant):
      const { [action.payload.variantIdentifier]: removedVariant, ...newVariantState } = state;
      return newVariantState;

    case getType(actions.removeWidget):
      return _omit(state, action.payload.widget.variants);

    case getType(actions.updateWidgetVariant):
    {
      const variant = state[action.payload.variantIdentifier];
      return {
        ...state,
        [action.payload.variantIdentifier]: {
          ...variant,
          properties: action.payload.properties,
        },
      };
    }

    case getType(actions.updateWidgetConditionTypeParameters):
    {
      const variant = state[action.payload.variantIdentifier];
      return {
        ...state,
        [action.payload.variantIdentifier]: {
          ...variant,
          name: action.payload.name,
          conditionTypeParameters: action.payload.conditionTypeParameters,
        },
      };
    }

    case getType(actions.changeSection):
    case getType(actions.removeSection):
      const variantsToRemove = _flatmap(action.payload.widgetsToRemove, (widget: Widget) => widget.variants);
      return _omit(state, variantsToRemove);

    default:
      return state;
  }
};

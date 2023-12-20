import _flatMap from "lodash.flatmap";
import _omit from "lodash.omit";
import { combineReducers, Reducer } from "redux";
import { getType } from "typesafe-actions";

import { Entities, SectionMarkup, Widget, WidgetVariantMarkup } from "@/builder/declarations";

import * as actions from "../actions";
import { BuilderAction } from "../types";

const INIT_STATE = {};

const sections: Reducer<Entities<SectionMarkup>, BuilderAction> = (state = INIT_STATE, action) => {
  switch (action.type) {
    case getType(actions.addSection):
      return {
        ...state,
        [action.payload.section.identifier]: action.payload.markup,
      };

    case getType(actions.changeSection):
      return {
        ...state,
        [action.payload.oldSection.identifier]: action.payload.newMarkup,
      };

    case getType(actions.removeSection):
      return _omit(state, action.payload.section.identifier);

    default:
      return state;
  }
};

const variants: Reducer<Entities<WidgetVariantMarkup>> = (state = INIT_STATE, action: BuilderAction) => {
  switch (action.type) {
    case getType(actions.addWidget):
    case getType(actions.addWidgetVariant):
      return {
        ...state,
        [action.payload.variant.identifier]: widgetVariantMarkup(undefined, action),
      };

    case getType(actions.removeWidgetVariant):
      const { [action.payload.variantIdentifier]: removedMarkup, ...newState } = state;
      return newState;

    case getType(actions.changeSection):
    case getType(actions.removeSection):
      return _omit(state, _flatMap(action.payload.widgetsToRemove, (item: Widget) => item.variants));

    case getType(actions.removeWidget):
      return _omit(state, action.payload.widget.variants);

    case getType(actions.updateWidgetVariant):
      return {
        ...state,
        [action.payload.variantIdentifier]: widgetVariantMarkup(state[action.payload.variantIdentifier], action),
      };

    case getType(actions.updateWidgetVariantMarkups):
      return {
        ...state,
        ...multipleWidgetVariantMarkups(action.payload.markups),
      };

      default:
        return state;
  }
};

const widgetVariantMarkup = (state: WidgetVariantMarkup, action: BuilderAction): WidgetVariantMarkup => {
  switch (action.type) {
    case getType(actions.addWidget):
    case getType(actions.addWidgetVariant):
      return {
        markup: action.payload.markup,
        isDirty: false,
      };

    case getType(actions.updateWidgetVariant):
      return {
        ...state,
        markup: action.payload.markup ? action.payload.markup : state.markup,
        isDirty: !action.payload.markup,
      };

    default:
      return state;
  }
};

const multipleWidgetVariantMarkups = (widgetVariantMarkups: { [variantId: string]: string }): Entities<WidgetVariantMarkup> => {
  return Object.keys(widgetVariantMarkups).reduce((variantMarkups, variantId) => {
    variantMarkups[variantId] = {
      isDirty: false,
      markup: widgetVariantMarkups[variantId],
    };
    return variantMarkups;
  }, {});
};

const markups = combineReducers({
  sections,
  variants,
});

export {
  markups,
};

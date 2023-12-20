import _isEqual from "lodash.isequal";
import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { Entities, Widget } from "@/builder/declarations";
import * as actions from "@/builder/store/actions";
import { BuilderAction } from "@/builder/store/types";
import { widgets as baseWidgetsReducer } from "@/builder/store/widgets/reducer";

import { PageBuilderAction } from "@/page-builder/store/types";

import * as pageBuilderActions from "../actions";

export const widgets: Reducer<Entities<Widget>, PageBuilderAction> = (state, action) => {
  state = baseWidgetsReducer(state, action as BuilderAction);

  switch (action.type) {
    case getType(actions.addWidgetVariant):
      {
        const widget = state[action.payload.widgetIdentifier];

        return {
          ...state,
          [action.payload.widgetIdentifier]: {
            ...widget,
            conditionType: action.payload.conditionType || widget.conditionType,
            variants: [
              ...widget.variants,
              action.payload.variant.identifier,
            ],
          },
        };
      }

    case getType(pageBuilderActions.changeVariantsPriority):
      {
        const { widgetIdentifier, variants: newVariants } = action.payload;
        const widget = state[widgetIdentifier];
        const { variants: currentVariants } = widget;

        if (_isEqual(newVariants, currentVariants)) {
          return state;
        } else if (_isEqual([...newVariants].sort(), [...currentVariants.slice(1)].sort())) { // slice out the original variant from the comparison
          return {
            ...state,
            [widgetIdentifier]: {
              ...widget,
              variants: [currentVariants[0], ...newVariants], // add the original variant which is not included in the action
            },
          };
        } else { // When the variants array contains more, less or different variant identifiers than before
          throw new Error("Changing variants priority can only modify their order.");
        }
      }

    default:
      return state;
  }
};

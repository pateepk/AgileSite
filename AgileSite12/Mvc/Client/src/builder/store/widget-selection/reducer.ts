import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { WidgetSelection } from "@/builder/declarations";
import { EntityType } from "@/builder/EntityType";

import * as actions from "../actions";
import { BuilderAction, PopupType } from "../types";

const INIT_STATE: WidgetSelection = { identifier: null, freezeSelection: false, preventClickAway: false };

export const widgetSelection: Reducer<WidgetSelection, BuilderAction> = (state = INIT_STATE, action) => {
  switch (action.type) {
    case getType(actions.selectWidget):
      return {
        ...state,
        identifier: action.payload.widgetIdentifier,
      };

    case getType(actions.startDragging):
      return action.payload.entity !== EntityType.Widget
            ? { ...INIT_STATE }
            : { ...state, identifier: action.payload.itemIdentifier};

    case getType(actions.unselectWidget):
    case getType(actions.removeWidget):
      return {
        ...INIT_STATE,
      };

    case getType(actions.openPopup):
      return action.payload.popupType === PopupType.Personalization ? state : { ...INIT_STATE };

    case getType(actions.addWidget):
      return {
        ...state,
        identifier: action.payload.widget.identifier,
      };

    case getType(actions.freezeWidgetSelection):
      return {
        ...state,
        freezeSelection: true,
      };

    case getType(actions.thawWidgetSelection):
      return {
        ...state,
        freezeSelection: false,
      };

    case getType(actions.disableWidgetClickAway):
      return {
        ...state,
        preventClickAway: true,
      };

    case getType(actions.enableWidgetClickAway):
      return {
        ...state,
        preventClickAway: false,
      };

    default:
      return state;
  }
};

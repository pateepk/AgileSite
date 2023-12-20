import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import * as actions from "../actions";
import { BuilderAction } from "../types";

const INIT_STATE: string = null;

export const highlightedWidget: Reducer<string, BuilderAction> = (state = INIT_STATE, action) => {
  switch (action.type) {
    case getType(actions.highlightWidget):
      return action.payload.widgetIdentifier;

    case getType(actions.dehighlightWidget):
    case getType(actions.openPopup):
    case getType(actions.selectWidget):
    case getType(actions.startDragging):
    case getType(actions.endDragging):
    case getType(actions.addWidget):
      return INIT_STATE;

    default:
      return state;
  }
};

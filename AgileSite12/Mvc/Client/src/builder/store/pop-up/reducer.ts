import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { Popup } from "@/builder/declarations/store";

import * as actions from "../actions";
import { BuilderAction } from "../types";

export const INIT_STATE: Popup = {
  componentIdentifier: null,
  position: null,
  popupType: null,
  listingOffset: null,
  areaIdentifier: null,
  zoneIdentifier: null,
};

export const popup: Reducer<Popup, BuilderAction> = (state = INIT_STATE, action) => {
  switch (action.type) {
    case getType(actions.openPopup):
      return {
        ...state,
        componentIdentifier: action.payload.componentIdentifier,
        position: action.payload.position,
        popupType: action.payload.popupType,
        listingOffset: action.payload.listingOffset,
        areaIdentifier: action.payload.areaIdentifier,
        zoneIdentifier: action.payload.zoneIdentifier,
      };

    case getType(actions.setPopupPosition):
      return {
        ...state,
        position: action.payload.position,
        listingOffset: action.payload.listingOffset,
      };

    case getType(actions.addSection):
    case getType(actions.startDragging):
    case getType(actions.closePopup):
    case getType(actions.changeSection):
    case getType(actions.addWidget):
    case getType(actions.selectWidgetVariant):
    case getType(actions.addWidgetVariant):
    case getType(actions.updateWidgetVariant):
    case getType(actions.updateWidgetConditionTypeParameters):
    case getType(actions.removeWidget):
      return {
        ...INIT_STATE,
      };

    default:
      return state;
  }
};

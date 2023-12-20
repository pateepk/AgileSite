import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { HighlightedSection } from "@/builder/declarations";

import * as actions from "../actions";
import { BuilderAction, PopupType } from "../types";

export const INIT_STATE: HighlightedSection = {
  sectionIdentifier: null,
  headerPosition: null,
  highlightBorder: false,
  freezeHighlight: false,
};

export const highlightedSection: Reducer<HighlightedSection, BuilderAction> = (state = INIT_STATE, action) => {
  switch (action.type) {
    case getType(actions.showSectionHeader):
      return { ...INIT_STATE, sectionIdentifier: action.payload.sectionIdentifier, headerPosition: action.payload.headerPosition };

    case getType(actions.hideSectionHeader):
    case getType(actions.changeSection):
    case getType(actions.startDragging):
      return { ...INIT_STATE };

    case getType(actions.highlightSectionBorder):
      return {
        ...state,
        sectionIdentifier: action.payload.sectionIdentifier || state.sectionIdentifier,
        highlightBorder: true,
      };

    case getType(actions.dehighlightSectionBorder):
      return { ...state, highlightBorder: false };

    case getType(actions.openPopup):
      return action.payload.popupType === PopupType.ChangeSection ? state : { ...INIT_STATE };

    default:
      return state;
  }
};

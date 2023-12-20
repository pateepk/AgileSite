
import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { HighlightedSection } from "@/builder/declarations";
import * as actions from "@/builder/store/actions";
import { highlightedSection as baseReducer } from "@/builder/store/highlighted-section/reducer";
import { BuilderAction, ModalDialogType } from "@/builder/store/types";

import { PageBuilderAction } from "../types";

const highlightedSection: Reducer<HighlightedSection, PageBuilderAction> = (state, action) => {
  state = baseReducer(state, action as BuilderAction);

  switch (action.type) {
    case getType(actions.openModalDialog):
      return action.payload.type !== ModalDialogType.SectionProperties ? state : {
        ...state,
        highlightBorder: true,
        freezeHighlight: true,
      };

    case getType(actions.closeModalDialog):
      return !state.freezeHighlight ? state : {
        ...state,
        highlightBorder: false,
        freezeHighlight: false,
      };

    default:
      return state;
  }
};

export {
  highlightedSection,
};

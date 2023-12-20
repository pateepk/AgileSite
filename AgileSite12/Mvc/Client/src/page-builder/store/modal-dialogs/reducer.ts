import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { ModalDialogsState } from "@/builder/declarations";
import { arrayHelper } from "@/builder/helpers";
import { modalDialogs as baseModalDialogsReducer } from "@/builder/store/modal-dialogs/reducer";
import { BuilderAction } from "@/builder/store/types";

import { PageBuilderAction } from "@/page-builder/store/types";

import * as actions from "../actions";

export const modalDialogs: Reducer<ModalDialogsState, PageBuilderAction> = (state, action) => {
  state = baseModalDialogsReducer(state, action as BuilderAction);

  switch (action.type) {
    case getType(actions.updateModalDialogMarkup):
    {
      const { dialogIndex, markup } = action.payload;

      return {
        ...state,
        dialogs: arrayHelper.replaceItem(state.dialogs, { ...state.dialogs[dialogIndex], markup }, dialogIndex),
      };
    }

    case getType(actions.invalidateModalDialog):
    {
      const { dialogIndex } = action.payload;

      return {
        ...state,
        dialogs: arrayHelper.replaceItem(state.dialogs, { ...state.dialogs[dialogIndex], isValid: false }, dialogIndex),
      };
    }

    default:
      return state;
  }
};

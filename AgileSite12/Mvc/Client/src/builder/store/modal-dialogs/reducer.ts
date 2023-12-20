import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { ModalDialog, ModalDialogsState } from "@/builder/declarations";
import { arrayHelper } from "@/builder/helpers";

import * as actions from "../actions";
import { BuilderAction } from "../types";

const INITIAL_STATE: ModalDialogsState = {
  dialogs: [],
  theme: null,
};

export const modalDialogs: Reducer<ModalDialogsState, BuilderAction> = (state = INITIAL_STATE, action) => {
  switch (action.type) {
    case getType(actions.openModalDialog):
    {
      const { theme, ...modalDialog } = action.payload;
      const dialog: ModalDialog = {
        ...modalDialog,
        index: state.dialogs.length,
        markup: null,
        isValid: true,
      };

      return {
        dialogs: arrayHelper.insertItemIntoArray(state.dialogs, dialog, state.dialogs.length),
        theme,
      };
    }

    case getType(actions.closeModalDialog):
      if (state.dialogs.length === 1) {
        return {...INITIAL_STATE};
      }
      return {
        ...state,
        dialogs: arrayHelper.removeItem(state.dialogs, action.payload.dialogIndex),
      };

    default:
      return state;
  }
};

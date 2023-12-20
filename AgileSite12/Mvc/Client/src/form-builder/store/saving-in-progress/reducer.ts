import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { FormBuilderAction } from "@/form-builder/store/types";

import * as actions from "../actions";

export const savingInProgress: Reducer<boolean, FormBuilderAction> = (state = null, action) => {
  switch (action.type) {
    case getType(actions.showSavingMessage):
      return true;

    case getType(actions.showSavedMessage):
      return false;

    default:
      return state;
  }
};

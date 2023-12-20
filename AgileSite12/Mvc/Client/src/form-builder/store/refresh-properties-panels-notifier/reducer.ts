import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { FormBuilderAction } from "@/form-builder/store/types";

import * as actions from "../actions";

export const refreshPropertiesPanelsNotifier: Reducer<boolean, FormBuilderAction> = (state = null, action) => {
  switch (action.type) {
    case getType(actions.refreshPropertiesPanels):
      return !state;

    default:
      return state;
  }
};

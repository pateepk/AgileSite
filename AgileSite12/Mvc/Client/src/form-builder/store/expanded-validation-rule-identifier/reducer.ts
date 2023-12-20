import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { FormBuilderAction } from "@/form-builder/store/types";

import * as actions from "../actions";

export const expandedValidationRuleIdentifier: Reducer<string, FormBuilderAction> = (state = null, action) => {
  switch (action.type) {
    case getType(actions.expandValidationRule):
      return action.payload.validationRuleIdentifier;

    // After validation rule was added, close all expanded validation rules
    case getType(actions.newValidationRuleAdded):
      return null;

    default:
      return state;
  }
};

import { Reducer } from "redux";
import { getType } from "typesafe-actions";

import { WidgetVariantToValidationRuleBinding } from "@/form-builder/declarations";
import { FormBuilderAction } from "@/form-builder/store/types";

import * as actions from "../actions";

export const newValidationRuleIdentifiers: Reducer<WidgetVariantToValidationRuleBinding[], FormBuilderAction> = (state = [], action) => {
  switch (action.type) {
    // Save passed instanceIdentifer for new Validation rule for given FormComponent(widgetVariant)
    case getType(actions.createNewValidationRuleIdentifier):
      return [...state.filter((x) => x.widgetVariantIdentifier !== action.payload.widgetVariantIdentifier), {
        validationRuleIdentifier: action.payload.identifierForNewValidationRule,
        widgetVariantIdentifier: action.payload.widgetVariantIdentifier,
      }];

    // New validation rule was added so remove already used identifier and save passed identifier for new Validation rule for given FormComponent(widgetVariant)
    case getType(actions.newValidationRuleAdded):
        return [...state.filter((x) => x.validationRuleIdentifier !== action.payload.validationRuleIdentifier), {
          validationRuleIdentifier: action.payload.identifierForNewValidationRule,
          widgetVariantIdentifier: action.payload.widgetVariantIdentifier,
        }];

    default:
      return state;
  }
};

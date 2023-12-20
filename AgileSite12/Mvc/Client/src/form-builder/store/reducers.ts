import { combineReducers } from "redux";

import { baseReducers } from "@/builder/store/reducers";

import { expandedValidationRuleIdentifier } from "@/form-builder/store/expanded-validation-rule-identifier/reducer";
import { newValidationRuleIdentifiers } from "@/form-builder/store/new-validation-rule-identifiers/reducer";
import { refreshPropertiesPanelsNotifier } from "@/form-builder/store/refresh-properties-panels-notifier/reducer";
import { savingInProgress } from "@/form-builder/store/saving-in-progress/reducer";
import { validationRulesMetadata } from "@/form-builder/store/validation-rules-metadata/reducer";

import { FormBuilderConfiguration } from "../declarations";

export const formBuilderReducers = combineReducers({
  ...baseReducers,
  config: (state: FormBuilderConfiguration = null) => state,
  expandedValidationRuleIdentifier,
  newValidationRuleIdentifiers,
  refreshPropertiesPanelsNotifier,
  savingInProgress,
  validationRulesMetadata,
});

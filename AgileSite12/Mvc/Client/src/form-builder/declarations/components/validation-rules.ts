import { WidgetVariantToValidationRuleBinding } from "@/form-builder/declarations";

export interface ValidationRulesComponentState {
  expandedValidationRuleIdentifier: string;
  newValidationRuleIdentifiers: WidgetVariantToValidationRuleBinding[];
}

export interface ValidationRulesComponentActions {
  expandValidationRule;
  createNewValidationRuleIdentifier;
  freezeWidgetSelection;
}

export interface ValidationRulesComponentProperties extends ValidationRulesComponentState, ValidationRulesComponentActions {
}

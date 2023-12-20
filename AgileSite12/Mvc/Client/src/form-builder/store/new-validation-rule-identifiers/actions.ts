import { createAction } from "typesafe-actions";

export const createNewValidationRuleIdentifier = createAction("newValidationRuleIdentifiers/NEW_VALIDATION_RULE_IDENTIFIER", (resolve) =>
  (identifierForNewValidationRule: string, widgetVariantIdentifier: string) => resolve({
    identifierForNewValidationRule,
    widgetVariantIdentifier,
  }),
);

export const newValidationRuleAdded = createAction("newValidationRuleIdentifiers/VALIDATION_RULE_ADDED", (resolve) =>
  (validationRuleIdentifier: string, identifierForNewValidationRule: string, widgetVariantIdentifier: string) => resolve({
    validationRuleIdentifier,
    identifierForNewValidationRule,
    widgetVariantIdentifier,
  }),
);

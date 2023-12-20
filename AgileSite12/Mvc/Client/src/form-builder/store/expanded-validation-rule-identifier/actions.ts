import { createAction } from "typesafe-actions";

export const expandValidationRule = createAction("expandedValidationRuleIdentifier/EXPAND_VALIDATION_RULE", (resolve) =>
  (validationRuleIdentifier: string) => resolve({
    validationRuleIdentifier,
  }),
);

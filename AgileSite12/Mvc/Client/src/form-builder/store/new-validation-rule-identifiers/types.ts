import { ActionType } from "typesafe-actions";

import * as actions from "./actions";

export type CreateNewValidationRuleIdentifierAction = ActionType<typeof actions.createNewValidationRuleIdentifier>;
export type ValidationRuleAddedAction = ActionType<typeof actions.newValidationRuleAdded>;
export type NewValidationRuleIdentifiersAction = ActionType<typeof actions>;

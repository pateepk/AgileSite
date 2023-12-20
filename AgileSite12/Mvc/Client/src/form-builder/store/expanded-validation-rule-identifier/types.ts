import { ActionType } from "typesafe-actions";

import * as actions from "./actions";

export type ExpandValidationRuleAction = ActionType<typeof actions.expandValidationRule>;
export type ExpandedValidationRuleIdentifierAction = ActionType<typeof actions>;

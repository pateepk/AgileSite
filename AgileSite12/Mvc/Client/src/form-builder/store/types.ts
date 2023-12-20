import { StateType } from "typesafe-actions";

import { BuilderAction } from "@/builder/store/types";

import { ExpandedValidationRuleIdentifierAction } from "./expanded-validation-rule-identifier/types";
import { NewValidationRuleIdentifiersAction } from "./new-validation-rule-identifiers/types";
import { formBuilderReducers } from "./reducers";
import { RefreshPropertiesPanelsNotifierAction } from "./refresh-properties-panels-notifier/types";
import { SavingInProgressAction } from "./saving-in-progress/types";

export type FormBuilderAction =
  | BuilderAction
  | ExpandedValidationRuleIdentifierAction
  | NewValidationRuleIdentifiersAction
  | RefreshPropertiesPanelsNotifierAction
  | SavingInProgressAction;

export type FormBuilderState = Readonly<StateType<typeof formBuilderReducers>>;

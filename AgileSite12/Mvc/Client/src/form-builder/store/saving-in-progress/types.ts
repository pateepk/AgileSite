import { ActionType } from "typesafe-actions";

import * as actions from "./actions";

export type ShowSavingMessageAction = ActionType<typeof actions.showSavingMessage>;
export type ShowSavedMessageAction = ActionType<typeof actions.showSavedMessage>;
export type SavingInProgressAction = ActionType<typeof actions>;

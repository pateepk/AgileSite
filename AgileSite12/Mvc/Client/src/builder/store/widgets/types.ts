import { ActionType } from "typesafe-actions";

import * as actions from "./actions";
import { addWidget } from "./thunks";

export type AddWidgetThunk = typeof addWidget;

export type AddWidgetAction = ActionType<typeof actions.addWidget>;
export type RemoveWidgetAction = ActionType<typeof actions.removeWidget>;
export type MoveWidgetAction = ActionType<typeof actions.moveWidget>;
export type WidgetsAction = ActionType<typeof actions>;

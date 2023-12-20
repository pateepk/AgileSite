import { ActionType } from "typesafe-actions";

import * as actions from "./actions";

export type HighlightWidgetAction = ActionType<typeof actions.highlightWidget>;
export type DehighlightWidgetAction = ActionType<typeof actions.dehighlightWidget>;
export type HighlightedWidgetAction = ActionType<typeof actions>;

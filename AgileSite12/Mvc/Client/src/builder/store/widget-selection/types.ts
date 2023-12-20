import { ActionType } from "typesafe-actions";

import * as actions from "./actions";

export type SelectWidgetAction = ActionType<typeof actions.selectWidget>;
export type UnselectWidgetAction = ActionType<typeof actions.unselectWidget>;
export type FreezeWidgetSelectionAction = ActionType<typeof actions.freezeWidgetSelection>;
export type ThawWidgetSelectionAction = ActionType<typeof actions.thawWidgetSelection>;
export type DisableWidgetClickAwayAction = ActionType<typeof actions.disableWidgetClickAway>;
export type EnableWidgetClickAwayAction = ActionType<typeof actions.enableWidgetClickAway>;
export type WidgetSelectionAction = ActionType<typeof actions>;

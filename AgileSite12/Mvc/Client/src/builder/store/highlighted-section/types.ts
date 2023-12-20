import { ActionType } from "typesafe-actions";

import * as actions from "./actions";

export type HideSectionHeaderAction = ActionType<typeof actions.hideSectionHeader>;
export type ShowSectionHeaderAction = ActionType<typeof actions.showSectionHeader>;
export type HighlightSectionBorderAction = ActionType<typeof actions.highlightSectionBorder>;
export type DehighlightSectionBorderAction = ActionType<typeof actions.dehighlightSectionBorder>;
export type HighlightedSectionAction = ActionType<typeof actions>;

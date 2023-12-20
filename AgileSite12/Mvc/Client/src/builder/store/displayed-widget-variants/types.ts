import { ActionType } from "typesafe-actions";

import * as actions from "./actions";

export type SelectWidgetVariantAction = ActionType<typeof actions.selectWidgetVariant>;
export type RestoreDisplayedWidgetVariantsAction = ActionType<typeof actions.restoreDisplayedWidgetVariants>;
export type DisplayedWidgetVariantsAction = ActionType<typeof actions>;

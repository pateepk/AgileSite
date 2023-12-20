import { ActionType } from "typesafe-actions";

import * as actions from "./actions";

export type ChangeVariantsPriority = ActionType<typeof actions.changeVariantsPriority>;
export type WidgetVariantAction = ActionType<typeof actions>;

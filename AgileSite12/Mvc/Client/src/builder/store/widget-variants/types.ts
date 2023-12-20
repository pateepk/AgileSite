import { ActionType } from "typesafe-actions";

import * as actions from "./actions";

export type AddWidgetVariantAction = ActionType<typeof actions.addWidgetVariant>;
export type RemoveWidgetVariantAction = ActionType<typeof actions.removeWidgetVariant>;
export type UpdateWidgetVariantAction = ActionType<typeof actions.updateWidgetVariant>;
export type UpdateWidgetConditionTypeParametersAction = ActionType<typeof actions.updateWidgetConditionTypeParameters>;
export type WidgetVariantsAction = ActionType<typeof actions>;

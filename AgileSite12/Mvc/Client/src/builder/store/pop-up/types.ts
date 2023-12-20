import { ActionType } from "typesafe-actions";

import * as actions from "./actions";
import { SelectItemThunkFactory } from "./factories";

export type OpenPopupAction = ReturnType<typeof actions.openPopup>;
export type ClosePopupAction = ReturnType<typeof actions.closePopup>;
export type SetPopupPositionAction = ReturnType<typeof actions.setPopupPosition>;
export type PopUpAction = ActionType<typeof actions>;
export type SelectItemThunk = typeof SelectItemThunkFactory.prototype.selectItem;

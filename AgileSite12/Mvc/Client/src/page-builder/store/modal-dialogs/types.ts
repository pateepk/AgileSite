import { ActionType } from "typesafe-actions";

import * as actions from "./actions";
import { SubmitPropertiesDialogFormFactory } from "./factories";

export type UpdateModalDialogMarkupAction = ActionType<typeof actions.updateModalDialogMarkup>;
export type InvalidateModalDialogAction = ActionType<typeof actions.invalidateModalDialog>;
export type ModalDialogsAction = ActionType<typeof actions>;
export type SubmitPropertiesDialogFormThunk = typeof SubmitPropertiesDialogFormFactory.prototype.submitPropertiesDialogForm;

import { createAction } from "typesafe-actions";

export const showSavingMessage = createAction("savingInProgress/SHOW_SAVING_MESSAGE");

export const showSavedMessage = createAction("savingInProgress/SHOW_SAVED_MESSAGE");

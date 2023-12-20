import { createAction } from "typesafe-actions";

import { Theme } from "@/builder/types";
import { ModalDialogType } from "./types";

/**
 * 'Open modal dialog' action creator.
 * @param identifier Component identifier.
 * @param type Modal dialog type.
 * @param title Modal dialog title.
 * @param markupUrl URL where the dialog markup should be retrieved.
 * @param model Model which should be sent when retrieving dialog markup.
 */
export const openModalDialog = createAction("modalDialogs/OPEN", (resolve) =>
  (identifier: string, type: ModalDialogType, title: string, markupUrl: string, model: object,
   theme: Theme, width: string, maximized: boolean, showFooter: boolean, applyButtonText: string, cancelButtonText: string) => resolve({
    identifier,
    type,
    title,
    markupUrl,
    model,
    theme,
    width,
    maximized,
    showFooter,
    applyButtonText,
    cancelButtonText,
  }),
);

/**
 * 'Close modal dialog' action creator.
 * @param dialogIndex Modal dialog index.
 */
export const closeModalDialog = createAction("modalDialogs/CLOSE", (resolve) =>
  (dialogIndex: number) => resolve({
    dialogIndex,
  }),
);

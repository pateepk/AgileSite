import { createAction } from "typesafe-actions";

/**
 * 'Update modal dialog markup' action creator.
 * @param dialogIndex Modal dialog index.
 * @param markup Modal dialog markup.
 */
export const updateModalDialogMarkup = createAction("modalDialogs/UPDATE_MARKUP", (resolve) =>
  (dialogIndex: number, markup: string) => resolve({
    dialogIndex,
    markup,
  }),
);

/**
 * 'Invalidate modal dialog' action creator.
 * @param dialogIndex Properties dialog index to be invalidated.
 */
export const invalidateModalDialog = createAction("modalDialogs/INVALIDATE", (resolve) =>
  (dialogIndex: number) => resolve({
    dialogIndex,
  }),
);

import { createAction } from "typesafe-actions";

import { Entities } from "@/builder/declarations";

/**
 * 'Select widget variant' action creator.
 * @param variantIdentifier Identifier of the variant to be selected.
 * @param widgetIdentifier Identifier of the widget whose variant should be changed.
 */
export const selectWidgetVariant = createAction("displayedWidgetVariants/SELECT", (resolve) =>
  (variantIdentifier: string, widgetIdentifier: string) => resolve({
    variantIdentifier,
    widgetIdentifier,
  }),
);

/**
 * 'Restore displayed widget variants' action creator.
 * @param savedWidgetVariants Saved widget variants, which should be restored.
 */
export const restoreDisplayedWidgetVariants = createAction("displayedWidgetVariants/RESTORE", (resolve) =>
  (savedWidgetVariants: Entities<string>) => resolve({
    savedWidgetVariants,
  }),
);

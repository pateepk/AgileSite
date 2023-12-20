import { createAction } from "typesafe-actions";

import { Entities } from "@/builder/declarations";

/**
 * 'Update widget variant markups' action creator.
 * @param markups Mapping of widget variant identifier to its markup.
 */
export const updateWidgetVariantMarkups = createAction("markups/UPDATE_WIDGET_VARIANT_MARKUPS", (resolve) =>
  (markups: Entities<string>) => resolve({
    markups,
  }),
);

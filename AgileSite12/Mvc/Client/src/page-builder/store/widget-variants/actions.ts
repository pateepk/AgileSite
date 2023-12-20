import { createAction } from "typesafe-actions";

/**
 * 'Change variants priority' action creator.
 * @param widgetIdentifier Widget identifier of which the variants priority should be changed.
 * @param variants Variant identifiers without the original variant.
 */
export const changeVariantsPriority = createAction("widgetVariants/CHANGE_PRIORITY", (resolve) =>
  (widgetIdentifier: string, variants: string[]) => resolve({
    widgetIdentifier,
    variants,
  }),
);

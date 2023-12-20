import { createAction } from "typesafe-actions";

import { WidgetVariant } from "@/builder/declarations";

/**
 * 'Add widget variant' action creator.
 * @param variant Variant to be added.
 * @param widgetIdentifier Widget identifier.
 * @param markup Variant markup.
 * @param conditionType Condition type identifier.
 */
export const addWidgetVariant = createAction("widgetVariants/ADD", (resolve) =>
  (variant: WidgetVariant, widgetIdentifier: string, markup: string, conditionType: string) => resolve({
    variant,
    widgetIdentifier,
    markup,
    conditionType,
  }),
);

/**
 * 'Remove widget variant' action creator.
 * @param widgetIdentifier Widget identifier.
 * @param variantIdentifier Variant identifier to be removed.
 */
export const removeWidgetVariant = createAction("widgetVariants/REMOVE", (resolve) =>
  (widgetIdentifier: string, variantIdentifier: string) => resolve({
    widgetIdentifier,
    variantIdentifier,
  }),
);

/**
 * 'Update widget variant' action creator.
 * @param variantIdentifier Identifier of the widget variant which should be updated.
 * @param properties New widget variant properties.
 * @param markup New widget variant markup.
 */
export const updateWidgetVariant = createAction("widgetVariants/UPDATE", (resolve) =>
  (variantIdentifier: string, properties: object, markup?: string) => resolve({
    variantIdentifier,
    properties,
    markup,
  }),
);

/**
 * 'Update widget condition type parameters' action creator.
 * @param variantIdentifier Identifier of the widget variant which should be updated.
 * @param name New widget variant name.
 * @param conditionTypeParameters New condition type parameters of widget variant.
 */
export const updateWidgetConditionTypeParameters = createAction("widgetVariants/UPDATE_WIDGET_CONDITION_TYPE_PARAMETERS", (resolve) =>
  (variantIdentifier: string, name: string, conditionTypeParameters: object) => resolve({
    variantIdentifier,
    name,
    conditionTypeParameters,
  }),
);

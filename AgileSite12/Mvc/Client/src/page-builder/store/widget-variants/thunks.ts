import _cloneDeep from "lodash.clonedeep";
import v4 from "uuid/v4";

import * as api from "@/builder/api";
import { ThunkAction, WidgetVariant } from "@/builder/declarations";
import { selectWidgetVariant } from "@/builder/store/displayed-widget-variants/actions";
import {
  addWidgetVariant as addWidgetVariantAction,
  removeWidgetVariant as removeWidgetVariantAction,
  updateWidgetVariant as updateWidgetVariantAction,
} from "@/builder/store/widget-variants/actions";

/**
 * Adds widget variant. Properties of a new variant are a clone of displayed variant properties.
 * @param widgetIdentifier Widget identifier.
 * @param variantName Variant name.
 * @param conditionType Condition type identifier on which the variant is based on.
 */
const addWidgetVariant = (widgetIdentifier: string, variantName: string, conditionType: string, conditionTypeParameters: object): ThunkAction<Promise<void>> =>
  async (dispatch, getState, { logger }) => {
    try {
      const { widgets, widgetVariants, metadata, markups, displayedWidgetVariants } = getState();
      const widget = widgets[widgetIdentifier];

      const originalVariantIdentifier = displayedWidgetVariants[widgetIdentifier];
      const properties = _cloneDeep(widgetVariants[originalVariantIdentifier].properties);
      const widgetType = metadata.widgets[widget.type];
      const markup = await api.getComponentMarkup(widgetType.markupUrl, properties);

      if (markups.variants[originalVariantIdentifier].isDirty) {
        dispatch(updateWidgetVariantAction(originalVariantIdentifier, properties, markup));
      }

      const variant: WidgetVariant = {
        identifier: v4(),
        name: variantName,
        properties,
        conditionTypeParameters,
      };

      dispatch(addWidgetVariantAction(variant, widgetIdentifier, markup, conditionType));
    } catch (err) {
      logger.logException(err);
    }
  };

/**
 * Removes widget variant and sets displayed widget variant.
 * @param widgetIdentifier Widget identifier.
 * @param removedVariantIdentifier Identifier of variant to be removed.
 */
const removeWidgetVariant = (widgetIdentifier: string, removedVariantIdentifier: string): ThunkAction =>
  async (dispatch, getState) => {
    const { widgets, displayedWidgetVariants } = getState();
    const widgetVariants = widgets[widgetIdentifier].variants;
    const removedVariantIndex = widgetVariants.indexOf(removedVariantIdentifier);
    let displayVariantIdentifier = displayedWidgetVariants[widgetIdentifier];

    if (displayVariantIdentifier === removedVariantIdentifier) {
      // If the variant has successor in the variant list then display that variant; if it doesn't then display the original variant.
      displayVariantIdentifier = widgetVariants[removedVariantIndex + 1] || widgetVariants[0];
    }

    dispatch(selectWidgetVariant(displayVariantIdentifier, widgetIdentifier));
    dispatch(removeWidgetVariantAction(widgetIdentifier, removedVariantIdentifier));
  };

export {
  addWidgetVariant,
  removeWidgetVariant,
};

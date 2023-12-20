import v4 from "uuid/v4";

import * as api from "@/builder/api";
import { ThunkAction, UpdatePropertyEventData, Widget, WidgetVariant } from "@/builder/declarations";

import * as actions from "../actions";

/**
 * Ensures widget markup of the widget and adds it to the zone.
 * This function is intended to be used by specialized builder and should not be used directly.
 * @param widgetType Widget type identifier.
 * @param zoneIdentifier Zone identifier.
 * @param position Target position.
 */
export const addWidget = (widgetType: string, zoneIdentifier: string, position: number): ThunkAction<Promise<void>> =>
  async (dispatch, getState) => {
    dispatch(actions.closePopup());

    let properties: object;
    const addedWidget = getState().metadata.widgets[widgetType];
    if (addedWidget.hasProperties === true) {
      properties = await api.getComponentDefaultProperties(addedWidget.defaultPropertiesUrl);
    }

    const markup = await api.getComponentMarkup(addedWidget.markupUrl, properties);

    const variant: WidgetVariant = {
      identifier: v4(),
      properties,
    };

    const widget: Widget = {
      identifier: v4(),
      type: widgetType,
      variants: [variant.identifier],
    };

    dispatch(actions.addWidget(widget, zoneIdentifier, position, variant, markup));
  };

/**
 * Removes widget from the zone.
 * This function is intended to be used by specialized builder and should not be used directly.
 * @param widgetIdentifier Widget identifier.
 * @param zoneIdentifier Zone identifier.
 */
export const removeWidget = (widgetIdentifier: string, zoneIdentifier: string): ThunkAction =>
  (dispatch, getState) => {
    const widgetToRemove = getState().widgets[widgetIdentifier];
    dispatch(actions.removeWidget(widgetToRemove, zoneIdentifier));
  };

/**
 * Move widget to a new position.
 * This function is intended to be used by specialized builder and should not be used directly.
 * @param widgetIdentifier Widget identifier.
 * @param originalZoneIdentifier Source zone identifier.
 * @param targetZoneIdentifier Target zone identifier.
 * @param position A new position of widget.
 */
export const moveWidget = (widgetIdentifier: string, originalZoneIdentifier: string, targetZoneIdentifier: string, position: number): ThunkAction<Promise<void>> =>
  async (dispatch, getState) => {
    const { displayedWidgetVariants, widgets, widgetVariants, markups, metadata } = getState();
    const widget = widgets[widgetIdentifier];
    const displayedVariantIdentifier = displayedWidgetVariants[widgetIdentifier];
    const markup = markups.variants[displayedVariantIdentifier];
    const widgetMetadata = metadata.widgets[widget.type];

    if (markup.isDirty) {
      const widgetVariantProperties = widgetVariants[displayedVariantIdentifier].properties;
      const markupHtml = await api.getComponentMarkup(widgetMetadata.markupUrl, widgetVariantProperties);
      dispatch(actions.updateWidgetVariantMarkups({
        [displayedVariantIdentifier]: markupHtml,
      }));
    }

    dispatch(actions.moveWidget(widgetIdentifier, originalZoneIdentifier, targetZoneIdentifier, position));
  };

/**
 * Sets widget properties and refreshes its markup.
 * This function is intended to be used by specialized builder and should not be used directly.
 * @param widgetIdentifier Identifier of widget whose properties to set.
 * @param properties Properties to be set.
 */
export const setWidgetProperties = (widgetIdentifier: string, properties: object): ThunkAction<Promise<void>> =>
  async (dispatch, getState) => {
    const { displayedWidgetVariants, widgets, metadata } = getState();
    const widget = widgets[widgetIdentifier];
    const widgetMetadata = metadata.widgets[widget.type];
    const variantIdentifier = displayedWidgetVariants[widgetIdentifier];

    const markup = await api.getComponentMarkup(widgetMetadata.markupUrl, properties);

    dispatch(actions.updateWidgetVariant(variantIdentifier, properties, markup));
  };

/**
 * Sets widget property and widget markup when required.
 * @param widgetIdentifier Identifier of widget whose property to set.
 * @param eventData Event data.
 */
export const setWidgetProperty = (widgetIdentifier: string, eventData: UpdatePropertyEventData): ThunkAction<Promise<void>> =>
  async (dispatch, getState, { logger }) => {
    try {
      const { displayedWidgetVariants, widgets, widgetVariants, metadata } = getState();
      const widget = widgets[widgetIdentifier];
      const widgetMetadata = metadata.widgets[widget.type];
      const variantIdentifier = displayedWidgetVariants[widgetIdentifier];
      const properties = { ...widgetVariants[variantIdentifier].properties, [eventData.name]: eventData.value };
      const markup = eventData.refreshMarkup !== false ? await api.getComponentMarkup(widgetMetadata.markupUrl, properties) : undefined;

      dispatch(actions.updateWidgetVariant(variantIdentifier, properties, markup));
    } catch (err) {
      logger.logException(err);
    }
  };

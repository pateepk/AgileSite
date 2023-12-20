import { createAction } from "typesafe-actions";

/**
 * 'Select widget' action creator.
 * @param widgetIdentifier Selected widget's identifier.
 */
export const selectWidget = createAction("widgetSelection/SELECT", (resolve) =>
  (widgetIdentifier: string) => resolve({
    widgetIdentifier,
  }),
);

/**
 * 'Unselect widget' action creator.
 */
export const unselectWidget = createAction("widgetSelection/UNSELECT");

/**
 * 'Freeze widget selection' action creator.
 */
export const freezeWidgetSelection = createAction("widgetSelection/FREEZE");

/**
 * 'Thaw widget selection' action creator.
 */
export const thawWidgetSelection = createAction("widgetSelection/THAW");

/**
 * 'Disable next widget click-away' action creator.
 */
export const disableWidgetClickAway = createAction("widgetSelection/DISABLE_CLICK_AWAY");

/**
 * 'Enable widget click-away' action creator.
 */
export const enableWidgetClickAway = createAction("widgetSelection/ENABLE_CLICK_AWAY");

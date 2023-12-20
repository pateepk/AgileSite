import { createAction } from "typesafe-actions";

/**
 * 'Highlight widget' action creator.
 * @param widgetIdentifier Highlighted widget's identifier.
 */
export const highlightWidget = createAction("highlightedWidget/HIGHLIGHT_WIDGET", (resolve) =>
  (widgetIdentifier: string) => resolve({
    widgetIdentifier,
  }),
);

/**
 * 'Dehighlight widget' action creator.
 */
export const dehighlightWidget = createAction("highlightedWidget/DEHIGHLIGHT_WIDGET");

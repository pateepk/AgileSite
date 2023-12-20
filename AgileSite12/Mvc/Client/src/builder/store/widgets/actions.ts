import { createAction } from "typesafe-actions";

import { Widget, WidgetVariant } from "@/builder/declarations";

/**
 * 'Add widget' action creator.
 * @param widget Widget to be added.
 * @param zoneIdentifier Target zone identifier.
 * @param widgetMarkup Widget markup.
 * @param position Position in the target zone to insert at.
 */
export const addWidget = createAction("widgets/ADD", (resolve) =>
  (widget: Widget, zoneIdentifier: string, position: number, variant: WidgetVariant, markup: string) => resolve({
    widget,
    zoneIdentifier,
    position,
    variant,
    markup,
  }),
);

/**
 * 'Remove widget' action creator.
 * @param widget Widget to be removed.
 * @param zoneIdentifier Identifier of the zone where the widget is located.
 */
export const removeWidget = createAction("widgets/REMOVE", (resolve) =>
  (widget: Widget, zoneIdentifier: string) => resolve({
    widget,
    zoneIdentifier,
  }),
);

/**
 * 'Move widget' action creator.
 * @param widgetIdentifier Identifier of the widget which should be moved.
 * @param originalZoneIdentifier Identifier of the zone from which the widget is being moved.
 * @param targetZoneIdentifier Identifier of the zone where the widget is being moved.
 * @param position Position of the widget in the target zone.
 */
export const moveWidget = createAction("widgets/MOVE", (resolve) =>
  (widgetIdentifier: string, originalZoneIdentifier: string, targetZoneIdentifier: string, position: number) => resolve({
    widgetIdentifier,
    originalZoneIdentifier,
    targetZoneIdentifier,
    position,
  }),
);

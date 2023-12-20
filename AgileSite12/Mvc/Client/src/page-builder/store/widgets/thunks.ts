import { ThunkAction } from "@/builder/declarations";
import * as widgetsThunks from "@/builder/store/widgets/thunks";

/**
 * Ensures widget markup of the widget and adds it to the zone.
 * @param widgetType Widget type identifier.
 * @param zoneIdentifier Zone identifier.
 */
export const addWidget = (widgetType: string, zoneIdentifier: string, position: number): ThunkAction<Promise<void>> =>
  async (dispatch, getState, services) => {
    try {
      await widgetsThunks.addWidget(widgetType, zoneIdentifier, position)(dispatch, getState, services);
    } catch (err) {
      services.logger.logException(err);
    }
  };

/**
 * Removes widget from a zone.
 * @param widgetIdentifier Identifier of a widget to be removed.
 * @param zoneIdentifier Identifier of the zone where the widget is located.
 */
export const removeWidget = (widgetIdentifier: string, zoneIdentifier: string): ThunkAction =>
  (dispatch, getState, services) => {
    widgetsThunks.removeWidget(widgetIdentifier, zoneIdentifier)(dispatch, getState, services);
  };

/**
 * Moves widget to a new position.
 * @param widgetIdentifier Widget identifier.
 * @param originalZoneIdentifier Source zone identifier.
 * @param targetZoneIdentifier Target zone identifier.
 * @param position A new position of widget.
 */
export const moveWidget = (widgetIdentifier: string, originalZoneIdentifier: string, targetZoneIdentifier: string, position: number): ThunkAction<Promise<void>> =>
  async (dispatch, getState, services) => {
    try {
      await widgetsThunks.moveWidget(widgetIdentifier, originalZoneIdentifier, targetZoneIdentifier, position)(dispatch, getState, services);
    } catch (err) {
      services.logger.logException(err);
    }
  };

/**
 * Sets widget properties and refreshes its markup.
 * @param widgetIdentifier Identifier of widget whose properties to set.
 * @param properties Properties to be set.
 */
export const setWidgetProperties = (widgetIdentifier: string, properties: object): ThunkAction<Promise<void>> =>
  async (dispatch, getState, services) => {
    try {
      await widgetsThunks.setWidgetProperties(widgetIdentifier, properties)(dispatch, getState, services);
    } catch (error) {
      services.logger.logException(error);
    }
  };

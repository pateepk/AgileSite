import { builderConfig } from "@/builder/api/client";
import { freezeWidgetSelection, thawWidgetSelection } from "@/builder/store/actions";
import * as widgetsThunks from "@/builder/store/widgets/thunks";

import { ThunkAction } from "@/form-builder/declarations";
import { FormBuilderConfig } from "@/form-builder/FormBuilderConfig";
import { showSavingMessage } from "@/form-builder/store/actions";
import { refreshPropertiesPanels } from "@/form-builder/store/refresh-properties-panels-notifier/actions";
import { formBuilderSaveState } from "@/form-builder/store/save-state";

/**
 * Ensures widget markup of the widget (form component) before adding it to the zone.
 * Saves the whole state for the current form on the server.
 * @param widgetType Widget type identifier.
 * @param zoneIdentifier Widget zone identifier.
 * @param position Position of where to add the widget within the widget zone.
 */
export const addWidget = (widgetType: string, zoneIdentifier: string, position: number): ThunkAction<Promise<void>> =>
  async (dispatch, getState, services) => {
    try {
      dispatch(freezeWidgetSelection());
      dispatch(showSavingMessage());

      await widgetsThunks.addWidget(widgetType, zoneIdentifier, position)(dispatch, getState, services);
      const formBuilderConfig = builderConfig as FormBuilderConfig;
      await formBuilderSaveState(dispatch, getState(), formBuilderConfig.formIdentifier.toString());

      dispatch(refreshPropertiesPanels());
      dispatch(thawWidgetSelection());
    } catch (error) {
      services.logger.logException(error);
    }
  };

/**
 * Moves widget to a new position.
 * Saves the whole state for the current form on the server.
 * @param widgetIdentifier Widget identifier.
 * @param originalZoneIdentifier Source zone identifier.
 * @param targetZoneIdentifier Target zone identifier.
 * @param position A new position of widget.
 */
export const moveWidget = (widgetIdentifier: string, originalZoneIdentifier: string, targetZoneIdentifier: string, position: number): ThunkAction<Promise<void>> =>
  async (dispatch, getState, services) => {
    try {
      await widgetsThunks.moveWidget(widgetIdentifier, originalZoneIdentifier, targetZoneIdentifier, position)(dispatch, getState, services);

      const formBuilderConfig = builderConfig as FormBuilderConfig;
      await formBuilderSaveState(dispatch, getState(), formBuilderConfig.formIdentifier.toString());

      dispatch(refreshPropertiesPanels());
    } catch (error) {
      services.logger.logException(error);
    }
  };

/**
 * Removes widget from a zone.
 * @param widgetIdentifier Identifier of a widget to be removed.
 * @param zoneIdentifier Identifier of the zone where the widget is located.
 */
export const removeWidget = (widgetIdentifier: string, zoneIdentifier: string): ThunkAction<Promise<void>> =>
  async (dispatch, getState, services) => {
    try {
      widgetsThunks.removeWidget(widgetIdentifier, zoneIdentifier)(dispatch, getState, services);
      const formBuilderConfig = builderConfig as FormBuilderConfig;
      await formBuilderSaveState(dispatch, getState(), formBuilderConfig.formIdentifier.toString());

      dispatch(refreshPropertiesPanels());
    } catch (error) {
      services.logger.logException(error);
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

      const formBuilderConfig = builderConfig as FormBuilderConfig;
      await formBuilderSaveState(dispatch, getState(), formBuilderConfig.formIdentifier.toString());

      dispatch(refreshPropertiesPanels());
    } catch (error) {
      services.logger.logException(error);
    }
  };

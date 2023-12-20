
import { builderConfig } from "@/builder/api/client";
import { eventHelper } from "@/builder/helpers";
import * as builderSectionsThunks from "@/builder/store/sections/thunks";

import { FORM_BUILDER_FREEZE_UI, FORM_BUILDER_UNFREEZE_UI } from "@/form-builder/constants";
import { ThunkAction } from "@/form-builder/declarations";
import { FormBuilderConfig } from "@/form-builder/FormBuilderConfig";
import { showSavingMessage } from "@/form-builder/store/actions";
import { formBuilderSaveState } from "@/form-builder/store/save-state";

/**
 * Ensures section markup and adds it to the zone.
 * @param sectionType Section type identifier.
 * @param areaIdentifier Area identifier.
 * @param position Section position within the area.
 */
export const addSection = (sectionType: string, areaIdentifier: string, position: number): ThunkAction<Promise<void>> =>
  async (dispatch, getState, services) => {
    try {
      eventHelper.dispatchEvent(FORM_BUILDER_FREEZE_UI, {});
      dispatch(showSavingMessage());

      await builderSectionsThunks.addSection(sectionType, areaIdentifier, position)(dispatch, getState, services);
      const formBuilderConfig = builderConfig as FormBuilderConfig;
      await formBuilderSaveState(dispatch, getState(), formBuilderConfig.formIdentifier.toString());

      eventHelper.dispatchEvent(FORM_BUILDER_UNFREEZE_UI, {});
    } catch (error) {
      services.logger.logException(error);
    }
  };

/**
 * Removes specific section with all zones and widgets inside.
 * @param sectionIdentifier Section identifier.
 * @param areaIdentifier Area identifier.
 */
export const removeSection = (sectionIdentifier: string, areaIdentifier: string): ThunkAction<Promise<void>> =>
  async (dispatch, getState, services) => {
    try {
      eventHelper.dispatchEvent(FORM_BUILDER_FREEZE_UI, {});
      dispatch(showSavingMessage());

      await builderSectionsThunks.removeSection(sectionIdentifier, areaIdentifier)(dispatch, getState, services);
      const formBuilderConfig = builderConfig as FormBuilderConfig;
      await formBuilderSaveState(dispatch, getState(), formBuilderConfig.formIdentifier.toString());

      eventHelper.dispatchEvent(FORM_BUILDER_UNFREEZE_UI, {});
    } catch (error) {
      services.logger.logException(error);
    }
  };

/**
 * Change section thunk.
 * @param sourceSectionIdentifier Identifier of the section to be replaced.
 * @param targetSectionType Type of section which replaces the original one.
 */
export const changeSection = (sourceSectionIdentifier: string, targetSectionType: string): ThunkAction<Promise<void>> =>
  async (dispatch, getState, services) => {
    try {
      eventHelper.dispatchEvent(FORM_BUILDER_FREEZE_UI, {});
      dispatch(showSavingMessage());

      await builderSectionsThunks.changeSection(sourceSectionIdentifier, targetSectionType)(dispatch, getState, services);
      const formBuilderConfig = builderConfig as FormBuilderConfig;
      await formBuilderSaveState(dispatch, getState(), formBuilderConfig.formIdentifier.toString());

      eventHelper.dispatchEvent(FORM_BUILDER_UNFREEZE_UI, {});
    } catch (error) {
      services.logger.logException(error);
    }
  };

/**
 * Move section to a new position.
 * @param sectionIdentifier Section identifier.
 * @param originalAreaIdentifier Source area identifier.
 * @param targetAreaIdentifier Target area identifier.
 * @param position A new position of section.
 */
export const moveSection = (sectionIdentifier: string, originalAreaIdentifier: string, targetAreaIdentifier: string, position: number): ThunkAction<Promise<void>> =>
  async (dispatch, getState, services) => {
    try {
      eventHelper.dispatchEvent(FORM_BUILDER_FREEZE_UI, {});
      dispatch(showSavingMessage());

      await builderSectionsThunks.moveSection(sectionIdentifier, originalAreaIdentifier, targetAreaIdentifier, position)(dispatch, getState, services);
      const formBuilderConfig = builderConfig as FormBuilderConfig;
      await formBuilderSaveState(dispatch, getState(), formBuilderConfig.formIdentifier.toString());

      eventHelper.dispatchEvent(FORM_BUILDER_UNFREEZE_UI, {});
    } catch (error) {
      services.logger.logException(error);
    }
  };

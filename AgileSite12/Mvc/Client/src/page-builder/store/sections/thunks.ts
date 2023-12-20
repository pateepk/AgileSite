import * as actions from "@/builder/store/sections/actions";
import { createNewSectionWithZones, updateSectionWidgetVariantsWithDirtyMarkup } from "@/builder/store/sections/helpers";
import * as builderSectionsThunks from "@/builder/store/sections/thunks";

import { ThunkAction } from "@/page-builder/declarations";

/**
 * Ensures section markup and adds it to the area.
 * @param sectionType Section type identifier.
 * @param areaIdentifier Area identifier.
 * @param position Section position within the area.
 */
export const addSection = (sectionType: string, areaIdentifier: string, position: number): ThunkAction<Promise<void>> =>
  async (dispatch, getState, services) => {
    try {
      await builderSectionsThunks.addSection(sectionType, areaIdentifier, position)(dispatch, getState, services);
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
      await builderSectionsThunks.removeSection(sectionIdentifier, areaIdentifier)(dispatch, getState, services);
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
      await builderSectionsThunks.changeSection(sourceSectionIdentifier, targetSectionType)(dispatch, getState, services);
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
      await builderSectionsThunks.moveSection(sectionIdentifier, originalAreaIdentifier, targetAreaIdentifier, position)(dispatch, getState, services);
    } catch (error) {
      services.logger.logException(error);
    }
  };

/**
 * Sets section properties and refreshes section's and optionally widgets' markup.
 * @param sectionIdentifier Identifier of section whose properties to set.
 * @param properties Properties to be set.
 */
export const setSectionProperties = (sectionIdentifier: string, properties: object): ThunkAction<Promise<void>> =>
  async (dispatch, getState, services) => {
    try {
      const state = getState();
      const section = state.sections[sectionIdentifier];

      // Prepare section of correct type and with appropriate number of zones.
      const { section: newSection, markup } = await createNewSectionWithZones(state, section.type, sectionIdentifier, properties);

      await updateSectionWidgetVariantsWithDirtyMarkup(sectionIdentifier, state, dispatch);

      dispatch(actions.changeSection(section, newSection, markup));
    } catch (error) {
      services.logger.logException(error);
    }
  };

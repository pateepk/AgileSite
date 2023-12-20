import _pick from "lodash.pick";

import { ThunkAction } from "@/builder/declarations";
import { createNewSectionWithZones, updateSectionWidgetVariantsWithDirtyMarkup } from "@/builder/store/sections/helpers";
import { getWidgetsInZones } from "@/builder/store/zones/helpers";

import * as actions from "./actions";

/**
 * Ensures section markup and adds it to the zone.
 * @param sectionType Section type identifier.
 * @param areaIdentifier Area identifier.
 * @param position Section position within the area.
 */
export const addSection = (sectionType: string, areaIdentifier: string, position: number): ThunkAction<Promise<void>> =>
  async (dispatch, getState) => {
    const { section, markup } = await createNewSectionWithZones(getState(), sectionType);

    dispatch(actions.addSection(section, areaIdentifier, position, markup));
  };

/**
 * Removes specific section with all zones and widgets inside.
 * @param sectionIdentifier Section identifier.
 * @param areaIdentifier Area identifier.
 */
export const removeSection = (sectionIdentifier: string, areaIdentifier: string): ThunkAction<Promise<void>> =>
  async (dispatch, getState) => {
    const state = getState();

    const section = state.sections[sectionIdentifier];
    const toBeDeletedWidgetIdentifiers = getWidgetsInZones(section.zones, state.zones);
    const toBeDeletedWidgets = Object.values(_pick(state.widgets, toBeDeletedWidgetIdentifiers));

    if (state.editableAreas[areaIdentifier].sections.length === 1) {
      const {
        section: defaultSection,
        markup,
      } = await createNewSectionWithZones(state, state.editableAreasConfiguration[areaIdentifier].defaultSection, sectionIdentifier);
      dispatch(actions.changeSection(section, defaultSection, markup, toBeDeletedWidgets));
    } else {
      dispatch(actions.removeSection(section, areaIdentifier, toBeDeletedWidgets));
    }
  };

/**
 * Change section thunk.
 * @param sourceSectionIdentifier Identifier of the section to be replaced.
 * @param targetSectionType Type of section which replaces the original one.
 */
export const changeSection = (sourceSectionIdentifier: string, targetSectionType: string): ThunkAction<Promise<void>> =>
  async (dispatch, getState) => {
    const state = getState();
    const sourceSection = state.sections[sourceSectionIdentifier];

    // Prepare new section of correct type and with appropriate number of zones.
    const { section: targetSection, markup } = await createNewSectionWithZones(state, targetSectionType);

    await updateSectionWidgetVariantsWithDirtyMarkup(sourceSectionIdentifier, state, dispatch);

    dispatch(actions.changeSection(sourceSection, targetSection, markup));
  };

/**
 * Move section to a new position.
 * @param sectionIdentifier Section identifier.
 * @param originalAreaIdentifier Source area identifier.
 * @param targetAreaIdentifier Target area identifier.
 * @param position A new position of section.
 */
export const moveSection = (sectionIdentifier: string, originalAreaIdentifier: string, targetAreaIdentifier: string, position: number): ThunkAction<Promise<void>> =>
  async (dispatch, getState) => {
    const state = getState();

    await updateSectionWidgetVariantsWithDirtyMarkup(sectionIdentifier, state, dispatch);

    // In case it is a last section in area, create a new default section there
    if (state.editableAreas[originalAreaIdentifier].sections.length === 1) {
      const { section, markup } = await createNewSectionWithZones(state, state.editableAreasConfiguration[originalAreaIdentifier].defaultSection);
      dispatch(actions.addSection(section, originalAreaIdentifier, 0, markup));
    }

    dispatch(actions.moveSection(sectionIdentifier, originalAreaIdentifier, targetAreaIdentifier, position));
  };

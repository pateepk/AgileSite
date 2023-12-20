import _flatmap from "lodash.flatmap";
import { ThunkDispatch } from "redux-thunk";
import v4 from "uuid/v4";

import * as api from "@/builder/api";
import { Section, SectionMarkup, State } from "@/builder/declarations";
import { getSectionZoneCount, parseSectionMarkup } from "@/builder/services/page-configuration/provider";
import { updateWidgetVariantMarkups } from "@/builder/store/markups/actions";
import { BuilderAction } from "@/builder/store/types";
import { getWidgetsInZones } from "@/builder/store/zones/helpers";

/**
 * Handles "dirty" widget variants - makes API calls to get fresh widget variants' markup.
 * @param sectionId Section identifier.
 * @param state Represents the structure of the store.
 */
const updateSectionWidgetVariantsWithDirtyMarkup = async (sectionId: string, state: State, dispatch: ThunkDispatch<State, {}, BuilderAction>): Promise<void> => {
  // Find widget variants whose markup is "dirty" and needs to be renewed
  const widgetsWithinSection = getWidgetsInZones(state.sections[sectionId].zones, state.zones);
  const widgetVariantsWithDirtyMarkup = getWidgetVariantsWithDirtyMarkup(widgetsWithinSection, state);
  if (widgetVariantsWithDirtyMarkup.length === 0) {
    return;
  }

  await updateWidgetVariantsWithDirtyMarkup(widgetVariantsWithDirtyMarkup,  state, dispatch);
};

const getWidgetVariantsWithDirtyMarkup = (widgetIdentifiers: string[], state: State): Array<[string, {}]> => {
  const isVariantDirty = (variantIdentifier: string): boolean => {
    return state.markups.variants[variantIdentifier].isDirty;
  };

  const dirtyVariants = widgetIdentifiers.reduce((acc, widgetIdentifier) => {
    const widgetType = state.widgets[widgetIdentifier].type;
    const dirtyVariantIdentifiers = state.widgets[widgetIdentifier].variants.filter(isVariantDirty);
    if (dirtyVariantIdentifiers.length === 0) {
      return acc;
    }
    if (acc[widgetType] === undefined) {
      return {
        ...acc,
        [widgetType]: dirtyVariantIdentifiers,
      };
    }
    return {
      ...acc,
      [widgetType]: [...acc[widgetType], ...dirtyVariantIdentifiers],
    };
  }, {});

  return Object.entries(dirtyVariants);
};

const updateWidgetVariantsWithDirtyMarkup = async (widgetVariantsWithDirtyMarkup: Array<[string, {}]>, state: State,
                                                   dispatch: ThunkDispatch<State, {}, BuilderAction>): Promise<void> => {
  const markupPromises = getWidgetVariantMarkupPromises(widgetVariantsWithDirtyMarkup, state);

  const updatedMarkups = await Promise.all(markupPromises);
  const variantIdentifiers = _flatmap(widgetVariantsWithDirtyMarkup, ([_, widgetVariantIdentifiers]) => widgetVariantIdentifiers);

  const variantMarkups = variantIdentifiers.reduce((acc, variantIdentifier: any, index) => {
    acc[variantIdentifier] = updatedMarkups[index];
    return acc;
  }, {});

  dispatch(updateWidgetVariantMarkups(variantMarkups));
};

const getWidgetVariantMarkupPromises = (widgetVariantsWithDirtyMarkup: Array<[string, {}]>, state: State): Array<Promise<string>> => {
  return _flatmap(widgetVariantsWithDirtyMarkup, ([widgetType, variantIds]: any) => {
    const markupUrl = state.metadata.widgets[widgetType].markupUrl;

    return variantIds.map((variantId) => {
      const properties = state.widgetVariants[variantId].properties;

      return api.getComponentMarkup(markupUrl, properties);
    });
  });
};

/**
 * Creates a new section based on passed section type and fills it with widget zones accordingly.
 * @param state Redux state.
 * @param sectionType Type of the to be created section.
 * @param sectionIdentifier Section identifier.
 * @param properties Section properties.
 * @returns Created section.
 */
const createNewSectionWithZones = async (state: State, sectionType: string, sectionIdentifier: string = null, properties: object = null):
  Promise<{ section: Section, markup: SectionMarkup }> => {
  const sectionMetadata = state.metadata.sections[sectionType];

  if (sectionMetadata.hasProperties === true && properties === null) {
    properties = await api.getComponentDefaultProperties(sectionMetadata.defaultPropertiesUrl);
  }
  const rawMarkup = await api.getComponentMarkup(sectionMetadata.markupUrl, properties);
  const markup = parseSectionMarkup(rawMarkup);
  const zoneCount = getSectionZoneCount(markup.markup);

  const section: Section = {
    identifier: sectionIdentifier || v4(),
    type: sectionType,
    properties,
    zones: [],
  };

  for (let index = 0; index < zoneCount; index++) {
    const identifier = v4();
    section.zones.push(identifier);
  }

  return {
    section,
    markup,
  };
};

export {
  updateSectionWidgetVariantsWithDirtyMarkup,
  createNewSectionWithZones,
};

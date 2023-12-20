import _flatten from "lodash.flatten";
import _mapValues from "lodash.mapvalues";
import _pick from "lodash.pick";
import v4 from "uuid/v4";

import * as api from "@/builder/api";
import { getService } from "@/builder/container";
import * as d from "@/builder/declarations";
import { EditableArea } from "@/builder/declarations";
import { arrayHelper } from "@/builder/helpers";
import { logger } from "@/builder/logger";
import { postMessage } from "@/builder/services/post-message";
import { MessageTypes, SERVICE_TYPES } from "@/builder/types";

const EDITABLE_AREA_DATA_ATTRIBUTE = "data-kentico-editable-area-id";
const WIDGET_ZONE_DATA_ATTRIBUTE = "data-kentico-widget-zone";

/**
 * Filters areas and nested objects in configuration. Doesn't modify parameters.
 * @param areaIdentifiers Area identifiers.
 * @param configuration Normalized page configuration.
 * @returns Page configuration which includes only specified areas and nested objects (zones, widgets...).
 */
const filterPageConfigurations = (areaIdentifiers: string[], configuration: d.NormalizedConfiguration): d.NormalizedConfiguration => {
  const filteredAreas = _pick(configuration.editableAreas, areaIdentifiers);

  const sectionIdentifiers = _flatten(Object.values(filteredAreas).map((area) => area.sections));
  const filteredSections = _pick(configuration.sections, sectionIdentifiers);

  const zoneIdentifiers = _flatten(Object.values(filteredSections).map((section) => section.zones));
  const filteredZones = _pick(configuration.zones, zoneIdentifiers);

  const widgetIdentifiers = _flatten(Object.values(filteredZones).map((zone) => zone.widgets));
  const filteredWidgets = _pick(configuration.widgets, widgetIdentifiers);

  const variantIdentifiers = _flatten(Object.values(filteredWidgets).map((widget) => widget.variants));
  const filteredWidgetVariants = _pick(configuration.widgetVariants, variantIdentifiers);

  const filteredPageConfig: d.NormalizedConfiguration = {
    editableAreas: filteredAreas,
    pageTemplate: configuration.pageTemplate,
    sections: filteredSections,
    zones: filteredZones,
    widgets: filteredWidgets,
    widgetVariants: filteredWidgetVariants,
  };

  if (Object.keys(configuration.editableAreas).length > Object.keys(filteredPageConfig.editableAreas).length) {
    const removedAreas = Object.values(configuration.editableAreas)
      .filter((area: d.EditableArea) => !arrayHelper.contains(areaIdentifiers, area.identifier))
      .map((area: d.EditableArea) => area.identifier);
    logWarning("areas.excessAreas", removedAreas);
    postMessage(MessageTypes.CONFIGURATION_CHANGED, null, "*");
  }

  return filteredPageConfig;
};

/**
 * Adds specified areas to configuration.
 * @param editableAreas Editable areas.
 * @param configuration Normalized page configuration.
 * @returns Page configuration which includes specified areas in provided configuration.
 */
const addEditableAreas = (editableAreas: d.Entities<d.EditableArea>, configuration: d.NormalizedConfiguration): d.NormalizedConfiguration => {
  return {
    ...configuration,
    editableAreas: {
      ...editableAreas,
      ...configuration.editableAreas,
    },
  };
};

/**
 * Returns properties of default sections of empty editable areas.
 * @param state State.
 * @param editableAreas Editable areas.
 */
const getDefaultSectionsProperties = async (state: d.State, editableAreas: EditableArea[]): Promise<object[]> => {
  const defaultPropertiesPromises: Array<Promise<object>> = [];
  editableAreas.forEach((area) => {
    const defaultSectionTypeIdentifier = state.editableAreasConfiguration[area.identifier].defaultSection;
    const metadata = state.metadata.sections[defaultSectionTypeIdentifier];
    if (metadata.hasProperties === true) {
      defaultPropertiesPromises.push(api.getComponentDefaultProperties(metadata.defaultPropertiesUrl));
    } else {
      defaultPropertiesPromises.push(Promise.resolve(null));
    }
  });

  return Promise.all(defaultPropertiesPromises);
};

/**
 * Ensures default properties for page template.
 * @param state State.
 */
const ensureDefaultTemplateProperties = async (state: d.State): Promise<d.State> => {
  if (state.pageTemplate) {
    const pageTemplate = state.metadata.pageTemplates[state.pageTemplate.identifier];
    if ((state.pageTemplate.properties === null) && pageTemplate.hasProperties) {
      const defaultProperties = await api.getComponentDefaultProperties(pageTemplate.defaultPropertiesUrl);

      return {
        ...state,
        pageTemplate: {
          ...state.pageTemplate,
          properties: defaultProperties,
        },
      };
    }
  }

  return state;
};

/**
 * Returns markups of default sections of empty editable areas.
 * @param state State.
 * @param editableAreas Editable areas.
 * @param defaultProperties Default sections properties.
 */
const getDefaultSectionsMarkups = async (state: d.State, editableAreas: EditableArea[], defaultProperties: object[]): Promise<string[]> => {
  const markupPromises: Array<Promise<string>> = [];
  editableAreas.forEach((area, index) => {
    const defaultSectionTypeIdentifier = state.editableAreasConfiguration[area.identifier].defaultSection;
    const metadata = state.metadata.sections[defaultSectionTypeIdentifier];
    markupPromises.push(api.getComponentMarkup(metadata.markupUrl, defaultProperties[index]));
  });

  return Promise.all(markupPromises);
};

/**
 * Adds a default section and relevant number of zones to editable areas without any sections.
 * @param state State.
 */
const addDefaultSections = async (state: d.State): Promise<void> => {
  const emptyEditableAreas = Object.values(state.editableAreas).filter((area) => area.sections.length === 0);
  const defaultProperties = await getDefaultSectionsProperties(state, emptyEditableAreas);
  const sectionMarkups = await getDefaultSectionsMarkups(state, emptyEditableAreas, defaultProperties);

  emptyEditableAreas.forEach((area, index) => {
    const sectionIdentifier = v4();
    const defaultSectionTypeIdentifier = state.editableAreasConfiguration[area.identifier].defaultSection;
    const markup = parseSectionMarkup(sectionMarkups[index]);
    const zoneCount = getSectionZoneCount(markup.markup);

    area.sections.push(sectionIdentifier);

    state.sections[sectionIdentifier] = {
      identifier: sectionIdentifier,
      properties: defaultProperties[index],
      type: defaultSectionTypeIdentifier,
      zones: [],
    };

    state.markups.sections[sectionIdentifier] = markup;

    for (let i = 0; i < zoneCount; i++) {
      addZone(state, sectionIdentifier);
    }
  });
};

/**
 * Adds empty zones to sections that have less zones stored than markup defines.
 * @param state State.
 */
const addMissingZones = (state: d.State): void => {
  Object.values(state.sections).forEach((section: d.Section) => {
    const actualZoneCount = getSectionZoneCount(state.markups.sections[section.identifier].markup);

    for (let zonesCount = section.zones.length; zonesCount < actualZoneCount; ++zonesCount) {
      addZone(state, section.identifier);
    }
  });
};

/**
 * Finds and notifies user about sections without zones.
 * @param state State.
 */
const notifyAboutSectionsWithoutZones = (state: d.State): void => {
  const sectionTypesWithoutZones = arrayHelper.getUniqueValues(Object.values(state.sections)
    .filter((section: d.Section) => getSectionZoneCount(state.markups.sections[section.identifier].markup) === 0)
    .map((section: d.Section) => section.type));
  if (sectionTypesWithoutZones.length !== 0) {
    logWarning("section.withoutZones", sectionTypesWithoutZones);
  }
};

/**
 * Sets up initial state for displayed variants. Does not modify parameters.
 * @param state State.
 * @returns State containing display variant for every widget.
 */
const ensureDisplayedWidgetVariants = (state: d.State): d.State => {
  return {
    ...state,
    displayedWidgetVariants: _mapValues(state.widgets, (widget) => widget.variants[0]),
  };
};

/**
 * Adds a new zone instance into the specified section.
 * @param state State.
 * @param sectionIdentifier Section identifier where a new zone will be added to.
 */
const addZone = (state: d.State, sectionIdentifier: string): void => {
  const zoneIdentifier = v4();
  state.zones[zoneIdentifier] = {
    identifier: zoneIdentifier,
    widgets: [],
  };

  state.sections[sectionIdentifier].zones.push(zoneIdentifier);
};

/**
 * Loads widget variant markups from server and adds them to the state.
 * @param state State.
 */
const loadWidgetVariantMarkups = async (state: d.State): Promise<void> => {
  const widgets = Object.values(state.widgets);

  const markupPromises = [];
  widgets.forEach((widget: d.Widget) => {
    const widgetMetadata = state.metadata.widgets[widget.type];

    widget.variants.forEach((variantGuid: string) => {
      markupPromises.push(api.getComponentMarkup(widgetMetadata.markupUrl, state.widgetVariants[variantGuid].properties));
    });
  });

  const variantMarkups = await Promise.all(markupPromises);

  let index = 0;
  widgets.forEach((widget: d.Widget) => {
    widget.variants.forEach((variantGuid: string) => {
      state.markups.variants[variantGuid] = {
        markup: variantMarkups[index],
        isDirty: false,
      } as d.WidgetVariantMarkup;
      ++index;
    });
  });
};

/**
 * Loads section markups from server and adds them to the state.
 * @param state State.
 */
const loadSectionMarkups = async (state: d.State): Promise<void> => {
  const sections = Object.values(state.sections);

  const markupPromises = [];
  sections.forEach((section: d.Section) => {
    markupPromises.push(api.getComponentMarkup(state.metadata.sections[section.type].markupUrl, section.properties));
  });

  const sectionMarkups = await Promise.all(markupPromises);

  sections.forEach((section: d.Section, index) => {
    state.markups.sections[section.identifier] = parseSectionMarkup(sectionMarkups[index]);
  });
};

/**
 * Adds widget zone index to each zone in section's markup.
 * @param markup Section markup.
 */
const parseSectionMarkup = (markup: string): d.SectionMarkup => {
  let zoneCount = 0;
  const widgetZoneMatcher = new RegExp(`<div ${WIDGET_ZONE_DATA_ATTRIBUTE}=""></div>`, "g");
  const getZoneMarkup = (zoneIndex: number) => `<widget-zone :section-identifier="sectionIdentifier" :area-identifier="areaIdentifier" :zone-index="${zoneIndex}"></widget-zone>`;
  markup = markup.replace(widgetZoneMatcher, () => getZoneMarkup(zoneCount++));

  return {
    markup,
  };
};

/**
 * Returns count of zones inside section.
 * @param markup Parsed section markup.
 */
const getSectionZoneCount = (markup: string): number => {
  const widgetZoneMatcher = new RegExp("<widget-zone.+?<\/widget-zone>", "g");
  const matches = markup.match(widgetZoneMatcher);

  return matches !== null ? matches.length : 0;
};

/**
 * Loads configuration for areas from page markup.
 * @returns Object containing area configurations from markup.
 */
const loadAreaConfigurations = (): d.Entities<d.AreaConfiguration> => {
  const areaConfigurations: d.Entities<d.AreaConfiguration> = {};
  const editableAreas = document.querySelectorAll(`[${EDITABLE_AREA_DATA_ATTRIBUTE}]`);

  // Use forEach on Array instead of NodeList which is not supported in all browsers
  Array.prototype.forEach.call(editableAreas, (area: HTMLElement) => {
    let allowedWidgets = [];

    if (area.dataset.kenticoAllowedWidgets) {
      allowedWidgets = JSON.parse(area.dataset.kenticoAllowedWidgets);
    }

    const areaConfiguration: d.AreaConfiguration = {
      defaultSection: area.dataset.kenticoDefaultSection,
      widgetRestrictions: allowedWidgets,
    };

    areaConfigurations[area.dataset.kenticoEditableAreaId] = areaConfiguration;
  });

  return areaConfigurations;
};

/**
 * Generates a default page configuration from DOM.
 */
const prepareDefaultConfiguration = (): d.NormalizedConfiguration => {
  const editableAreas = document.querySelectorAll(`[${EDITABLE_AREA_DATA_ATTRIBUTE}]`);
  const editableAreaIds = Array.prototype.map.call(editableAreas, (area: HTMLElement) => area.dataset.kenticoEditableAreaId);
  const initialState: d.NormalizedConfiguration = {
    editableAreas: {},
    sections: {},
    zones: {},
    widgets: {},
    widgetVariants: {},
  };

  editableAreaIds.forEach((identifier) => {
    initialState.editableAreas[identifier] = {
      identifier,
      sections: [],
    };
  });

  return initialState;
};

/**
 * Loads metadata for components from server.
 * @returns Metadata object when resolved.
 */
const loadComponentsMetadata = async (): Promise<d.Metadata> => {
  return api.getComponentMetadata();
};

/**
 * Logs message with list argument joined by commas.
 * @param localizationResourceKey Localization resource string for message.
 * @param items List of items to be passed as first parameter for localized string formatter.
 */
const logWarning = (localizationResourceKey: string, items: string[]): void => {
  const localizationService = getService<d.LocalizationService>(SERVICE_TYPES.LocalizationService);
  const message = localizationService.getLocalization(localizationResourceKey, items.join(", "));
  logger.logWarning(message);
};

export {
  addDefaultSections,
  addMissingZones,
  ensureDisplayedWidgetVariants,
  ensureDefaultTemplateProperties,
  filterPageConfigurations,
  loadComponentsMetadata,
  loadWidgetVariantMarkups,
  loadAreaConfigurations,
  prepareDefaultConfiguration,
  loadSectionMarkups,
  notifyAboutSectionsWithoutZones,
  parseSectionMarkup,
  getSectionZoneCount,
  addEditableAreas,
  logWarning,
};

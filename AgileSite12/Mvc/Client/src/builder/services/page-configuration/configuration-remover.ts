import _flatMap from "lodash.flatmap";
import _mapValues from "lodash.mapvalues";
import _omit from "lodash.omit";
import _pick from "lodash.pick";

import { Entities, Section, State, Widget, WidgetVariant, WidgetZone } from "@/builder/declarations";
import { arrayHelper } from "@/builder/helpers";
import { getSectionZoneCount, logWarning } from "@/builder/services/page-configuration/provider";
import { postMessage } from "@/builder/services/post-message";
import { MessageTypes } from "@/builder/types";

/**
 * Removes sections with missing type metadata.
 * @param state State.
 * @returns State with filtered missing section types.
 */
const removeSectionsWithMissingType = (state: State): State => {
  const sectionsToRemove = Object.values(state.sections).filter((section: Section) => !state.metadata.sections[section.type]);
  if (sectionsToRemove.length !== 0) {
    logWarning("section.missingmetadata", arrayHelper.getUniqueValues(sectionsToRemove.map((section: Section) => section.type)));
    postMessage(MessageTypes.CONFIGURATION_CHANGED, null, "*");
    return removeSections(state, sectionsToRemove);
  }
  return state;
};

/**
 * Removes widgets with missing type metadata.
 * @param state State.
 * @returns State with filtered missing widget types.
 */
const removeWidgetsWithMissingType = (state: State): State => {
  const widgetsToRemove = Object.values(state.widgets).filter((widget: Widget) => !state.metadata.widgets[widget.type]);
  if (widgetsToRemove.length !== 0) {
    logWarning("widget.missingmetadata", arrayHelper.getUniqueValues(widgetsToRemove.map((widget: Widget) => widget.type)));
    postMessage(MessageTypes.CONFIGURATION_CHANGED, null, "*");
    return removeWidgets(state, widgetsToRemove);
  }
  return state;
};

/**
 * Removes personalization variants and condition type from widgets with missing condition type in metadata.
 * @param state State.
 * @returns State with filtered widgets and variants.
 */
const removePersonalizationOfMissingConditionTypes = (state: State): State => {
  const conditionTypesToRemove = arrayHelper.getUniqueValues(Object.values(state.widgets)
    .filter((widget: Widget) => widget.conditionType && !state.metadata.personalizationConditionTypes[widget.conditionType])
    .map((widget: Widget) => widget.conditionType));
  if (conditionTypesToRemove.length !== 0) {
    logWarning("conditiontype.missingmetadata", conditionTypesToRemove);
    postMessage(MessageTypes.CONFIGURATION_CHANGED, null, "*");
    return removeConditionTypesFromWidgets(state, conditionTypesToRemove);
  }
  return state;
};

/**
 * Removes excess zones and their widgets from sections with more zones stored than markup defines.
 * @param state State.
 * @return State with excess zones removed from sections.
 */
const removeExcessZones = (state: State): State => {
  const sectionTypesWithRemovedWidgets = new Set<string>();
  const zoneNames = _flatMap(Object.values(state.sections), (section: Section) => {
    const extraZoneIdentifiers: string[] = [];
    for (let i = getSectionZoneCount(state.markups.sections[section.identifier].markup); i < section.zones.length; i++) {
      const extraZoneIdentifier = state.sections[section.identifier].zones[i];
      if (state.zones[extraZoneIdentifier].widgets.length !== 0) {
        sectionTypesWithRemovedWidgets.add(section.type);
      }
      extraZoneIdentifiers.push(extraZoneIdentifier);
    }
    return extraZoneIdentifiers;
  });

  if (sectionTypesWithRemovedWidgets.size !== 0) {
    postMessage(MessageTypes.CONFIGURATION_CHANGED, null, "*");
    logWarning("section.missingNonemptyZone", [...sectionTypesWithRemovedWidgets]);
  }

  const zonesToRemove = Object.values(_pick(state.zones, zoneNames));
  return removeZones(state, zonesToRemove);
};

/**
 * Removes provided sections and their zones from state.
 * @param state State.
 * @param sectionsToRemove List of sections that should be removed from state.
 * @returns Copy of state with sections removed.
 */
const removeSections = (state: State, sectionsToRemove: Section[]): State => {
  const identifiersToRemove = sectionsToRemove.map((section: Section) => section.identifier);
  state = {
    ...state,
    editableAreas: removeValuesFromPropertyOfSubstate(state.editableAreas, "sections", identifiersToRemove),
    sections: _omit(state.sections, identifiersToRemove),
  };
  return removeZonesOfSections(state, sectionsToRemove);
};

const removeZonesOfSections = (state: State, sections: Section[]): State => {
  const zoneNames = _flatMap(sections, (section: Section) => section.zones);
  const zonesToRemove = Object.values(_pick(state.zones, zoneNames));
  return removeZones(state, zonesToRemove);
};

/**
 * Removes provided zones and their widgets from state.
 * @param state State.
 * @param zonesToRemove List of zones that should be removed from state.
 * @returns Copy of state with zones removed.
 */
const removeZones = (state: State, zonesToRemove: WidgetZone[]): State => {
  const identifiersToRemove = zonesToRemove.map((zone: WidgetZone) => zone.identifier);
  state = {
    ...state,
    sections: removeValuesFromPropertyOfSubstate(state.sections, "zones", identifiersToRemove),
    zones: _omit(state.zones, identifiersToRemove),
  };
  return removeWidgetsOfZones(state, zonesToRemove);
};

const removeWidgetsOfZones = (state: State, zones: WidgetZone[]): State => {
  const widgetNames = _flatMap(zones, (zone: WidgetZone) => zone.widgets);
  const widgetsToRemove = Object.values(_pick(state.widgets, widgetNames));
  return removeWidgets(state, widgetsToRemove);
};

/**
 * Removes provided widgets and their variants.
 * @param state State.
 * @param widgetsToRemove List of widgets that should be removed from state.
 * @returns Copy of state with widgets removed.
 */
const removeWidgets = (state: State, widgetsToRemove: Widget[]): State => {
  const identifiersToRemove = widgetsToRemove.map((widget: Widget) => widget.identifier);
  state = {
    ...state,
    zones: removeValuesFromPropertyOfSubstate(state.zones, "widgets", identifiersToRemove),
    widgets: _omit(state.widgets, identifiersToRemove),
  };
  return removeVariantsOfWidgets(state, widgetsToRemove);
};

const removeVariantsOfWidgets = (state: State, widgets: Widget[]): State => {
  const variantNames = _flatMap(widgets, (widget: Widget) => widget.variants);
  const variantsToRemove = Object.values(_pick(state.widgetVariants, variantNames));
  return removeVariants(state, variantsToRemove);
};

/**
 * Removes provided variants.
 * @param state State.
 * @param variantsToRemove List of variants that should be removed from state.
 * @returns Copy of state with variants removed.
 */
const removeVariants = (state: State, variantsToRemove: WidgetVariant[]): State => {
  const identifiersToRemove = variantsToRemove.map((variant: WidgetVariant) => variant.identifier);
  return {
    ...state,
    widgets: removeValuesFromPropertyOfSubstate(state.widgets, "variants", identifiersToRemove),
    widgetVariants: _omit(state.widgetVariants, identifiersToRemove),
  };
};

/**
 * Removes provided condition types from all widgets and their personalization variants.
 * @param state State.
 * @param variantsToRemove List of condition types that should be removed from state.
 * @returns Copy of state with condition types removed.
 */
const removeConditionTypesFromWidgets = (state: State, conditionTypesToRemove: string[]): State => {
  const widgetsWithMissingConditionTypes = Object.values(state.widgets).filter((widget: Widget) => arrayHelper.contains(conditionTypesToRemove, widget.conditionType));
  const variantNames = _flatMap(widgetsWithMissingConditionTypes, (item) => item.variants.slice(1));
  const variantsToRemove = Object.values(_pick(state.widgetVariants, variantNames));

  state = {
    ...state,
    widgets: _mapValues(state.widgets, (widget: Widget) => arrayHelper.contains(conditionTypesToRemove, widget.conditionType) ? _omit(widget, "conditionType") : widget),
  };
  return removeVariants(state, variantsToRemove);
};

/**
 * Removes identifiers from an array in object.
 * @param entities Entities object.
 * @param arrayEntityKey Key from entities that contain array of identifiers to remove from.
 * @param identifiersToRemove List of identifiers to be removed from provided entities.
 * @returns Copy of provided entities with identifiers removed from list on listKey.
 */
const removeValuesFromPropertyOfSubstate = <T extends object>(substate: Entities<T>, substatePropertyKey: string, substatePropertyValuesToRemove: string[]): Entities<T> =>
  _mapValues(substate, (entity: any) => ({
    ...entity,
    [substatePropertyKey]: entity[substatePropertyKey].filter((identifier: string) => !arrayHelper.contains(substatePropertyValuesToRemove, identifier)),
  }));

export {
  removeSectionsWithMissingType,
  removeWidgetsWithMissingType,
  removePersonalizationOfMissingConditionTypes,
  removeExcessZones,
};

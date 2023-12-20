import _flatmap from "lodash.flatmap";

import { getService } from "@/builder/container";
import { Entities, LocalizationService, WidgetZone } from "@/builder/declarations";
import { SERVICE_TYPES } from "@/builder/types";

/**
 * Maps widgets of the source zones to the target zones while keeping order of zones. Return a map that represent the state of widgets in zones.
 * @param sourceZones Source set of zones.
 * @param targetZones Target set of zones.
 * @param zoneWidgets Current state of widgets in zones.
 */
export const mapZoneWidgets = (sourceZones: string[], targetZones: string[], zoneWidgets: Entities<WidgetZone>): Entities<WidgetZone> => {
  if (!sourceZones) { return null; }
  if (!targetZones) { return zoneWidgets; }

  const leastCount = Math.min(sourceZones.length, targetZones.length);

  if (targetZones.length === 0) {
    throw new Error(getService<LocalizationService>(SERVICE_TYPES.LocalizationService).getLocalization("changesection.invalidsectionstate"));
  }

  // Copy widgets of common zones
  const newZoneWidgets: Entities<WidgetZone> = sourceZones.slice(0, leastCount).reduce((zones, zoneId, index) => {
    const widgets = zoneWidgets[zoneId].widgets;
    const targetZoneId = targetZones[index];
    zones[targetZoneId] = {
      identifier: targetZoneId,
      widgets: [...widgets],
    };

    return zones;
  }, {});

  // Add remaining zones if target section has more zones
  const targetZoneRemainingZones = targetZones.slice(leastCount);
  targetZoneRemainingZones.forEach((zoneId) => {
    newZoneWidgets[zoneId] = {
      identifier: zoneId,
      widgets: [],
    };
  });

  // Add all remaining widgets to the last zone if the target zone has less zones than the source
  const remainingZones = sourceZones.slice(leastCount);
  const remainingZonesWidgets = getWidgetsInZones(remainingZones, zoneWidgets);
  const [lastZoneInTargetSection] = targetZones.slice(-1);
  Array.prototype.push.apply(newZoneWidgets[lastZoneInTargetSection].widgets, remainingZonesWidgets);

  return newZoneWidgets;
};

/**
 * Gets all widgets in given zones and returns an array of their identifiers.
 * @param zones List of zones whose widgets are queried.
 * @param zoneState Map that describes what widgets each zone contains.
 * @returns Array of widget identifiers.
 */
export const getWidgetsInZones = (zones: string[], zoneState: Entities<WidgetZone>): string[] =>
  _flatmap(zones, (zoneId: string) => zoneState[zoneId].widgets);

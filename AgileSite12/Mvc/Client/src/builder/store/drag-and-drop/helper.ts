import { AreaConfiguration, Entities, Section, Widget, WidgetZone } from "@/builder/declarations";
import { arrayHelper } from "@/builder/helpers";
import { isWidgetAllowed } from "@/builder/helpers/widget-restrictions";
import { getWidgetsInZones } from "@/builder/store/zones/helpers";

const getBannedAreasForWidgets = (areasConfiguration: Entities<AreaConfiguration>, ...widgetTypes: string[]) => (
  Object.keys(areasConfiguration).filter((areaId: string) => {
    const { widgetRestrictions } = areasConfiguration[areaId];
    return widgetTypes.some((widgetType: string) => !isWidgetAllowed(widgetRestrictions, widgetType));
  })
);

const getBannedAreasForSection = (areasConfiguration: Entities<AreaConfiguration>, section: Section, zoneState: Entities<WidgetZone>, widgetState: Entities<Widget>): string[] => {
  // Get widget identifiers in section
  const widgetsInSection = getWidgetsInZones(section.zones, zoneState);

  // Get widget types in section
  const widgetTypes = arrayHelper.getUniqueValues(widgetsInSection, (widgetId: string) => widgetState[widgetId].type);

  return getBannedAreasForWidgets(areasConfiguration, ...widgetTypes);
};

export const dragAndDropHelper = {
  getBannedAreasForSection,
  getBannedAreasForWidgets,
};

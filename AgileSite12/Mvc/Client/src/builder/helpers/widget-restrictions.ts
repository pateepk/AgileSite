import { WidgetMetadata } from "@/builder/declarations/store";
import { arrayHelper } from "@/builder/helpers";

/**
 * Returns filtered widget list based on allowed widgets.
 * @param allowedWidgets List of allowed widgets.
 * @param widgetList List of widgets to be filtered.
 */
const filterWidgets = (allowedWidgets: string[], widgetList: WidgetMetadata[]): WidgetMetadata[] => {
  if (allowedWidgets && allowedWidgets.length === 0) {
    return widgetList;
  }

  return widgetList.filter((widget) => isWidgetAllowed(allowedWidgets, widget.typeIdentifier));
};

/**
 * Indicates if widget is allowed based on allowed widgets.
 * @param allowedWidgets List of allowed widgets.
 * @param widgetType Widget type identifier.
 */
const isWidgetAllowed = (allowedWidgets: string[], widgetType: string): boolean => {
  return !allowedWidgets || allowedWidgets.length === 0 || arrayHelper.contains(allowedWidgets, widgetType);
};

/**
 * Indicates whether a given area is banned for dropping widget.
 * @param bannedAreas List of banned areas.
 * @param area Area identifier.
 */
const isAreaBanned = (bannedAreas: string[], area: string): boolean => {
  return bannedAreas && arrayHelper.contains(bannedAreas, area);
};

export {
  filterWidgets,
  isWidgetAllowed,
  isAreaBanned,
};

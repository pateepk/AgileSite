import { ThunkAction } from "@/builder/declarations/store";
import { EntityType } from "@/builder/EntityType";

import { startDragging } from "./actions";
import { dragAndDropHelper } from "./helper";

/**
 * 'Start dragging widget' thunk.
 * @param widgetIdentifier Dragged widget identifier.
 * @param zoneIdentifier Identifier of the zone which contains the dragged widget.
 */
const startDraggingWidget = (widgetIdentifier: string, zoneIdentifier: string): ThunkAction => (dispatch, getState) => {
  const { editableAreasConfiguration, widgets, zones } = getState();
  const widgetType = widgets[widgetIdentifier].type;
  const position = zones[zoneIdentifier].widgets.indexOf(widgetIdentifier);
  const bannedAreas = dragAndDropHelper.getBannedAreasForWidgets(editableAreasConfiguration, widgetType);

  dispatch(startDragging(EntityType.Widget, widgetType, widgetIdentifier, zoneIdentifier, position, bannedAreas));
};

/**
 * 'Start dragging section' thunk.
 * @param sectionIdentifier Dragged section identifier.
 * @param areaIdentifier Identifier of the editable area which contains the dragged section.
 */
const startDraggingSection = (sectionIdentifier: string, areaIdentifier: string): ThunkAction => (dispatch, getState) => {
  const { editableAreas, editableAreasConfiguration, sections, zones, widgets } = getState();
  const sectionType = sections[sectionIdentifier].type;
  const position = editableAreas[areaIdentifier].sections.indexOf(sectionIdentifier);
  const bannedAreas = dragAndDropHelper.getBannedAreasForSection(editableAreasConfiguration, sections[sectionIdentifier], zones, widgets);

  dispatch(startDragging(EntityType.Section, sectionType, sectionIdentifier, areaIdentifier, position, bannedAreas));
};

export {
  startDraggingWidget,
  startDraggingSection,
};

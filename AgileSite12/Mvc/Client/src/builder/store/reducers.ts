import { BuilderConfiguration, Metadata, PageTemplate } from "../declarations";
import { displayedWidgetVariants } from "./displayed-widget-variants/reducer";
import { dragAndDrop } from "./drag-and-drop/reducer";
import { editableAreas } from "./editable-areas/reducer";
import { highlightedSection } from "./highlighted-section/reducer";
import { highlightedWidget } from "./highlighted-widget/reducer";
import { markups } from "./markups/reducer";
import { modalDialogs } from "./modal-dialogs/reducer";
import { popup } from "./pop-up/reducer";
import { sections } from "./sections/reducer";
import { widgetSelection } from "./widget-selection/reducer";
import { widgetVariants } from "./widget-variants/reducer";
import { widgets } from "./widgets/reducer";
import { zones } from "./zones/reducer";

/**
 * Reducers which are common to all builders.
 */
export const baseReducers = {
  config: (state: BuilderConfiguration = null) => state,
  displayedWidgetVariants,
  dragAndDrop,
  editableAreas,
  highlightedSection,
  highlightedWidget,
  markups,
  popup,
  sections,
  widgets,
  widgetSelection,
  widgetVariants,
  zones,
  pageTemplate: (state: PageTemplate = {
    identifier: "",
  }) => state,
  editableAreasConfiguration: (state = {}) => state,
  metadata: (state: Metadata = {
    personalizationConditionTypes: {},
    sections: {},
    widgets: {},
    pageTemplates: {},
  }) => state,
  modalDialogs,
};

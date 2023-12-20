import * as d from "../";

export interface WidgetComponentContext {
  widgetIdentifier: string;
  position: number;
  zoneIdentifier: string;
  isDropBanned: boolean;
  areaIdentifier: string;
  friendlyElementIds: string[];
  availableWidgetTypes: d.PopupListingElement[];
}

export interface WidgetComponentState {
  dragAndDrop: d.DragAndDrop;
  selectedWidgetIdentifier: string;
  highlightedWidgetIdentifier: string;
  popup: d.Popup;
  widget: d.Widget;
  displayedVariant: d.WidgetVariant;
  displayedVariantMarkup: string;
  widgetTitle: string;
  isFrozen: boolean;
  isClickAwayDisabled: boolean;
}

export interface WidgetComponentActions {
  highlightWidget;
  dehighlightWidget;
  selectWidget;
  unselectWidget;
  enableWidgetClickAway;
  closePopup: () => void;
  openPopup;
  setWidgetProperty;
}

export interface WidgetComponentProperties extends WidgetComponentContext, WidgetComponentState, WidgetComponentActions {
}

import * as d from "../";

export interface WidgetZoneComponentContext {
  sectionIdentifier: string;
  zoneIndex: number;
  areaIdentifier: string;
}

export interface WidgetZoneComponentState {
  zoneIdentifier: string;
  dragAndDrop: d.DragAndDrop;
  zone: d.WidgetZone;
  popup: d.Popup;
  widgetsInZone: d.Widget[];
  availableWidgetTypes: d.PopupListingElement[];
}

export interface WidgetZoneComponentActions {
  removeWidget;
  closePopup: () => void;
  openPopup;
}

export interface WidgetZoneComponentProperties extends WidgetZoneComponentContext, WidgetZoneComponentState, WidgetZoneComponentActions {
}

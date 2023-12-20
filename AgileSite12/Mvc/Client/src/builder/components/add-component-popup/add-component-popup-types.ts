import { Popup, PopupListingElement } from "@/builder/declarations";

export type ClosePopup = () => void;

export interface AddComponentPopupComponentState {
  popup: Popup;
  isDnDActive: boolean;
  items: PopupListingElement[];
}

export interface AddComponentPopupComponentActions {
  closePopup: ClosePopup;
}

export interface AddComponentPopupComponentContext {
  selectItem: (componentName: string) => void;
}

export interface AddComponentPopupComponentProperties extends AddComponentPopupComponentState, AddComponentPopupComponentActions, AddComponentPopupComponentContext {
}

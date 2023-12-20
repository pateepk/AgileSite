import { PopupListingElement } from "@/builder/declarations";
import { ComponentPosition, PopupType } from "@/builder/store/types";

export type ClosePopup = () => void;
export type OpenPopup = (componentIdentifier: string, position: ComponentPosition, popupType: PopupType) => void;

export interface SectionHeaderComponentContext {
  areaIdentifier: string;
  sectionIdentifier: string;
  showSectionTypeList: boolean;
  sectionTypes: PopupListingElement[];
  sectionType: string;
}

export interface SectionHeaderComponentState {
  sectionTypeListPosition: ComponentPosition;
  sectionHeaderPosition: ComponentPosition;
}

export interface SectionHeaderComponentActions {
  openPopup: OpenPopup;
  closePopup: ClosePopup;
}

export interface SectionHeaderComponentProperties extends SectionHeaderComponentContext, SectionHeaderComponentState, SectionHeaderComponentActions {
}

import { ComponentPosition, ListingOffset, PopupType } from "@/builder/store/types";

export type ClosePopup = () => void;
export type OpenPopup = (componentIdentifier: string, position: ComponentPosition, popupType: PopupType, parentClientRect: ClientRect,
                         areaIdentifier: string) => void;
export type SetPopupPosition = (position: ComponentPosition, listingOffset: ListingOffset) => void;

export interface SectionDividerComponentContext {
  sectionIdentifier: string;
  areaIdentifier: string;
}

export interface SectionDividerComponentState {
  isSectionListOpen: boolean;
}

export interface SectionDividerComponentActions {
  closePopup: ClosePopup;
  openPopup: OpenPopup;
  setPopupPosition: SetPopupPosition;
}

export interface SectionDividerComponentProperties extends SectionDividerComponentContext, SectionDividerComponentState, SectionDividerComponentActions {
}

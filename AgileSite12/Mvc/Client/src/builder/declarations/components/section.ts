import * as d from "../";
import { PopupListingElement } from "./pop-up-listing";

export interface SectionComponentContext {
  areaIdentifier: string;
  sectionIdentifier: string;
  position: number;
}

export interface SectionComponentState {
  sectionType: string;
  sectionMarkup: string;
  sectionTypes: PopupListingElement[];
  popup: d.Popup;
  highlightedSection: d.HighlightedSection;
  dragAndDrop: d.DragAndDrop;
}

export interface SectionComponentActions {
  showSectionHeader;
  hideSectionHeader;
  highlightSectionBorder;
  dehighlightSectionBorder;
  closePopup: () => void;
}

export interface SectionComponentProperties extends SectionComponentContext, SectionComponentState, SectionComponentActions {
}

import { getService } from "@/builder/container";
import { IPopUpElementsService, SectionComponentActions, SectionComponentContext, SectionComponentState, State } from "@/builder/declarations";
import { connect } from "@/builder/helpers/connector";
import { closePopup, dehighlightSectionBorder, hideSectionHeader, highlightSectionBorder, showSectionHeader } from "@/builder/store/actions";
import { SERVICE_TYPES } from "@/builder/types";

import Section from "./Section.vue";

const mapStateToProps = (state: State, { sectionIdentifier }: SectionComponentContext): SectionComponentState => {
  const { sections, markups, metadata, popup, highlightedSection, dragAndDrop } = state;
  const sectionType = sections[sectionIdentifier] ? sections[sectionIdentifier].type : "";
  const popUpElementsService = getService<IPopUpElementsService>(SERVICE_TYPES.PopUpElementsService);
  return {
    sectionType,
    sectionMarkup: markups.sections[sectionIdentifier] ? markups.sections[sectionIdentifier].markup : "",
    sectionTypes: popUpElementsService.getSectionElements(Object.values(metadata.sections)),
    popup,
    highlightedSection,
    dragAndDrop,
  };
};

const mapDispatchToProps = (): SectionComponentActions => ({
  showSectionHeader,
  hideSectionHeader,
  highlightSectionBorder,
  dehighlightSectionBorder,
  closePopup,
});

export default connect(mapStateToProps, mapDispatchToProps)(Section);

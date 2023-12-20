import { connect, MapDispatchToProps, MapStateToProps } from "@/builder/helpers/connector";
import { closePopup, openPopup } from "@/builder/store/pop-up/actions";
import { PopupType, State } from "@/builder/store/types";

import {
  SectionHeaderComponentActions,
  SectionHeaderComponentContext,
  SectionHeaderComponentState,
} from "./section-header-types";
import SectionHeader from "./SectionHeader.vue";

type StateToProps = MapStateToProps<State, SectionHeaderComponentContext, SectionHeaderComponentState>;
type DispatchToProps = MapDispatchToProps<SectionHeaderComponentActions>;

const mapStateToProps: StateToProps = ({ highlightedSection, popup }, { sectionIdentifier }) => ({
  sectionTypeListPosition: highlightedSection.sectionIdentifier === sectionIdentifier && popup.popupType === PopupType.ChangeSection
    ? popup.position
    : null,
  sectionHeaderPosition: highlightedSection.sectionIdentifier === sectionIdentifier ? highlightedSection.headerPosition : null,
});

const mapDispatchToProps: DispatchToProps = () => ({
  openPopup,
  closePopup,
});

export default connect(mapStateToProps, mapDispatchToProps)(SectionHeader);

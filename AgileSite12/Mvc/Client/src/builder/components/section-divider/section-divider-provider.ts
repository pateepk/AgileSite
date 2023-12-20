import { connect, MapDispatchToProps, MapStateToProps } from "@/builder/helpers/connector";
import { closePopup, setPopupPosition } from "@/builder/store/pop-up/actions";
import { openPopup } from "@/builder/store/pop-up/thunks";
import { PopupType, State } from "@/builder/store/types";

import {
  SectionDividerComponentActions,
  SectionDividerComponentContext,
  SectionDividerComponentState,
} from "./section-divider-types";
import SectionDivider from "./SectionDivider.vue";

type StateToProps = MapStateToProps<State, SectionDividerComponentContext, SectionDividerComponentState>;
type DispatchToProps = MapDispatchToProps<SectionDividerComponentActions>;

const mapStateToProps: StateToProps = ({ popup }, { sectionIdentifier }) => ({
  isSectionListOpen: popup.componentIdentifier === sectionIdentifier && popup.popupType === PopupType.AddSection,
});

const mapDispatchToProps: DispatchToProps = () => ({
  closePopup,
  openPopup,
  setPopupPosition,
});

export default connect(mapStateToProps, mapDispatchToProps)(SectionDivider);

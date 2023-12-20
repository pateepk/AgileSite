import { connect } from "@/builder/helpers/connector";

import { State } from "@/builder/declarations/store";
import { SectionHeaderExtraButtonsContext, SectionHeaderExtraButtonsState } from "@/page-builder/declarations";
import SectionHeaderExtraButtons from "./SectionHeaderExtraButtons.vue";

const mapStateToProps = (state: State, propsData: SectionHeaderExtraButtonsContext): SectionHeaderExtraButtonsState => {
  const section = state.sections[propsData.sectionIdentifier];
  const hasProperties = section ? state.metadata.sections[section.type].hasProperties : false;

  return {
    showPropertiesButton: hasProperties,
  };
};

export default connect(mapStateToProps)(SectionHeaderExtraButtons);

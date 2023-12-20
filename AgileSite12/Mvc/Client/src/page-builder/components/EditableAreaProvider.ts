import { EditableAreaComponentActions, EditableAreaComponentContext, EditableAreaComponentState, State } from "@/builder/declarations";
import { connect } from "@/builder/helpers/connector";
import { changeSection, removeSection } from "@/page-builder/store/thunks";

import EditableArea from "@/builder/components/EditableArea.vue";

const mapStateToProps = (state: State, propsData: EditableAreaComponentContext): EditableAreaComponentState => ({
  sectionsInArea: state.editableAreas[propsData.identifier].sections,
  dragAndDrop: state.dragAndDrop,
});

const mapDispatchToProps = (): EditableAreaComponentActions => ({
  removeSection,
  changeSection,
});

export default connect(mapStateToProps, mapDispatchToProps)(EditableArea);

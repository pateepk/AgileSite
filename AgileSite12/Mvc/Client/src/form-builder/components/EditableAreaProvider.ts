import { EditableAreaComponentActions, EditableAreaComponentContext, EditableAreaComponentState } from "@/builder/declarations";
import { connect } from "@/builder/helpers/connector";
import { FormBuilderState } from "@/form-builder/declarations";
import { changeSection, removeSection } from "@/form-builder/store/thunks";

import EditableArea from "@/builder/components/EditableArea.vue";

const mapStateToProps = (state: FormBuilderState, propsData: EditableAreaComponentContext): EditableAreaComponentState => ({
  sectionsInArea: state.editableAreas[propsData.identifier].sections,
  dragAndDrop: state.dragAndDrop,
});

const mapDispatchToProps = (): EditableAreaComponentActions => ({
  removeSection,
  changeSection,
});

export default connect(mapStateToProps, mapDispatchToProps)(EditableArea);

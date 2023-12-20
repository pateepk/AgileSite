import { connect } from "@/builder/helpers/connector";
import VisibilityConditionEditorForm from "@/form-builder/components/VisibilityConditionEditorForm.vue";
import { FormBuilderState } from "@/form-builder/declarations";
import { VisibilityConditionEditorFormState } from "@/form-builder/declarations/components/visibility-condition-editor-form";

const mapStateToProps = (state: FormBuilderState): VisibilityConditionEditorFormState => ({
  refreshPropertiesPanelsNotifier: state.refreshPropertiesPanelsNotifier,
});

export default connect(mapStateToProps)(VisibilityConditionEditorForm);

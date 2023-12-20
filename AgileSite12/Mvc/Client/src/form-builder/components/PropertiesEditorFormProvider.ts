import { connect } from "@/builder/helpers/connector";
import { FormBuilderState } from "@/form-builder/declarations";

import { PropertiesEditorFormComponentState } from "@/form-builder/declarations";
import PropertiesEditorForm from "./PropertiesEditorForm.vue";

const mapStateToProps = (state: FormBuilderState): PropertiesEditorFormComponentState => ({
  refreshPropertiesPanelsNotifier: state.refreshPropertiesPanelsNotifier,
});

export default connect(mapStateToProps)(PropertiesEditorForm);

import { connect } from "@/builder/helpers/connector";
import { FormBuilderState } from "@/form-builder/declarations";

import { ValidationRuleConfigurationFormComponentProperties } from "@/form-builder/declarations/components/validation-rule-configuration-form";
import ValidationRuleConfigurationForm from "./ValidationRuleConfigurationForm.vue";

const mapStateToProps = (state: FormBuilderState): ValidationRuleConfigurationFormComponentProperties => ({
  refreshPropertiesPanelsNotifier: state.refreshPropertiesPanelsNotifier,
});

export default connect(mapStateToProps)(ValidationRuleConfigurationForm);

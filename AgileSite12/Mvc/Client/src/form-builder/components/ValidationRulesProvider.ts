import { connect } from "@/builder/helpers/connector";
import { FormBuilderState, ValidationRulesComponentActions, ValidationRulesComponentState } from "@/form-builder/declarations";
import { createNewValidationRuleIdentifier, expandValidationRule } from "@/form-builder/store/actions";

import { freezeWidgetSelection } from "@/builder/store/actions";
import ValidationRules from "./ValidationRules.vue";

const mapStateToProps = (state: FormBuilderState): ValidationRulesComponentState => ({
  expandedValidationRuleIdentifier: state.expandedValidationRuleIdentifier,
  newValidationRuleIdentifiers: state.newValidationRuleIdentifiers,
});

const mapDispatchToProps = (): ValidationRulesComponentActions => ({
  expandValidationRule,
  createNewValidationRuleIdentifier,
  freezeWidgetSelection,
});

export default connect(mapStateToProps, mapDispatchToProps)(ValidationRules);

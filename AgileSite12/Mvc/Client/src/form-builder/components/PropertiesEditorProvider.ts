import { connect } from "@/builder/helpers/connector";
import { disableWidgetClickAway, freezeWidgetSelection, thawWidgetSelection } from "@/builder/store/actions";
import { FormBuilderState, PropertiesEditorComponentActions, PropertiesEditorComponentState } from "@/form-builder/declarations";
import { setWidgetProperties } from "@/form-builder/store/widgets/thunks";

import { expandValidationRule, newValidationRuleAdded } from "@/form-builder/store/actions";

import PropertiesEditor from "./PropertiesEditor.vue";

const mapStateToProps = (state: FormBuilderState): PropertiesEditorComponentState => ({
  validationRulesMetadataPerType: state.validationRulesMetadata,
  widgetMetadata: state.metadata.widgets,
  widgetVariants: state.widgetVariants,
  widgets: state.widgets,
  widgetSelection: state.widgetSelection,
  savingInProgress: state.savingInProgress,
});

const mapDispatchToProps = (): PropertiesEditorComponentActions => ({
  expandValidationRule,
  freezeWidgetSelection,
  newValidationRuleAdded,
  setWidgetProperties,
  thawWidgetSelection,
  disableWidgetClickAway,
});

export default connect(mapStateToProps, mapDispatchToProps)(PropertiesEditor);

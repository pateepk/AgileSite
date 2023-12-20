import { connect } from "@/builder/helpers/connector";

import { openDialog } from "@/builder/store/modal-dialogs/thunks";

import { ChangeTemplateButtonComponentActions, ChangeTemplateButtonComponentState, PageBuilderState } from "../declarations";
import ChangeTemplateButton from "./ChangeTemplateButton.vue";

const mapStateToProps = ({ config, pageTemplate }: PageBuilderState): ChangeTemplateButtonComponentState => ({
  currentTemplateIdentifier: pageTemplate.configurationIdentifier ? pageTemplate.configurationIdentifier : pageTemplate.identifier,
  dialogUrl: config.pageTemplate.selectorDialogEndpoint,
});

const mapDispatchToProps = (): ChangeTemplateButtonComponentActions => ({
  openModalDialog: openDialog,
});

export default connect(mapStateToProps, mapDispatchToProps)(ChangeTemplateButton);

import { connect } from "@/builder/helpers/connector";
import { ModalDialogType } from "@/builder/store/types";

import { PageBuilderGlobalComponentWrapperComponentState } from "../declarations";
import { PageBuilderState } from "../store/types";
import PageBuilderGlobalComponentWrapper from "./PageBuilderGlobalComponentWrapper.vue";

const mapStateToProps = ({ config, modalDialogs, pageTemplate, metadata }: PageBuilderState): PageBuilderGlobalComponentWrapperComponentState => ({
  hasTemplate: !!pageTemplate && config.pageTemplate.isSelectable,
  hasTemplateProperties: !!pageTemplate && pageTemplate.identifier && metadata.pageTemplates[pageTemplate.identifier].hasProperties,
  templateIdentifier: pageTemplate && pageTemplate.identifier,
  showPropertiesDialog: !!modalDialogs.dialogs.length && modalDialogs.dialogs[0].type !== ModalDialogType.Custom,
});

export default connect(mapStateToProps)(PageBuilderGlobalComponentWrapper);

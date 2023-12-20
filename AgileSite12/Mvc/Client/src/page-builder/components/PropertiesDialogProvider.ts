import { connect, MapDispatchToProps, MapStateToProps } from "@/builder/helpers/connector";
import { closeModalDialog } from "@/builder/store/thunks";

import {
  PageBuilderState,
  PropertiesDialogComponentActions,
  PropertiesDialogComponentState,
} from "@/page-builder/declarations";
import { fetchModalDialogMarkup, setPageTemplateProperties, setSectionProperties, setWidgetProperties } from "@/page-builder/store/thunks";

import { SubmitPropertiesDialogFormFactory } from "../store/modal-dialogs/factories";
import PropertiesDialog from "./PropertiesDialog.vue";

type PropertiesDialogStateToProps = MapStateToProps<PageBuilderState, object, PropertiesDialogComponentState>;
type PropertiesDialogDispatchToProps = MapDispatchToProps<PropertiesDialogComponentActions>;

const mapStateToProps: PropertiesDialogStateToProps = (state) => {
  const { modalDialogs } = state;
  const currentDialog = modalDialogs.dialogs[0];

  if (!currentDialog) {
    return null;
  }

  const { type, markup, isValid, title, markupUrl, model } = currentDialog;

  return {
    componentName: title,
    dialogType: type,
    dialogTheme: modalDialogs.theme,
    formMarkup: markup,
    properties: model,
    propertiesFormUrl: markupUrl,
    formIsValid: isValid,
    dialogsCount: modalDialogs.dialogs.length,
  };
};

const mapDispatchToProps: PropertiesDialogDispatchToProps = () => ({
  closeDialog: closeModalDialog,
  fetchMarkup: fetchModalDialogMarkup,
  submitDialogForm: new SubmitPropertiesDialogFormFactory(setWidgetProperties, setSectionProperties, setPageTemplateProperties).submitPropertiesDialogForm,
});

export default connect(mapStateToProps, mapDispatchToProps)(PropertiesDialog);

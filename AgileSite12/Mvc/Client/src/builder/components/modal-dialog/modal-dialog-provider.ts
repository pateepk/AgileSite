import { connect, MapDispatchToProps, MapStateToProps } from "@/builder/helpers/connector";

import { State } from "@/builder/declarations";
import { applyDialog, closeDialog } from "@/builder/store/modal-dialogs/thunks";

import { ModalDialogComponentActions, ModalDialogComponentContext, ModalDialogComponentState } from "./modal-dialog-types";
import ModalDialog from "./ModalDialog.vue";

type ModalDialogStateToProps = MapStateToProps<State, ModalDialogComponentContext, ModalDialogComponentState>;
type ModalDialogDispatchToProps = MapDispatchToProps<ModalDialogComponentActions>;

const mapStateToProps: ModalDialogStateToProps = ({ modalDialogs }, { dialogIndex }) => {
  const dialog = modalDialogs.dialogs[dialogIndex];
  const openedDialogsCount = modalDialogs.dialogs.length;

  return {
    dialog,
    openedDialogsCount,
    theme: modalDialogs.theme,
  };
};

const mapDispatchToProps: ModalDialogDispatchToProps = () => ({
  closeDialog,
  applyDialog,
});

export default connect(mapStateToProps, mapDispatchToProps)(ModalDialog);

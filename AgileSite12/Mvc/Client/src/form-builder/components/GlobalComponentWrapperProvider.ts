import GlobalComponentWrapper from "@/builder/components/GlobalComponentWrapper.vue";
import { GlobalComponentWrapperComponentActions, GlobalComponentWrapperComponentState } from "@/builder/declarations/components";
import { connect } from "@/builder/helpers/connector";
import { SelectItemThunkFactory } from "@/builder/store/pop-up/factories";
import { ModalDialogType } from "@/builder/store/types";

import { addSection, addWidget } from "../store/thunks";
import { FormBuilderState } from "../store/types";

const mapStateToProps = ({ modalDialogs, popup }: FormBuilderState): GlobalComponentWrapperComponentState => ({
  modalDialogs: modalDialogs.dialogs.filter((dialog) => dialog.type === ModalDialogType.Custom),
  popupType: popup.popupType,
});

const mapDispatchToProps = (): GlobalComponentWrapperComponentActions => ({
  selectItem: new SelectItemThunkFactory(addWidget, addSection).selectItem,
});

export default connect(mapStateToProps, mapDispatchToProps)(GlobalComponentWrapper);

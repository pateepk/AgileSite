import { ModalDialogOptions } from "@/builder/declarations";

type OpenModalDialog = ((options: ModalDialogOptions) => void);

export interface ChangeTemplateButtonComponentContext {
  readonly buttonTooltip: string;
}

export interface ChangeTemplateButtonComponentActions {
  readonly openModalDialog: OpenModalDialog;
}

export interface ChangeTemplateButtonComponentState {
  readonly currentTemplateIdentifier: string;
  readonly dialogUrl: string;
}

export interface ChangeTemplateButtonComponentProperties extends ChangeTemplateButtonComponentState, ChangeTemplateButtonComponentContext, ChangeTemplateButtonComponentActions { }

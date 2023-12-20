import { Theme } from "@/builder/types";

import { ModalDialog } from "@/builder/declarations";

export type ApplyDialog = (dialogWindow: Window) => void;
export type CloseDialog = (dialogWindow: Window) => void;

export interface ModalDialogComponentContext {
  readonly dialogIndex: number;
}

export interface ModalDialogComponentState {
  readonly dialog: ModalDialog;
  readonly theme: Theme;
  readonly openedDialogsCount: number;
}

export interface ModalDialogComponentActions {
  readonly applyDialog: ApplyDialog;
  readonly closeDialog: CloseDialog;
}

export interface ModalDialogComponentProperties
  extends ModalDialogComponentContext, ModalDialogComponentState, ModalDialogComponentActions {}

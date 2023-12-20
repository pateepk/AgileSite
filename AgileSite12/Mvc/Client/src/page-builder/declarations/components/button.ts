import { ModalDialogType, OpenModalDialogOptions } from "@/builder/store/types";

type OpenModalDialog = ((options: OpenModalDialogOptions) => void);

export interface ButtonComponentContext {
  readonly identifier: string;
  readonly buttonTooltip: string;
  readonly dialogType: ModalDialogType;
}

export interface ButtonComponentActions {
  readonly openModalDialog: OpenModalDialog;
}

export interface ButtonComponentProperties extends ButtonComponentContext, ButtonComponentActions { }

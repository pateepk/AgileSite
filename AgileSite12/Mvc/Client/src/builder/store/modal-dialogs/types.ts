import { ActionType } from "typesafe-actions";

import { Theme } from "@/builder/types";
import * as actions from "./actions";
import { OpenModalDialogFactory } from "./factories";

export enum ModalDialogType {
  WidgetProperties = "WidgetProperties",
  SectionProperties = "SectionProperties",
  TemplateProperties = "TemplateProperties",
  Custom = "Custom",
}

export interface OpenModalDialogOptions {
  readonly componentIdentifier: string;
  readonly dialogType: ModalDialogType;
  readonly showFooter: boolean;
  readonly dialogTitle?: string;
  readonly dialogMarkupUrl?: string;
  readonly dialogMarkupModel?: object;
  readonly dialogTheme?: Theme;
  readonly dialogWidth?: string;
  readonly maximized?: boolean;
  readonly applyButtonText?: string;
  readonly cancelButtonText?: string;
}

export interface DialogData {
  readonly title: string;
  readonly markupUrl: string;
  readonly model: object;
  readonly theme: Theme;
}

export type OpenModalDialogAction = ActionType<typeof actions.openModalDialog>;
export type CloseModalDialogAction = ActionType<typeof actions.closeModalDialog>;
export type ModalDialogsAction = ActionType<typeof actions>;

export type OpenModalDialogThunk = typeof OpenModalDialogFactory.prototype.openModalDialog;

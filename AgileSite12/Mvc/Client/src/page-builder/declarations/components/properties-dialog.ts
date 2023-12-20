import { Theme } from "@/builder/types";

export interface PropertiesDialogComponentState {
  readonly formMarkup: string;
  readonly properties: object;
  readonly propertiesFormUrl: string;
  readonly componentName: string;
  readonly formIsValid: boolean;
  readonly dialogTheme: Theme;
  readonly dialogsCount: number;
}

export interface PropertiesDialogComponentActions {
  readonly closeDialog: (dialogIndex: number) => void;
  readonly fetchMarkup: (dialogIndex: number, formUrl: string, properties: object) => void;
  readonly submitDialogForm: (dialogIndex: number, actionUrl: string, currentProperties: object, formData: FormData) => void;
}

export interface PropertiesDialogComponentProperties
  extends PropertiesDialogComponentState, PropertiesDialogComponentActions {}

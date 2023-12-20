import { Theme } from "@/builder/types";

export interface ModalDialogConfiguration {
  readonly title?: string;
  readonly theme: Theme;
  readonly contentUrl: string;
  readonly dialogIndex: number;
  readonly width: string;
  readonly maximized: boolean;
  readonly openedDialogsCount: number;
  readonly showFooter: boolean;
  readonly showLoader: boolean;
  readonly cancelButtonText: string;
  readonly applyButtonText: string;
}

export interface ModalDialogMessages {
  readonly unsavedChanges: string;
  readonly loaderMessage: string;
  readonly closeTooltip: string;
  readonly cancelTooltip?: string;
  readonly applyTooltip?: string;
}

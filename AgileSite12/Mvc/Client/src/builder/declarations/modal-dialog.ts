import { Theme } from "@/builder/types";

type ModalDialogApplyCallbackReturnValue = ModalDialogApplyCallbackResult | void;
export type ModalDialogApplyCallback = ModalDialogApplyCallbackReturnValue | Promise<ModalDialogApplyCallbackReturnValue>;

export interface ModalDialogApplyCallbackResult {
  /**
   * Determines whether the dialog should be closed after callback execution.
   */
  readonly closeDialog?: boolean;
}

export interface ModalDialogCallbacks {
  readonly applyCallback: (window: Window) => ModalDialogApplyCallback;
  readonly cancelCallback: (window: Window) => void | Promise<void>;
}

export interface ModalDialogOptions {
  /**
   * Dialog title.
   */
  readonly title?: string;

  /**
   * Dialog theme.
   */
  readonly theme?: Theme;

  /**
   * Dialog width.
   */
  readonly width?: string;

  /**
   * Forces the dialog to display in maximized size. If set to true, the width property is ignored.
   */
  readonly maximized?: boolean;

  /**
   * Apply button text.
   */
  readonly applyButtonText?: string;

  /**
   * Cancel button text.
   */
  readonly cancelButtonText?: string;

  /**
   * Function which is called when the dialog is being cancelled and closed.
   */
  readonly cancelCallback?: (window: Window) => void | Promise<void>;
}

export interface CustomModalDialogOptions extends ModalDialogOptions {
  /**
   * Url which provides the dialog content.
   */
  readonly url: string;

  /**
   * Function which is called when the apply button on the dialog is clicked.
   */
  readonly applyCallback: (window: Window) => ModalDialogApplyCallback;

  /**
   * Data that should be provided to the dialog.
   */
  readonly data?: any;

  /**
   * Controls the dialog footer visibility.
   */
  readonly showFooter?: boolean;
}

/**
 * Modal dialog API.
 */
export interface ModalDialogService {
  /**
   * Opens a modal dialog.
   * @param options Modal dialog options.
   */
  open: (options: ModalDialogOptions) => void;

  /**
   * Applies the modal dialog.
   */
  apply: (window: Window) => void;

  /**
   * Closes the modal dialog.
   */
  cancel: (window: Window) => void;

  /**
   * Gets the data for the modal dialog.
   */
  getData: () => any;
}

import { Component, Element, Event, EventEmitter, Method, Prop, State } from "@stencil/core";

import { ModalDialogConfiguration, ModalDialogMessages } from "./modal-dialog-types";

const BASE_Z_INDEX = 10200;
const DIALOG_DISTANCE = 100;

@Component({
  tag: "kentico-modal-dialog",
  styleUrl: "modal-dialog.less",
  shadow: true,
})
export class ModalDialog {
  dialogElement: HTMLElement;
  dialogHeader: HTMLElement;
  submitButton: HTMLElement;

  baseUnit = 16;
  isModified = false;
  dialogHeight = 250;

  defaultMessages: ModalDialogMessages = {
    closeTooltip: "Close",
    unsavedChanges: `Are you sure you want to close the dialog?

    You have unsaved changes.

    Press OK to continue or Cancel to stay on the current page.`,
    loaderMessage: "Loading",
  };

  @State() topPosition: string;

  @Element() hostElement: HTMLElement;

  @Prop() configuration: ModalDialogConfiguration;
  @Prop() messages: ModalDialogMessages;

  @Event() close: EventEmitter;
  @Event() apply: EventEmitter;

  @Method()
  adjustDialogHeight(wholeDialogHeight: number): void {
    this.dialogHeight = wholeDialogHeight;
    this.setDialogPosition();
  }

  get overlayStyle() {
    return {
      zIndex: this.getZIndex(this.configuration.openedDialogsCount - 1).toString(),
    };
  }

  get modalDialogStyle() {
    return {
      zIndex: (this.getZIndex(this.configuration.dialogIndex) + 1).toString(),
      top: this.topPosition,
      width: `${this.getDialogWidth()}px`,
    };
  }

  componentDidLoad() {
    this.setDialogPosition();
  }

  /**
   * Gets a calculated width in pixels of the dialog according to its size.
   * @returns Integer representing a width.
   */
  getDialogWidth = () => {
    const widthValue = parseInt(this.configuration.width, 10);

    if (this.configuration.maximized) {
      return window.document.documentElement.clientWidth - 2 * this.baseUnit;
    } else if (this.configuration.width.endsWith("%")) {
      return Math.floor(window.document.documentElement.clientWidth * (widthValue / 100));
    } else if (this.configuration.width.endsWith("px")) {
      return widthValue;
    }
  }

  getZIndex(currentIndex: number) {
    return BASE_Z_INDEX + currentIndex * DIALOG_DISTANCE;
  }

  onDialogClosed = (event: CustomEvent<any> | MouseEvent) => {
    event.stopPropagation();

    if (this.isModified && !confirm(this.messages.unsavedChanges)) {
      return;
    }

    this.close.emit();
  }

  combineWithDefaultMessages(messages: ModalDialogMessages = this.defaultMessages): ModalDialogMessages {
    return Object.assign({}, this.defaultMessages, messages);
  }

  /**
   * Gets the CSS "top" property value for the dialog according to its size.
   * @param elementHeight Height of a whole dialog in pixels.
   * @param document Document object.
   */
  getTopPosition = (elementHeight: number, document: HTMLDocument): string => {
    const windowHeight = document.documentElement.clientHeight;

    return `calc((${windowHeight}px - ${elementHeight}px)/2)`;
  }

  setDialogPosition = (): void => {
    this.topPosition = this.getTopPosition(this.dialogHeight, document);
  }

  render = (): JSX.Element => {
    const { dialogIndex, theme, showFooter, title, applyButtonText, cancelButtonText } = this.configuration;
    const { applyTooltip, closeTooltip } = this.combineWithDefaultMessages(this.messages);

    return (
      <div class="ktc-form-wrapper" onWheel={(event) => event.preventDefault()} onClick={(event) => event.stopPropagation()}>
        { !dialogIndex && <div class="ktc-modal-dialog__overlay" style={this.overlayStyle} /> }
        <div class="ktc-modal-dialog" ref={(el) => this.dialogElement = el} style={this.modalDialogStyle}>
          <kentico-dialog-header
            ref={(el) => this.dialogHeader = el}
            headerTitle={title}
            theme={theme}
            closeTooltip={closeTooltip}
            onClose={this.onDialogClosed} />
          { this.configuration.showLoader &&  <div class={showFooter ? "ktc-loader-container-wrapper with-footer" : "ktc-loader-container-wrapper"}>

                      <kentico-loader loaderMessage={this.messages.loaderMessage} />
              </div> }
          <slot></slot>
          { showFooter &&
            <div class="ktc-modal-dialog__footer">
              <div class="ktc-modal-dialog__footer-buttons">
                <button type="button" class="ktc-btn ktc-btn-default" onClick={this.onDialogClosed}>{cancelButtonText}</button>
                <div class="ktc-modal-dialog__footer-buttons-spacer"></div>
                <button type="button" class="ktc-btn ktc-btn-primary"
                  ref={(el) => this.submitButton = el}
                  title={applyTooltip}
                  onClick={() => this.apply.emit()}> {applyButtonText} </button>
              </div>
            </div>
          }
        </div>
      </div>
    );
  }
}

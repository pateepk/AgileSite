import { Component, Event, EventEmitter, Listen, Prop, Watch } from "@stencil/core";

import { LocalizationService } from "@/builder/declarations/index";
import { removeScriptElements, renderMarkup } from "@/builder/helpers/markup-helper";
import { disablePageScrolling, enablePageScrolling } from "@/builder/services/modal-dialog/scrolling";
import { Theme } from "@/builder/types";

const BASE_Z_INDEX = 10200;
const DIALOG_DISTANCE = 100;

@Component({
  tag: "kentico-form-modal-dialog",
  styleUrl: "form-modal-dialog.less",
  shadow: false,
})
export class FormModalDialog {
  dialogElement: HTMLElement;
  dialogHeader: HTMLElement;
  formWrapper: HTMLElement;
  submitButton: HTMLElement;
  isModified = false;
  baseUnit = 16;
  externalScriptIdentifier = "ktc-form-modal-dialog-script";
  modalDialogContentElementClass = "ktc-form-modal-content";
  modalDialogFooterClass = "ktc-form-modal-footer";

  @Prop() localizationService: LocalizationService;
  @Prop({ context: "getString" }) getString: any;
  @Prop({ context: "ensureStyles" })
  ensureStyles: (markup: string, attributeName: string) => string;

  @Prop() formMarkup: string;
  @Prop() componentName: string;
  @Prop() isValid: boolean;
  @Prop() theme: Theme;
  @Prop() openedDialogsCount: number;

  @Event() closeModalDialog: EventEmitter;
  @Event() submitPropertiesForm: EventEmitter;

  @Watch("formMarkup")
  formMarkupChanged() {
    removeScriptElements(this.externalScriptIdentifier);
    this.renderNewMarkup();
  }

  @Listen("kenticoPropertiesDialogInputInit")
  propertiesDialogInputInit(event: CustomEvent<HTMLElement>) {
    this.addModifyFormListeners(event.detail);
  }

  get closeTooltip() {
    return this.getString(this.localizationService, "typelist.closeTooltip");
  }

  get overlayStyle() {
    return {
      zIndex: this.getZIndex(this.openedDialogsCount - 1).toString(),
    };
  }

  componentDidLoad() {
    // disable page scrolling when dialog is open
    disablePageScrolling(document);

    this.renderNewMarkup();
  }

  componentDidUnload() {
    enablePageScrolling(document);

    removeScriptElements(this.externalScriptIdentifier);
  }

  onDialogClosed = (event) => {
    event.stopPropagation();

    if (this.isModified &&
      !confirm(this.getString(this.localizationService, "modalDialog.close.confirmation"))) {
      return;
    }

    this.closeModalDialog.emit();
  }

  getZIndex(currentIndex: number) {
    return BASE_Z_INDEX + currentIndex * DIALOG_DISTANCE;
  }

  addModifyFormListeners = (el: HTMLElement) => {
    const inputElements: NodeListOf<HTMLElement> = el.querySelectorAll("input,textarea,select");

    Array.prototype.map.call(inputElements, ((element: HTMLElement) => {
      element.addEventListener("change", () => {
        this.isModified = true;
      });
    }));
  }

  addSubmitFormHandler = () => {
    const formElement = this.formWrapper.getElementsByTagName("form")[0];
    formElement.addEventListener("submit", (event: UIEvent) => {
      event.preventDefault();
      this.validateProperties(event);
    });
  }

  // Scope focus only for modal dialog i.e. it is not possible to focus outside the dialog with 'tab' key
  scopeFocus = () => {
    const formInputs = this.dialogElement.querySelectorAll("button, [href]:not(link), input, select, textarea, [tabindex]:not([tabindex=\"-1\"])");
    if (formInputs) {
      const firstElement = (formInputs[0] as HTMLElement);
      firstElement.focus();

      this.submitButton.addEventListener("focusout", () => {
        firstElement.focus();
      });
    }
  }

  renderNewMarkup = () => {
    if (!this.formMarkup) {
      return;
    }

    if (this.formWrapper.classList.contains("ktc-is-loading")) {
      this.formWrapper.classList.remove("ktc-is-loading");
    }

    renderMarkup(this.formMarkup, this.formWrapper, this.externalScriptIdentifier);
    this.addModifyFormListeners(this.formWrapper);
    this.addSubmitFormHandler();
    this.scopeFocus();
  }

  validateProperties = (event: Event) => {
    event.stopPropagation();

    const configurationForm = this.formWrapper.getElementsByTagName("form")[0];
    const formData = new FormData(configurationForm);
    this.submitPropertiesForm.emit({ configurationForm, formData });
  }

  render = (): JSX.Element =>
    (
      <div class="ktc-form-wrapper">
        <div class="ktc-form-modal-overlay" style={this.overlayStyle} />
        <div class="ktc-form-modal-dialog" ref={(el) => this.dialogElement = el}>
          <kentico-dialog-header
            ref={(el) => this.dialogHeader = el}
            headerTitle={this.componentName}
            theme={this.theme}
            closeTooltip={this.closeTooltip}
            onClose={this.onDialogClosed} />
          <div ref={(el) => this.formWrapper = el} class={`ktc-form ${this.modalDialogContentElementClass} ktc-is-loading`}>
          </div>
          <div class={`${this.modalDialogFooterClass}`}>
            <div class="ktc-modal-dialog-buttons">
              <button type="button" class="ktc-btn ktc-btn-default" onClick={this.onDialogClosed}>{this.getString(this.localizationService, "widgetproperties.cancel")}</button>
              <div class="ktc-modal-dialog-button-spacer"></div>
              <button type="button" class="ktc-btn ktc-btn-primary"
                ref={(el) => this.submitButton = el}
                title={this.getString(this.localizationService, "widgetproperties.applytooltip")}
                onClick={this.validateProperties}> {this.getString(this.localizationService, "widgetproperties.apply")} </button>
            </div>
          </div>
        </div>
      </div>
    )
}

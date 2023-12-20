import { Component, Method, Prop, State } from "@stencil/core";
import { GetString } from "../selector-types";

@Component({
  tag: "kentico-messages-placeholder",
  styleUrl: "messages-placeholder.less",
  shadow: true,
})
export class MessagesPlaceholder {
  @State() errorMessages: string[] = [];
  @State() successMessages: string[] = [];

  @Prop() getString: GetString;

  @Method()
  showError(message: string): void {
    this.errorMessages = [...this.errorMessages, message];
  }

  @Method()
  showSuccess(message: string): void {
    this.successMessages = [...this.successMessages, message];
    setTimeout(() => this.successMessages = [], 2000);
  }

  @Method()
  clear(): void {
    this.successMessages = [];
    this.errorMessages = [];
  }

  clearErrorMessages = () => {
    this.errorMessages = [];
  }

  render() {
    const renderErrorMessages = () => this.errorMessages.map((message) =>
      (
        <div>
          {message}
        </div>
      ),
    );

    const renderSuccessMessages = () => this.successMessages.map((message) =>
      (
        <div>
          {message}
        </div>
      ),
    );

    return (
      <div class={{
          "ktc-hidden": this.errorMessages.length === 0 && this.successMessages.length === 0,
          "ktc-messages-placeholder": true}}>
        <div class={{"ktc-hidden": this.successMessages.length === 0 }} >
          <div class="ktc-alert-success ktc-alert" >
            <span class="ktc-alert-icon">
              <i class="icon-check-circle"></i>
              <span class="ktc-sr-only">{this.getString("kentico.components.messagesPlaceholder.success")}</span>
            </span>
            <div class="ktc-alert-label">
              {renderSuccessMessages()}
            </div>
          </div>
        </div>

        <div class={{"ktc-hidden": this.errorMessages.length === 0 }} >
          <div class="ktc-alert-dismissable ktc-alert-error ktc-alert">
            <span class="ktc-alert-icon">
              <i class="icon-times-circle"></i>
              <span class="ktc-sr-only">{this.getString("kentico.components.messagesPlaceholder.error")}</span>
            </span>
            <div class="ktc-alert-label">
              {renderErrorMessages()}
            </div>
            <span class="ktc-alert-close" >
              <i class="ktc-close icon-modal-close" onClick={() => this.clearErrorMessages()}></i>
              <span class="ktc-sr-only">{this.getString("kentico.components.messagesPlaceholder.close")}</span>
            </span>
          </div>
        </div>
      </div >
    );
  }
}

import { Component, Event, EventEmitter, Prop } from "@stencil/core";

@Component({
  tag: "kentico-submit-button",
  styleUrl: "submit-button.less",
  shadow: true,
})
export class SubmitButton {

  @Prop() buttonText: string;

  @Prop() buttonTooltip: string;

  @Prop() buttonStyle: string;

  @Prop() disabled: boolean;

  @Event() buttonClick: EventEmitter;

  onButtonClick = (_: UIEvent) => {
    this.buttonClick.emit();
  }

  render = () =>
    <div class="ktc-submit-button-wrapper">
      <button type="button" class={this.buttonStyle || "ktc-btn ktc-btn-primary"} disabled={this.disabled}
        title={this.buttonTooltip} onClick={this.onButtonClick}> {this.buttonText} </button >
    </div>
}

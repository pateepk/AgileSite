import { Component, Prop } from "@stencil/core";

@Component({
  tag: "kentico-alert-box",
  styleUrl: "alert-box.less",
  shadow: true,
})
export class AlertBox {

  @Prop() message: string;

  render() {
    return (
      <div class="ktc-alert-box-wrapper">
        <div class="ktc-alert-box">
          <div class="ktc-alert-icon">
            <i aria-hidden="true" class="icon-times-circle"></i>
          </div>
          <div class="ktc-alert-text">
            <span>{this.message}</span>
          </div>
        </div>
      </div>
    );
  }
}

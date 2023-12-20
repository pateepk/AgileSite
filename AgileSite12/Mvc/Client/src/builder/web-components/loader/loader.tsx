import { Component, Prop, State } from "@stencil/core";

@Component({
  tag: "kentico-loader",
  styleUrl: "loader.less",
  shadow: true,
})
export class Loader {

  @Prop() loaderMessage: string;
  @Prop() delayed: boolean;

  @State() visible: boolean;

  componentWillLoad() {
    this.visible = false;
    if (this.delayed) {
      setTimeout(() => {
        this.visible = true;
      }, 1000);
    } else {
      this.visible = true;
    }
  }

  render() {
    return (
      this.visible ?
      <div class="ktc-loader-wrapper">
        <div class="ktc-overlayer ktc-overlayer-general"></div>
        <div class="ktc-loader">
          <i class="icon-spinner ktc-spinning ktc-loader-icon"></i>
          <span class="ktc-loader-text">{this.loaderMessage}</span>
        </div>
      </div> : ""
    );
  }
}

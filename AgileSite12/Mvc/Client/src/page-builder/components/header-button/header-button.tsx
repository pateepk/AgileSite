import { Component, Event, EventEmitter, Prop } from "@stencil/core";

import { ButtonIcon } from "@/page-builder/types";

@Component({
  tag: "kentico-header-button",
  styleUrl: "header-button.less",
  shadow: true,
})
export class HeaderButton {
  @Prop() buttonTooltip: string;
  @Prop() icon: ButtonIcon;

  @Event() buttonClick: EventEmitter;

  render = (): JSX.Element => {
    return (
      <div class="ktc-header-button">
        <a onClick={() => this.buttonClick.emit()}>
          <i aria-hidden="true" title={this.buttonTooltip} class={this.icon}></i>
        </a>
      </div>
    );
  }
}

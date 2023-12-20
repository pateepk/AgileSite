import { Component, Element, Event, EventEmitter, Prop } from "@stencil/core";

import { Theme } from "@/builder/types";

@Component({
  tag: "kentico-dialog-header",
  styleUrl: "dialog-header.less",
  shadow: true,
})
export class DialogHeader {
  @Element() dialogHeaderEl: HTMLElement;

  @Prop() headerTitle: string;
  @Prop() showBackButton: boolean;
  @Prop() theme: Theme;
  @Prop() closeTooltip: string;
  @Prop() backTooltip: string;

  @Event() close: EventEmitter;
  @Event() back: EventEmitter;

  onClose = (event: UIEvent) => {
    event.stopPropagation();
    this.close.emit();
  }

  onBackButtonClick = (event: UIEvent) => {
    event.stopPropagation();
    this.back.emit();
  }

  render() {
    // the color property must also be used in the markup in order to trigger the render function
    this.dialogHeaderEl.setAttribute("theme", this.theme);

    return (
      <div class={`ktc-dialog-header-wrapper ktc-theme-${this.theme}`}>
        <div class="ktc-dialog-header">
          <div class="ktc-dialog-header-controls">
            <a onClick={this.onClose}>
              <i aria-hidden="true" title={this.closeTooltip} class="icon-modal-close"></i>
            </a>
          </div>
          {
            this.showBackButton &&
            <div class="ktc-dialog-header-back">
              <a onClick={this.onBackButtonClick}>
                <i aria-hidden="true" title={this.backTooltip} class="icon-chevron-left"></i>
              </a>
            </div>
          }
          <div class={{ "ktc-dialog-header-title": true, "ktc-dialog-header-title--displaced": this.showBackButton }}>
            {this.headerTitle}
          </div>
        </div>
      </div>
    );
  }
}

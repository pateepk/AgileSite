import { Component, Element, Event, EventEmitter, Prop } from "@stencil/core";

@Component({
  tag: "kentico-add-component-button",
  styleUrl: "add-component-button.less",
  shadow: false,
})
export class AddComponentButton {
  @Element() addComponentButton: HTMLElement;

  // Changes button color theme to blue
  @Prop() primary: boolean;
  @Prop() tooltip: string;

  @Event() openComponentList: EventEmitter;

  openComponentListHandler = (event: UIEvent): void => {
    event.stopPropagation();
    this.openComponentList.emit(this.addComponentButton.getBoundingClientRect());
  }

  render() {
    return (
      <div class="ktc-component-button" onClick={this.openComponentListHandler}>
        <a class={{ "ktc-primary": this.primary, "ktc-default": !this.primary }}>
          <i aria-hidden="true" title={this.tooltip} class="icon-plus"></i>
        </a>
      </div>
    );
  }
}

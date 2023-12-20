import { Component, Element, Event, EventEmitter, Prop } from "@stencil/core";

import { LocalizationService } from "@/builder/declarations/index";
import { Theme } from "@/builder/types";

@Component({
  tag: "kentico-pop-up-container",
  styleUrl: "pop-up-container.less",
  shadow: false,
})
export class PopupContainer {

  @Element() popupContainerEl: HTMLElement;

  @Prop() localizationService: LocalizationService;
  @Prop() headerTitle: string;
  @Prop() position: string;
  // Changes pop up color theme to blue
  @Prop() theme: Theme;
  @Prop() showBackButton: boolean;
  @Prop({ context: "getString" }) getString: any;

  @Event() closePopup: EventEmitter;
  @Event() backClick: EventEmitter;

  get closeTooltip() {
    return this.getString(this.localizationService, "typelist.closeTooltip");
  }

  get backTooltip() {
    return this.getString(this.localizationService, "variant.back");
  }

  onClosePopupContainer = (event: CustomEvent) => {
    event.stopPropagation();
    this.closePopup.emit(event);
  }

  onBackButtonClick = (event: CustomEvent) => {
    event.stopPropagation();
    this.backClick.emit(event);
  }

  render() {
    this.popupContainerEl.setAttribute("position", this.position);
    this.popupContainerEl.setAttribute("theme", this.theme);

    return (
      <div class="ktc-pop-up" onClick={(e) => e.stopPropagation()}>
        <kentico-dialog-header
          headerTitle={this.headerTitle}
          theme={this.theme}
          showBackButton={this.showBackButton}
          closeTooltip={this.closeTooltip}
          backTooltip={this.backTooltip}
          onClose={this.onClosePopupContainer}
          onBack={this.onBackButtonClick} />
        <div class="ktc-pop-up-content">
          <slot name="pop-up-content" />
        </div>

        <slot name="pop-up-footer" />
      </div>
    );
  }
}

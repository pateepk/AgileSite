import { Component, Event, EventEmitter, Prop } from "@stencil/core";

import { LocalizationService, PopupListingElement } from "@/builder/declarations/index";
import { ButtonType, Theme } from "@/builder/types";

@Component({
  tag: "kentico-section-header",
  styleUrl: "section-header.less",
  shadow: false,
})
export class SectionHeader {

  @Prop() localizationService: LocalizationService;
  @Prop() sectionTypes: PopupListingElement[];
  @Prop() sectionTypeListPosition: string;
  @Prop() activeSectionTypeIdentifier: string;
  @Prop() sectionTypeListTitleText: string;
  @Prop({ context: "getString" }) getString: any;

  @Event() removeSection: EventEmitter;
  @Event() toggleSectionTypeList: EventEmitter;

  get dragTooltip() {
    return this.getString(this.localizationService, "section.dragTooltip");
  }

  get deleteTooltip() {
    return this.getString(this.localizationService, "section.deleteTooltip");
  }

  removeSectionHandler = (event: UIEvent) => {
    event.stopPropagation();
    this.removeSection.emit();
  }

  toggleSectionTypeListHandler = () => {
    this.toggleSectionTypeList.emit();
  }

  render() {
    return (
      <div class="ktc-section-header">
        <div class="ktc-section-header-drag-icon" button-type={ButtonType.Move} onMouseDown={(e) => e.preventDefault()}>
          <i aria-hidden="true" title={this.dragTooltip} class="icon-dots-vertical"></i>
        </div>
        <div class="ktc-section-header-layout-icon" button-type={ButtonType.Change}>
          <i aria-hidden="true" title={this.sectionTypeListTitleText} class="icon-l-cols-30-70" onClick={this.toggleSectionTypeListHandler}></i>
        </div>
        {this.sectionTypeListPosition &&
          <kentico-pop-up-container
            class="ktc-section-type-list"
            localizationService={this.localizationService}
            headerTitle={this.sectionTypeListTitleText}
            theme={Theme.Section}
          >
            <kentico-pop-up-listing slot="pop-up-content"
              items={this.sectionTypes}
              activeItemIdentifier={this.activeSectionTypeIdentifier}
            />
          </kentico-pop-up-container>
        }
        <slot name="sectionHeaderExtraButtons" />
        <div class="ktc-section-header-bin-icon" button-type={ButtonType.Delete}>
          <i aria-hidden="true" title={this.deleteTooltip} class="icon-bin" onClick={this.removeSectionHandler}></i>
        </div>
      </div>
    );
  }
}
